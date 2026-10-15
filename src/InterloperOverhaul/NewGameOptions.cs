extern alias GameAssembly;

using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;

namespace InterloperOverhaul;

[HarmonyPatch(typeof(Game.ExperienceModeManager), nameof(Game.ExperienceModeManager.GetGameModeFromName))]
internal static class NewGameOptions
{
    private static void Postfix(Gameplay.GameModeConfig __result)
    {
        // Check the actual config, including lookups before a survival game is active.
        // Custom and Misery must retain their own setup and save identity.
        if (__result == null || __result.name != "Interloper") return;
        var config = __result.TryCast<Gameplay.SandboxConfig>();
        if (config == null) return;
        var normal = Game.ExperienceModeManager.GetGameModeFromName("Stalker").Cast<Gameplay.SandboxConfig>();
        config.m_StartRegionSelectionBlocked = false;
        config.m_AvailableStartRegions = normal.m_AvailableStartRegions;
        config.m_NumFeats = 3;
        // Remove the gear bans themselves as well as the scene-level XP filters.
        // This covers native loot tables, prefab validation, and trader stock.
        // Keep every unrelated tag (DLC, Tales, safehouses, and mission gates).
        for (int i = config.m_ActiveTags.Count - 1; i >= 0; i--)
        {
            var tag = config.m_ActiveTags[i];
            if (tag.MatchesTagExact("Gameplay.Gear.NoFirearms") || tag.MatchesTagExact("Gameplay.Gear.NoRifle") ||
                tag.MatchesTagExact("Gameplay.Gear.NoRevolver")) config.m_ActiveTags.RemoveAt(i);
        }
    }
}

[HarmonyPatch(typeof(Game.Panel_SelectExperience), nameof(Game.Panel_SelectExperience.OnExperienceClicked))]
internal static class PrepareNewGameOptions
{
    private static void Prefix()
    {
        // Menu items hold direct references to the configs. Prepare the Interloper
        // asset before the native handler chooses the next panel (region / survivor).
        Game.ExperienceModeManager.GetGameModeFromName("Interloper");
    }
}
