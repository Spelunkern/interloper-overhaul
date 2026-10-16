extern alias GameAssembly;

using System.Globalization;
using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
using UnityEngine;

namespace InterloperOverhaul;

internal static class LootBalance
{
    internal static bool Active => WorldObjects.IsInterloper || RewardScope.Active;
    private static readonly Dictionary<int, GameObject> restoredObjects = new();

    internal static void MarkRestored(GameObject go) => restoredObjects[go.GetInstanceID()] = go;

    internal static void Prune()
    {
        foreach (int id in restoredObjects.Where(pair => pair.Value == null).Select(pair => pair.Key).ToArray())
            restoredObjects.Remove(id);
    }

    // Read authored restrictions rather than invoking patched eligibility methods.
    // Remember scene exclusions before Update removes their disabler components.
    internal static bool HasRestoredRestriction(GameObject go)
    {
        for (Transform node = go.transform; node != null; node = node.parent)
        {
            if (restoredObjects.TryGetValue(node.gameObject.GetInstanceID(), out var marked) && marked == node.gameObject)
                return true;
            foreach (var xp in node.GetComponents<Game.DisableObjectForXPMode>())
                if (xp.m_XPModesToDisable != null && xp.m_XPModesToDisable.Contains(Game.ExperienceModeType.Interloper))
                    return true;
            foreach (var xp in node.GetComponents<Game.EnableObjectForXPMode>())
                if (xp.m_XPModesToEnable != null && !xp.m_XPModesToEnable.Contains(Game.ExperienceModeType.Interloper) &&
                    xp.m_XPModesToEnable.Contains(Game.ExperienceModeType.Stalker)) return true;
            foreach (var tagged in node.GetComponents<Gameplay.DisableObjectForGameMode>())
            {
                if (tagged.m_DisableIfAnyActive == null) continue;
                foreach (var tag in tagged.m_DisableIfAnyActive)
                    if (tag.MatchesTagExact("Gameplay.Gear.NoFirearms") || tag.MatchesTagExact("Gameplay.Gear.NoRifle") ||
                        tag.MatchesTagExact("Gameplay.Gear.NoRevolver")) return true;
            }
        }
        return false;
    }

    internal static double Retention(GameObject go)
    {
        if (!Active || go == null || !WorldObjects.IsRestorable(go)) return 1;
        var gear = go.GetComponent<Game.GearItem>();
        if (gear == null || gear.m_BeenInPlayerInventory || gear.m_HasBeenOwnedByPlayer || gear.m_BeenInContainer) return 1;
        string name = LootBalancePolicy.ItemName(go.name);
        if (Workshop.Card(gear) != null || (LootBalancePolicy.IsWeaponVariant(name) && Workshop.Authored(gear))) return 1;
        if (RecoveredCasings.Active && WorldPolicy.IsSpentCasing(name)) return 1;
        if (FirearmSpawnPolicy.IsFiniteItem(name)) return FiniteFirearms.IsManaged(gear) ? 1 : 0;
        if (WorldPolicy.IsTaleReward(name) ||
            WorldPolicy.IsSignalFireSatchel(go.scene.name, WorldObjects.PathOf(go))) return 1;
        // Do not substitute canonical prefab restrictions for a scene instance.
        // Native Interloper, for example, allows placed hammers while excluding
        // hammers from ordinary container loot. Only newly restored sources change.
        return LootBalancePolicy.Retention(name, HasRestoredRestriction(go));
    }

    internal static string Key(GameObject source)
    {
        var p = source.transform.position;
        return source.scene.name + "/" + WorldObjects.PathOf(source) + "/" +
            p.x.ToString("R", CultureInfo.InvariantCulture) + "," +
            p.y.ToString("R", CultureInfo.InvariantCulture) + "," + p.z.ToString("R", CultureInfo.InvariantCulture);
    }

    internal static bool Keep(GameObject item, string key, int slot) =>
        LootBalancePolicy.Sample(Game.GameManager.GetRandomSeed(0), key + "/" + LootBalancePolicy.ItemName(item.name), slot) < Retention(item);

    internal static string RadialKey(Game.RadialObjectSpawner spawner) => Key(spawner.gameObject) + "/" +
        spawner.m_GameHourTimeToRespawn.ToString("R", CultureInfo.InvariantCulture);

