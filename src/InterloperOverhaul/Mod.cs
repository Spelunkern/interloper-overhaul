using MelonLoader;
using InterloperOverhaul;

[assembly: MelonInfo(typeof(Mod), "Interloper Overhaul", "0.2.0", "Spelunkern", "https://github.com/Spelunkern/interloper-overhaul")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace InterloperOverhaul;

public sealed class Mod : MelonMod
{
#if IO_VALIDATION
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        if (sceneName.StartsWith("MainMenu", System.StringComparison.Ordinal))
            RuntimeValidation.Run();
    }
#endif

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Interloper Overhaul 0.2.0 loaded. World restoration patches enabled for Interloper.");
    }
}
