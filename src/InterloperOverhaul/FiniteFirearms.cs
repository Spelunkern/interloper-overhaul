extern alias GameAssembly;

using System.Reflection;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Game = GameAssembly::Il2Cpp;
using Trader = GameAssembly::Il2CppTLD.Trader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterloperOverhaul;

internal static class FiniteFirearms
{
    internal const string SaveKey = "interloper-overhaul-finite-firearms-v1";
    internal static FirearmRunState State;
    private static readonly HashSet<int> deserialized = new();
    private static readonly Dictionary<int, int> initialConditions = new();
    private static bool failedLoad;
    internal static bool AllocationBlocked => failedLoad;
    internal static string RunKey => Game.SaveGameSystem.m_CurrentEpisode + "/" + Game.SaveGameSystem.m_CurrentGameId;

    internal static void Reset()
    {
        State = null; failedLoad = false; deserialized.Clear(); initialConditions.Clear(); Workshop.Reset();
    }

    internal static void ReadSlot(Game.SlotData slot)
    {
        State = null; failedLoad = false;
        try
        {
            if (slot != null && slot.HasData(SaveKey))
            {
                // SlotData stores opaque byte payloads. Own our key's encoding and
                // avoid uninstantiated native generic TryGetData<string> wrappers.
                State = FirearmRunState.Deserialize(Encoding.UTF8.GetString(slot.m_Dict[SaveKey].ToArray()));
            }
            else
            {
                State = FirearmRunState.Create(Game.GameManager.GetRandomSeed(0), RunKey);
                // A pre-mod save can already contain unknown guns in unloaded scenes.
                // Preserve them without allocating new copies or pretending to migrate it.
                State.Legacy = slot != null && slot.HasData(Game.SlotData.GLOBAL_KEY);
                if (State.Legacy)
                    MelonLogger.Msg("[Finite firearms] Existing save preserved. Start a new Interloper game for exactly two allocated rifles, revolvers, hunting knives, and hatchets.");
            }
        }
        catch (Exception ex)
        {
            failedLoad = true;
            MelonLogger.Error("[Finite firearms] Saved allocation could not be loaded; spawning is disabled to prevent duplicates. " + ex.Message);
        }
    }

    internal static bool WriteSlot(Game.SlotData slot)
    {
        if (failedLoad || State == null || slot == null) return false;
        slot.Lock();
        try { slot.m_Dict[SaveKey] = new Il2CppStructArray<byte>(Encoding.UTF8.GetBytes(State.Serialize())); }
        finally { slot.UnLock(); }
        return true;
    }

    internal static void RememberDeserialized(Game.GearItem gear)
    {
        if (FirearmSpawnPolicy.IsFiniteItem(gear.name) || LootBalancePolicy.IsWeaponVariant(gear.name)) deserialized.Add(gear.GetInstanceID());
    }

    internal static void AfterStart(Game.GearItem gear)
    {
        Workshop.InitializeCard(gear);
        if (WorkshopConversions.Record(gear.m_InProgressCraftItem) is ConversionState work)
        { gear.SetNormalizedHP(work.Condition,false); Empty(gear); return; }
        int id = gear.GetInstanceID();
        if (initialConditions.Remove(id, out int condition))
        {
            gear.SetNormalizedHP(condition / 100f, false);
            Empty(gear);
            return;
        }
        bool variant = LootBalancePolicy.IsWeaponVariant(gear.name);
        if (!LootBalance.Active || (!variant && !FirearmSpawnPolicy.IsFiniteItem(gear.name))) return;
        if (Game.SaveGameSystem.IsRestoreInProgress() || Game.SaveGameSystem.IsSceneRestoreInProgress()) return;
        if (deserialized.Remove(id) || gear.m_BeenInPlayerInventory || gear.m_HasBeenOwnedByPlayer ||
            gear.m_BeenInContainer || IsManaged(gear)) return;
        if (!variant && FirearmSpawnPolicy.IsStandardFirearm(gear.name) && State?.Legacy == true) return;
        if (variant && Workshop.ReplaceWorldVariant(gear)) return;
        // Catch fresh weapons whose native spawner skipped RollSpawnChance entirely.
        gear.gameObject.SetActive(false);
    }

    internal static bool IsManaged(Game.GearItem gear)
    {
        if (State == null || !FirearmSpawnPolicy.IsFiniteItem(gear.name)) return false;
        string guid = Game.ObjectGuid.GetGuidFromGameObject(gear.gameObject);
        if (LightSpawnPolicy.IsLight(gear.name)) return State.Lights.Any(light => light.Item == LootBalancePolicy.ItemName(gear.name) && guid == State.GuidFor("light/" + light.Key));
        return Enumerable.Range(0,FirearmSpawnPolicy.CopiesPerRun).Any(copy=>guid==State.GuidFor(LootBalancePolicy.ItemName(gear.name),copy));
    }

    internal static void Empty(Game.GearItem gear)
    {
        var gun = gear.m_GunItem ?? gear.GetComponent<Game.GunItem>();
        if (gun == null) return;
        gun.m_RoundsInClip = 0;
        gun.EmptyClip();
        gun.ClearSpentCasings();
    }

