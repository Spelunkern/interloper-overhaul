extern alias GameAssembly;
using MelonLoader;
using InterloperOverhaul;

[assembly: MelonInfo(typeof(Mod), "Interloper Overhaul", "0.20.0", "Spelunkern", "https://github.com/Spelunkern/interloper-overhaul")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace InterloperOverhaul;

public sealed class Mod : MelonMod
{
    public override void OnSceneWasUnloaded(int buildIndex, string sceneName) => LootBalance.Prune();
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        if (WorldObjects.IsInterloper)
        {
            RockCacheBalance.Manager(GameAssembly::Il2Cpp.GameManager.GetRockCacheManager());
            var vehicle=GameAssembly::Il2Cpp.GameManager.GetPlayerInVehicle();
            if (vehicle!=null) VehicleBalance.Temperature(vehicle);
        }
#if IO_VALIDATION
        // Keep isolated menu probes advancing when their window loses focus.
        UnityEngine.Application.runInBackground = true;
        if (sceneName.StartsWith("MainMenu", System.StringComparison.Ordinal))
            MelonCoroutines.Start(RuntimeValidation.WaitForMenu());
#endif
        if (sceneName.Contains("Region",StringComparison.Ordinal) || sceneName.Contains("SANDBOX",StringComparison.Ordinal))
            MelonCoroutines.Start(PermanentAccessories.AfterRestore());
        if (FirearmSpawnPolicy.Sites.Concat(LightSpawnPolicy.Sites).Any(site => site.Scene == sceneName))
            MelonCoroutines.Start(FiniteFirearms.WaitForWorld());
    }

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Interloper Overhaul 0.20.0 loaded. One allocated rifle, revolver, hunting knife, and hatchet per new Interloper run; found ammo retention 0.25%, casings 0.5%. Variant workshop schematics enabled. Two standard flashlights and one miner flashlight per new run. Native cannery manufacturing and item weight/warmth balance enabled in Interloper. Permanent tool-belt and rifle-holster collectibles enabled in Interloper. Clothing, nutrition, and drink-effect balance enabled in Interloper. Fire materials, feats, Mending, and cougar balance enabled in Interloper. Cooking, food preparation, shelters, noisemakers, ammunition ingredients, and randomized mementos enabled in Interloper. Temporary frostbite, fixed parasite treatment, cabin fever removal, instant severe lacerations, defensive prybar, warmer vehicles, roadside supplies, and cheaper rock caches enabled in Interloper. Exploration-gated restored equipment, expedition allocations, scarce research books, and Cannery manufacturing costs enabled. Direct main menu enabled globally.");
    }
}
