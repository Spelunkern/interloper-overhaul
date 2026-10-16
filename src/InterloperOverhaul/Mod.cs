using MelonLoader;
using InterloperOverhaul;

[assembly: MelonInfo(typeof(Mod), "Interloper Overhaul", "0.9.0", "Spelunkern", "https://github.com/Spelunkern/interloper-overhaul")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace InterloperOverhaul;

public sealed class Mod : MelonMod
{
    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) => LootBalance.Prune();
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
#if IO_VALIDATION
        if (sceneName.StartsWith("MainMenu", System.StringComparison.Ordinal))
            MelonCoroutines.Start(RuntimeValidation.WaitForMenu());
#endif
        if (FirearmSpawnPolicy.Sites.Any(site => site.Scene == sceneName))
            MelonCoroutines.Start(FiniteFirearms.WaitForWorld());
    }

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Interloper Overhaul 0.9.0 loaded. One allocated rifle and revolver per new Interloper run; found ammo retention 0.25%, casings 0.5%. Direct main menu enabled globally.");
    }
}
