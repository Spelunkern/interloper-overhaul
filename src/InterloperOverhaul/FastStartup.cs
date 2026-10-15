extern alias GameAssembly;

using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using MelonLoader;

namespace InterloperOverhaul;

internal static class FastStartup
{
    internal static void SkipBootPresentation(Game.Panel_Boot panel)
    {
        // The native Update still handles loading completion and optional-content errors.
        // ReadyToActivateScene stays under the game's control until loading completes.
        if (panel.m_ErrorFlow != null && panel.m_ErrorFlow.activeSelf) return;
        panel.m_StartShowingDisclaimers = false;
        panel.m_HoldLoadingUntilInputGiven = false;
        if (panel.m_BootFlow != null) panel.m_BootFlow.SetActive(false);
        if (panel.m_MainDisclaimer != null) panel.m_MainDisclaimer.SetActive(false);
        if (panel.m_DisclaimerScreenList != null)
            foreach (var screen in panel.m_DisclaimerScreenList)
                if (screen != null) screen.SetActive(false);
        if (panel.m_LoadingFlow != null) panel.m_LoadingFlow.SetActive(true);
    }

    internal static void SkipIntro(Game.Panel_MainMenu menu)
    {
        // Scope this to the main menu intro, not MoviePlayer.Play (story cinematics).
        Game.MoviePlayer.m_HasIntroPlayedForMainMenu = true;
        menu.m_PlayedIntroMovie = true;
    }
}

[HarmonyPatch(typeof(Game.Panel_Boot), nameof(Game.Panel_Boot.Enable))]
internal static class SkipStartupScreens
{
    private static void Postfix(Game.Panel_Boot __instance, bool enable)
    {
        if (!enable) return;
        FastStartup.SkipBootPresentation(__instance);
        MelonLogger.Msg("[Startup] Skipped boot logos and disclaimers; loading continues without input.");
    }
}

[HarmonyPatch(typeof(Game.Panel_Boot), nameof(Game.Panel_Boot.Update))]
internal static class SkipDelayedDisclaimers
{
    private static void Prefix(Game.Panel_Boot __instance)
    {
        // A delayed boot animation callback can request disclaimers after Enable.
        if (__instance.m_StartShowingDisclaimers)
            FastStartup.SkipBootPresentation(__instance);
    }
}

[HarmonyPatch]
internal static class PrepareDirectMainMenu
{
    private static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Panel_MainMenu), nameof(Game.Panel_MainMenu.Initialize));
        yield return AccessTools.Method(typeof(Game.Panel_MainMenu), nameof(Game.Panel_MainMenu.Enable));
    }

    private static void Prefix(Game.Panel_MainMenu __instance) => FastStartup.SkipIntro(__instance);

    private static void Postfix(Game.Panel_MainMenu __instance)
    {
        FastStartup.SkipIntro(__instance);
        __instance.m_InitialScreenFadeInDuration = 0.01f;
        Game.Panel_MainMenu.m_AdjustedFadeInDuration = 0.01f;
        MelonLogger.Msg("[Startup] Main menu ready; intro movie skipped.");
    }
}

[HarmonyPatch(typeof(Game.Panel_MainMenu), nameof(Game.Panel_MainMenu.PlayIntroMovie))]
internal static class SkipMainMenuIntroMovie
{
    private static bool Prefix(Game.Panel_MainMenu __instance)
    {
        FastStartup.SkipIntro(__instance);
        return false;
    }
}
