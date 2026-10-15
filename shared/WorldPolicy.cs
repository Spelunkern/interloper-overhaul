namespace InterloperOverhaul;

// Independent of Unity so policy boundaries can be checked without game binaries.
public static class WorldPolicy
{
    public static bool IsTaleBunker(string scene) => scene is "BunkerA" or "BunkerB" or "BunkerC" or "BunkerXL";

    public static bool IsRadioCache(string path) =>
        path.Split('/').Any(segment => segment.Equals("TrackableHiddenCaches", System.StringComparison.OrdinalIgnoreCase));

    public static bool IsReducedRadioCache(string path) =>
        path.Split('/').Any(segment => segment.Equals("TrackableHiddenCaches_Interloper", System.StringComparison.OrdinalIgnoreCase));

    public static bool IsTaleReward(string name) => name.Replace("(Clone)", "").Trim() is
        "GEAR_TacticalJacket" or "GEAR_RifleScabbardA" or "GEAR_TacticalGloves" or "GEAR_ToolBelt";

    public static bool IsStoryExchange(string name) => name is
        "SpecialRequest1" or "SpecialRequest2" or "SpecialRequest3" or "SpecialRequest4" or "SpecialRequest5" or
        "SpecialRequestGroup1" or "SpecialRequestGroup2" or "SpecialRequestGroup3" or "SpecialRequestGroup4" or "SpecialRequestGroup5";

    public const string CoastalScene = "CoastalRegion";
    public const string BurnedHousesRoot = "Design/RandomSpawnStructures/RandomBurnedHouses/";
    public const string NightmareSpawner = "RandomSpawnObjectBase_burnt_nightmare";

    public static bool IsCoastalHouseSpawner(string scene, string path, string name) =>
        scene == CoastalScene && path.StartsWith(BurnedHousesRoot, System.StringComparison.Ordinal) &&
        (name is "RandomSpawnObjectBase_cabins" or "RandomSpawnObjectBase_townNorth" or
            "RandomSpawnObjectBase_townSouth" or "RandomSpawnObjectBase_waterfront" or
            "RandomSpawnObjectBase_logsort" or NightmareSpawner);

    public static bool IsMemento(string path) =>
        path.IndexOf("Memento", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
        path.IndexOf("TrackableHiddenCache", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
        path.IndexOf("VisorNote", System.StringComparison.OrdinalIgnoreCase) >= 0;

    public static bool IsCoastalStandaloneHouse(string scene, string path) =>
        scene == CoastalScene &&
        (path == "Art/Structure_Group/STRSPAWN_HouseExteriorE_Prefab" ||
         path == "Art/Structure_Group/LakeCabins_with_Fishing_interiors/STRSPAWN_LakeCabinD_Prefab");
}
