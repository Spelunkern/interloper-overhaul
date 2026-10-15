using InterloperOverhaul;

int checks = 0;
void Check(bool condition, string scenario)
{
    if (!condition) throw new InvalidOperationException(scenario);
    checks++;
}

// Fixture paths observed in the installed 2.55 scene; no game assets are distributed.
var coastal = new (string Folder, string Spawner)[]
{
    ("Cabins", "RandomSpawnObjectBase_cabins"),
    ("TownNorth", "RandomSpawnObjectBase_townNorth"),
    ("TownSouth", "RandomSpawnObjectBase_townSouth"),
    ("Waterfront", "RandomSpawnObjectBase_waterfront"),
    ("LogSort", "RandomSpawnObjectBase_logsort"),
    ("Nightmare_Burned_Buildings", "RandomSpawnObjectBase_burnt_nightmare")
};
foreach (var (folder, name) in coastal)
{
    string path = $"Design/RandomSpawnStructures/RandomBurnedHouses/{folder}/{name}";
    Check(WorldPolicy.IsCoastalHouseSpawner("CoastalRegion", path, name), "Known Coastal house group");
    Check(!WorldPolicy.IsCoastalHouseSpawner("LakeRegion", path, name), "Other region preserved");
    Check(!WorldPolicy.IsCoastalHouseSpawner("CoastalRegion", "Design/Gear/" + name, name), "Unrelated same-name object preserved");
}
Check(!WorldPolicy.IsCoastalHouseSpawner("CoastalRegion", "Design/RandomSpawnStructures/RandomFishHuts/RandomSpawnObjectBase", "RandomSpawnObjectBase"), "Fishing hut doors preserved");
Check(WorldPolicy.IsMemento("Design/Vehicles/Car/GEAR_VisorNoteCH2"), "Visor note restored");
Check(WorldPolicy.IsMemento("Design/TrackableHiddenCaches"), "Hidden cache parent restored");
Check(!WorldPolicy.IsMemento("Design/Gear/Weapons/Rifle/GEAR_Rifle"), "Firearm is not a memento");
Check(!WorldPolicy.IsMemento("Design/Plants/ReishiMushroom"), "Harvestable is not a memento");
Check(WorldPolicy.IsCoastalStandaloneHouse("CoastalRegion", "Art/Structure_Group/STRSPAWN_HouseExteriorE_Prefab"), "Standalone Coastal house restored");
Check(!WorldPolicy.IsCoastalStandaloneHouse("LakeRegion", "Art/Structure_Group/STRSPAWN_HouseExteriorE_Prefab"), "Same house name in another region preserved");
Check(WorldPolicy.IsRadioCache("Design/TrackableHiddenCaches/INTERACTIVE_TrackableCrashBox"), "Normal radio cache branch");
Check(WorldPolicy.IsReducedRadioCache("Root/Design/TrackableHiddenCaches_interloper/Cache"), "Reduced branch ignores authored case differences");
Check(!WorldPolicy.IsRadioCache("Design/TrackableHiddenCaches_Interloper/Cache"), "Reduced cache is not normal cache");
Check(!WorldPolicy.IsReducedRadioCache("Design/TrackableHiddenCaches/Cache"), "Normal cache is not reduced cache");
Check(!WorldPolicy.IsRadioCache("Design/NotTrackableHiddenCaches/Cache"), "Unrelated substring excluded");
foreach (string scene in new[] { "BunkerA", "BunkerB", "BunkerC", "BunkerXL" })
    Check(WorldPolicy.IsTaleBunker(scene), "Authored Signal Void bunker");