    internal static void FilterPending(Game.Container container, int first)
    {
        var pending = container.m_GearToInstantiate;
        if (!Active || pending == null) return;
        string key = Key(container.gameObject);
        // Never touch m_Items: it includes saved loot and player storage.
        for (int i = pending.Count - 1; i >= first; i--)
        {
            var variant = pending[i] == null ? null : VariantPolicy.Find(pending[i].name);
            if (variant != null && RewardScope.IsRewardContainer(container) && !Workshop.Failed)
            {
                // Keep the selected reward opportunity, but deliver its schematic.
                var card = Workshop.MakeCard(variant, System.Guid.NewGuid().ToString());
                container.AddGear(card);
                pending.RemoveAt(i);
                continue;
            }
            if (pending[i]!=null && MementoRewardPolicy.IsCache(WorldObjects.PathOf(container.gameObject)) &&
                !WorldPolicy.IsTaleReward(LootBalancePolicy.ItemName(pending[i].name)))
            {
                var replacement=Game.GearItem.LoadGearItemPrefab(MementoRewardPolicy.Choose(Game.GameManager.GetRandomSeed(0),key,i));
                if (replacement!=null) { pending[i]=replacement.gameObject; continue; }
            }
            if (pending[i] != null && !Keep(pending[i], key, i)) pending.RemoveAt(i);
        }
    }
}

[HarmonyPatch]
internal static class RememberRestoredGear
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem), nameof(Game.GearItem.Awake));
    private static void Prefix(Game.GearItem __instance)
    {
        if (LootBalance.Active && LootBalance.HasRestoredRestriction(__instance.gameObject))
            LootBalance.MarkRestored(__instance.gameObject);
    }

    private static void Postfix(Game.GearItem __instance)
    {
        RecoveredCasings.Preserve(__instance);
        Manufacturing.PreserveOutput(__instance);
    }
}

[HarmonyPatch]
internal static class RareWorldGear
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem), nameof(Game.GearItem.RollSpawnChance));

    private static bool Prefix(Game.GearItem __instance)
    {
        if (!LootBalance.Active || __instance.m_RolledSpawnChance || __instance.m_BeenInPlayerInventory ||
            __instance.m_BeenInContainer || __instance.m_HasBeenOwnedByPlayer ||
            __instance.m_RadialObjectSpawnerParent != null) return true;
        if (LootBalance.Keep(__instance.gameObject, LootBalance.Key(__instance.gameObject), 0)) return true;
        __instance.m_RolledSpawnChance = true;
        // Use the native destruction/serialization path for a failed world spawn.
        Game.GameManager.RollSpawnChance(__instance.gameObject, 0);
        return false;
    }
}

[HarmonyPatch]
internal static class RareContainerGear
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Container), nameof(Game.Container.PopulateWithRandomGear));
        yield return AccessTools.Method(typeof(Game.Container), nameof(Game.Container.PopulateWithScriptedGear));
    }

    private static void Prefix(Game.Container __instance, out int __state) =>
        __state = LootBalance.Active && __instance.m_GearToInstantiate != null ? __instance.m_GearToInstantiate.Count : -1;

    private static void Postfix(Game.Container __instance, int __state)
    {
        if (__state >= 0) LootBalance.FilterPending(__instance, __state);
    }
}

[HarmonyPatch]
internal static class RareRadialGear
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.RadialObjectSpawner), nameof(Game.RadialObjectSpawner.SpawnObjectAttempt));

    internal static void Prefix(Game.RadialObjectSpawner __instance, out int __state) =>
        // Pending spawns are saved finds being restored, not new loot opportunities.
        __state = LootBalance.Active && __instance.m_PendingSpawns.Count == 0 ? __instance.m_Spawns.Count : -1;

    internal static void Postfix(Game.RadialObjectSpawner __instance, int __state)
    {
        if (__state < 0 || !LootBalance.Active) return;
        // Include the freshly scheduled respawn time so later renewable cycles do
        // not repeat one permanent decision for this spawner/item/count combination.
        string key = LootBalance.RadialKey(__instance);
        for (int i = __instance.m_Spawns.Count - 1; i >= __state; i--)
        {
            var go = __instance.m_Spawns[i];
            if (go == null || LootBalance.Retention(go) >= 1) continue;
            // The opportunity has already consumed the native count and respawn timer.
            // Its native radial parent lets RollSpawnChance preserve the original
            // gear chance later without applying our rarity filter a second time.
            if (LootBalance.Keep(go, key, __instance.m_NumToSpawn)) continue;
            __instance.m_Spawns.RemoveAt(i);
            UnityEngine.Object.Destroy(go);
        }
    }
}
