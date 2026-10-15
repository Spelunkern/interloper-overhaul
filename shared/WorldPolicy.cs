namespace InterloperOverhaul;

// Independent of Unity so policy boundaries can be checked without game binaries.
public static class WorldPolicy
{
    public static bool IsBeachcombingObject(string path) => path.Split('/').Any(segment =>
        segment.Replace("(Clone)", "").Trim() is
            "CustomSpawnedItem_canoe" or "CustomSpawnedItem_rowboat" or "CustomSpawnedItem_fishingboat" or
            "CustomSpawnedItem_backpack" or "CustomSpawnedItem_trunk" or "CustomSpawnedItem_pallet" or
            "CustomSpawnedItem_planks" or "CustomSpawnedItem_suitcase1" or "CustomSpawnedItem_suitcase2" or
            "CustomSpawnedItem_locker1" or "CustomSpawnedItem_locker2" or
            "CustomSpawnedItem_corpse1" or "CustomSpawnedItem_corpse2" or "CustomSpawnedItem_corpse3" or
            "CustomSpawnedItem_carcass1" or "CustomSpawnedItem_carcass2");

    public static bool IsNonItemSpawnPath(string path) => path.Split('/').Any(segment =>
        segment is "WildlifeSpawns" or "Wildlife" or "PlayerSpawns" or "PlayerSpawnPoints");

    public static bool IsGearRestrictionTag(string name) => name is
        "Gameplay.Gear.NoFirearms" or "Gameplay.Gear.NoRifle" or "Gameplay.Gear.NoRevolver";

    public static bool IsSpentCasing(string name) => name.Replace("(Clone)", "").Trim() is
        "GEAR_RevolverAmmoCasing" or "GEAR_RifleAmmoCasing";

    public static bool IsSignalFireSatchel(string scene, string path) =>
        (scene is "RiverValleyRegion" or "RiverValleyRegion_SANDBOX") &&
        (path is "Design/SignalFires/SignalFireGroup1/GEAR_MooseHideBag" or
            "Design/SignalFires/SignalFireGroup2/GEAR_MooseHideBag");

    public static bool IsReducedCargo(string scene, string path) => scene == "CrashMountainRegion" &&
        (path.StartsWith("Design/Loot/CargoCaches/Interloper_set/", System.StringComparison.Ordinal) ||
         path.StartsWith("Design/Loot/PlaneTailStuff/InterloperOnly_Cargo/", System.StringComparison.Ordinal));

    public static bool IsNormalCargo(string scene, string path) => scene == "CrashMountainRegion" &&
        !IsReducedCargo(scene, path) &&
        (path.StartsWith("Design/Loot/CargoCaches/", System.StringComparison.Ordinal) ||
         path.StartsWith("Design/Loot/PlaneTailStuff/", System.StringComparison.Ordinal)) &&
        path.Split('/').Any(segment => segment.StartsWith("CONTAINER_CargoCrate_", System.StringComparison.Ordinal));

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
