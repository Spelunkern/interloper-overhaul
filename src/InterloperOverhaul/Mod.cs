using MelonLoader;
using InterloperOverhaul;

[assembly: MelonInfo(typeof(Mod), "Interloper Overhaul", "0.8.0", "Spelunkern", "https://github.com/Spelunkern/interloper-overhaul")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace InterloperOverhaul;

public sealed class Mod : MelonMod
{
    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) => LootBalance.Prune();
#if IO_VALIDATION
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        if (sceneName.StartsWith("MainMenu", System.StringComparison.Ordinal))
            MelonCoroutines.Start(RuntimeValidation.WaitForMenu());
    }
#endif

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Interloper Overhaul 0.8.0 loaded. Restored loot retention: 5% ordinary, 1% premium, 0.25% special variants. Direct main menu enabled globally.");
    }
}
