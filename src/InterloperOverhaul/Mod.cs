using MelonLoader;
using InterloperOverhaul;

[assembly: MelonInfo(typeof(Mod), "Interloper Overhaul", "0.7.0", "Spelunkern", "https://github.com/Spelunkern/interloper-overhaul")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace InterloperOverhaul;

public sealed class Mod : MelonMod
{
#if IO_VALIDATION
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        if (sceneName.StartsWith("MainMenu", System.StringComparison.Ordinal))
            MelonCoroutines.Start(RuntimeValidation.WaitForMenu());
    }
#endif

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Interloper Overhaul 0.7.0 loaded. World and Tales reward restoration enabled for Interloper; direct main menu enabled globally.");
    }
}