    internal static Game.GearItem CreateWeapon(string item, FirearmSite site, Scene scene, LightAllocation light = null, int copy = 0)
    {
        var gear = Game.GearItem.InstantiateGearItem(item);
        if (gear == null) throw new InvalidOperationException("Could not instantiate " + item);
        try
        {
            SceneManager.MoveGameObjectToScene(gear.gameObject, scene);
            var source = Resources.FindObjectsOfTypeAll<Game.GearItem>().FirstOrDefault(candidate =>
                candidate.Pointer != gear.Pointer && candidate.gameObject.scene == scene &&
                WorldObjects.PathOf(candidate.gameObject) == site.Path);
            gear.transform.position = source != null ? source.transform.position : new Vector3(site.X, site.Y, site.Z);
            gear.transform.rotation = source != null ? source.transform.rotation : Quaternion.identity;
            Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(gear.gameObject, light==null ? State.GuidFor(item,copy) : State.GuidFor("light/"+light.Key));
            gear.m_DisableSerialization = false;
            gear.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            int condition = FirearmSpawnPolicy.Condition(State.Seed, item,copy);
            initialConditions[gear.GetInstanceID()] = condition;
            gear.SetNormalizedHP(condition / 100f, false);
            Empty(gear); // Also zero the legacy field before GunItem.Start can fill the clip.
            if (light == null) State.SetCreated(item,copy,true); else light.Created = true;
            return gear;
        }
        catch
        {
            UnityEngine.Object.Destroy(gear.gameObject);
            throw;
        }
    }

    internal static void PopulateLoadedScenes()
    {
        if (!WorldObjects.IsInterloper || Game.GameManager.s_ActiveIsMainMenu ||
            Game.SaveGameSystem.IsRestoreInProgress() || Game.SaveGameSystem.IsSceneRestoreInProgress() || failedLoad) return;
        if (State == null) State = FirearmRunState.Create(Game.GameManager.GetRandomSeed(0), RunKey);
        if (State.Legacy) return;
        PopulateLights();
        foreach (string item in FirearmSpawnPolicy.Items)
        for(int copy=0;copy<FirearmSpawnPolicy.CopiesPerRun;copy++)
        {
            if (State.IsLegacy(item) || State.Created(item,copy)) continue;
            var site = State.Site(item,copy);
            var scene = SceneManager.GetSceneByName(site.Scene);
            if (!scene.IsValid() || !scene.isLoaded) continue;
            try { CreateWeapon(item,site,scene,copy:copy); }
            catch (Exception ex) { MelonLogger.Error("[Finite firearms] Failed to place allocated item: " + ex); }
        }
    }

    internal static void PopulateLights()
    {
        if (State == null || State.Legacy || State.LightsLegacy) return;
        foreach (var light in State.Lights)
        {
            if (light.Created) continue;
            var scene = SceneManager.GetSceneByName(light.Site.Scene);
            if (scene.IsValid() && scene.isLoaded) CreateWeapon(light.Item, light.Site, scene, light);
        }
    }

    internal static System.Collections.IEnumerator WaitForWorld()
    {
        // Scene initialization can precede the native save/scene restoration work.
        // Retry after it finishes without doing game-save work on every menu frame.
        for (int frame = 0; frame < 600; frame++)
        {
            yield return null;
            if (frame % 30 == 0) PopulateLoadedScenes();
        }
    }
}

[HarmonyPatch]
internal static class NewFirearmAllocation
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GameManager), nameof(Game.GameManager.LaunchSandbox));
    private static void Prefix() => FiniteFirearms.Reset();
}

[HarmonyPatch]
internal static class RestoreFirearmAllocation
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem), nameof(Game.SaveGameSystem.RestoreGlobalData));
    private static void Postfix(string name, bool __result)
    {
        if (!__result || !WorldObjects.IsInterloper) return;
        FiniteFirearms.ReadSlot(Game.SaveGameSlots.GetSaveSlotFromName(name));
    }
}

[HarmonyPatch]
internal static class SaveFirearmAllocation
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem), nameof(Game.SaveGameSystem.SaveGlobalData));
    private static void Postfix(Game.SlotData slot)
    {
        if (!WorldObjects.IsInterloper) return;
        if (FiniteFirearms.AllocationBlocked) return;
        if (FiniteFirearms.State == null) FiniteFirearms.ReadSlot(null);
        if (!FiniteFirearms.WriteSlot(slot))
            MelonLogger.Error("[Finite firearms] Allocation could not be attached to the current save snapshot.");
    }
}

[HarmonyPatch]
internal static class PreserveSavedFirearms
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem), nameof(Game.GearItem.Deserialize));
    private static void Postfix(Game.GearItem __instance) => FiniteFirearms.RememberDeserialized(__instance);
}

[HarmonyPatch]
internal static class InitializeAllocatedFirearms
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem), nameof(Game.GearItem.ManualStart));
    private static void Postfix(Game.GearItem __instance) => FiniteFirearms.AfterStart(__instance);
}

[HarmonyPatch]
internal static class NoAdditionalStandardFirearmTrades
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Trader.TradeExchangeData), nameof(Trader.TradeExchangeData.IsValidInCurrentGamemode));
    private static void Postfix(Trader.TradeExchangeData __instance, ref bool __result)
    {
        if (!__result || !LootBalance.Active || __instance.m_GainedItem == null) return;
        string item = LootBalancePolicy.TraderItem(__instance.m_GainedItem.AssetGUID);
        if (FirearmSpawnPolicy.IsFiniteItem(item)) __result = false;
    }
}
