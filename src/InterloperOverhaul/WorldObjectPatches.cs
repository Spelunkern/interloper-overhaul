extern alias GameAssembly;

using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;

namespace InterloperOverhaul;

[HarmonyPatch(typeof(Game.DisableObjectForXPMode), nameof(Game.DisableObjectForXPMode.ShouldDisableForCurrentMode))]
internal static class RestoreDisabledWorldObjects
{
    private static void Postfix(Game.DisableObjectForXPMode __instance, ref bool __result)
    {
        if (__result && WorldObjects.IsInterloper &&
            __instance.m_XPModesToDisable.Contains(Game.ExperienceModeType.Interloper) &&
            WorldObjects.IsRestorable(__instance.gameObject))
            __result = false;
    }
}

[HarmonyPatch(typeof(Game.EnableObjectForXPMode), nameof(Game.EnableObjectForXPMode.ShouldDisableForCurrentMode))]
internal static class RestoreEnabledWorldObjects
{
    private static void Postfix(Game.EnableObjectForXPMode __instance, ref bool __result)
    {
        if (__result && WorldObjects.IsInterloper &&
            __instance.m_XPModesToEnable.Contains(Game.ExperienceModeType.Stalker) &&
            WorldObjects.IsRestorable(__instance.m_ObjectToEnable != null ?
                __instance.m_ObjectToEnable : __instance.gameObject))
            __result = false;
    }
}

[HarmonyPatch(typeof(Gameplay.DisableObjectForGameMode), nameof(Gameplay.DisableObjectForGameMode.ShouldDisableForCurrentMode))]
internal static class RestoreTaggedWorldObjects
{
    private static void Postfix(Gameplay.DisableObjectForGameMode __instance, ref bool __result)
    {
        if (!__result || !WorldObjects.IsInterloper || !WorldObjects.IsRestorable(__instance.gameObject)) return;
        var mode = Game.ExperienceModeManager.GetCurrentGameModeConfig();
        bool restrictedByGear = false;
        foreach (var tag in __instance.m_DisableIfAnyActive)
        {
            if (!mode.HasActiveTag(tag)) continue;
            // Ignore only gear restrictions on world containers / collectibles.
            // Preserve any unrelated active tag, including mission and DLC conditions.
            if (!tag.MatchesTagExact("Gameplay.Gear.NoFirearms") && !tag.MatchesTagExact("Gameplay.Gear.NoRifle")) return;
            restrictedByGear = true;
        }
        if (restrictedByGear) __result = false;
    }
}

[HarmonyPatch(typeof(Game.RandomSpawnObject), nameof(Game.RandomSpawnObject.GetNumObjectsToEnableCurrentXPMode))]
internal static class WorldObjectCounts
{
    private static void Postfix(Game.RandomSpawnObject __instance, ref int __result)
    {
        if (!WorldObjects.IsInterloper) return;
        if (WorldObjects.IsHouseSpawner(__instance))
        {
            __result = __instance.name == WorldPolicy.NightmareSpawner ? 0 : 1;
            return;
        }
        // Raise only groups made entirely of containers / collectibles; never gear or wildlife groups.
        var objects = __instance.m_ObjectList;
        if (objects == null || objects.Length == 0) return;
        foreach (var go in objects)
            if (!WorldObjects.IsRestorable(go)) return;
        var counts = __instance.m_NumObjectsToEnableByLevel;
        if (counts != null && counts.Length > 1)
            __result = System.Math.Max(__result, counts[1]); // Medium base resources (Stalker).
    }
}
