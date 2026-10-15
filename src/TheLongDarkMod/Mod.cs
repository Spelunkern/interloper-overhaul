using MelonLoader;
using TheLongDarkMod;

[assembly: MelonInfo(typeof(Mod), "The Long Dark Mod", "0.1.0", "Spelunkern")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace TheLongDarkMod;

public sealed class Mod : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Development scaffold loaded successfully. Version 0.1.0.");
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        LoggerInstance.Msg($"Scene initialized: {sceneName} ({buildIndex})");
    }
}