Check(!WorldPolicy.IsTaleBunker("BunkerInteriorA"), "Ordinary prepper bunker does not use the Tales reward scope");
Check(WorldPolicy.IsTaleReward("GEAR_TacticalJacket(Clone)"), "Instantiated unique reward");
Check(WorldPolicy.IsTaleReward("GEAR_RifleScabbardA"), "Holster reward");
Check(!WorldPolicy.IsTaleReward("GEAR_Rifle"), "Ordinary rifle is not a unique Tale reward");
Check(WorldPolicy.IsStoryExchange("SpecialRequestGroup4"), "Story unlock exchange");
Check(!WorldPolicy.IsStoryExchange("RifleAmmoBox"), "Common trader stock does not use the story reward scope");
Check(!WorldPolicy.IsStoryExchange("SpecialRequestGroup40"), "Unrecognized trader exchange excluded");
Check(WorldPolicy.IsNormalCargo("CrashMountainRegion", "Design/Loot/CargoCaches/cargo_creek/CONTAINER_CargoCrate_clothesC_foodC"), "Mountain cargo assembly restored");
Check(WorldPolicy.IsNormalCargo("CrashMountainRegion", "Design/Loot/PlaneTailStuff/CONTAINER_CargoCrate_rifle_medicalRare/Compartment"), "Summit cargo compartment restored");
Check(!WorldPolicy.IsNormalCargo("CrashMountainRegion", "Design/Loot/Hacksaw/CrashPieceSaw/CargoSaw/GEAR_Hacksaw"), "Nearby tool is not a cargo assembly");
Check(!WorldPolicy.IsNormalCargo("LakeRegion", "Design/Loot/CargoCaches/CONTAINER_CargoCrate_food"), "Other scenes excluded from cargo policy");
foreach (var path in new[] { "Design/Loot/CargoCaches/Interloper_set/CONTAINER_CargoCrate_food", "Design/Loot/PlaneTailStuff/InterloperOnly_Cargo/CONTAINER_CargoCrate_food" })
{
    Check(WorldPolicy.IsReducedCargo("CrashMountainRegion", path), "Reduced cargo assembly identified");
    Check(!WorldPolicy.IsNormalCargo("CrashMountainRegion", path), "Reduced cargo is not restored as normal cargo");
}
foreach (int group in new[] { 1, 2 })
    Check(WorldPolicy.IsSignalFireSatchel("RiverValleyRegion", $"Design/SignalFires/SignalFireGroup{group}/GEAR_MooseHideBag"), "Both signal fire satchel locations covered");
Check(!WorldPolicy.IsSignalFireSatchel("LakeRegion", "Design/SignalFires/SignalFireGroup1/GEAR_MooseHideBag"), "Satchel restoration limited to HRV");
Check(WorldPolicy.IsSignalFireSatchel("RiverValleyRegion_SANDBOX", "Design/SignalFires/SignalFireGroup1/GEAR_MooseHideBag"), "Authored HRV additive scene covered");
Check(!WorldPolicy.IsSignalFireSatchel("RiverValleyRegion", "Design/SignalFires/SignalFireGroup1/GEAR_Hatchet"), "Hatchet is not the specific Signal Fire satchel");
Check(WorldPolicy.IsSpentCasing("GEAR_RevolverAmmoCasing(Clone)"), "Revolver casing recognized after instantiation");
Check(WorldPolicy.IsSpentCasing("GEAR_RifleAmmoCasing"), "Rifle casing recognized");
Check(!WorldPolicy.IsSpentCasing("GEAR_RifleAmmoSingle"), "Live ammunition is not an empty casing");
Check(!WorldPolicy.IsSpentCasing("GEAR_Revolver"), "Firearm is not an empty casing");
Check(WorldPolicy.IsNonItemSpawnPath("Design/WildlifeSpawns/WolfGroup"), "Wildlife spawn hierarchy excluded");
Check(WorldPolicy.IsNonItemSpawnPath("Design/PlayerSpawns/Point1"), "Player spawn hierarchy excluded");
Check(!WorldPolicy.IsNonItemSpawnPath("Design/Gear/GEAR_Rifle"), "Ordinary world gear eligible");
foreach (string tag in new[] { "Gameplay.Gear.NoFirearms", "Gameplay.Gear.NoRifle", "Gameplay.Gear.NoRevolver" })
    Check(WorldPolicy.IsGearRestrictionTag(tag), "Gear ban identified");
Check(!WorldPolicy.IsGearRestrictionTag("Gameplay.SafehouseSupported"), "Unrelated gameplay tag preserved");
Console.WriteLine($"PASS: {checks} policy boundary checks.");
