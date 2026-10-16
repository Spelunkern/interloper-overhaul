using MelonLoader;
using InterloperOverhaul;

[assembly: MelonInfo(typeof(Mod), "Interloper Overhaul", "0.12.0", "Spelunkern", "https://github.com/Spelunkern/interloper-overhaul")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace InterloperOverhaul;

public sealed class Mod : MelonMod
{
    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) => LootBalance.Prune();
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
#if IO_VALIDATION
        // Keep isolated menu probes advancing when their window loses focus.
        UnityEngine.Application.runInBackground = true;
        if (sceneName.StartsWith("MainMenu", System.StringComparison.Ordinal))
            MelonCoroutines.Start(RuntimeValidation.WaitForMenu());
#endif
        if (FirearmSpawnPolicy.Sites.Any(site => site.Scene == sceneName))
            MelonCoroutines.Start(FiniteFirearms.WaitForWorld());
    }

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Interloper Overhaul 0.12.0 loaded. One allocated rifle, revolver, hunting knife, and hatchet per new Interloper run; found ammo retention 0.25%, casings 0.5%. Weapon variants disabled. Native cannery manufacturing enabled in Interloper. Direct main menu enabled globally.");
    }
}
