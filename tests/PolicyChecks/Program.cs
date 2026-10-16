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
Check(WorldPolicy.IsBeachcombingObject("CustomSpawnedItem_canoe/GEAR_Arrow"), "Canoe loot recognized");
Check(WorldPolicy.IsBeachcombingObject("CustomSpawnedItem_fishingboat(Clone)/RandomLoot/GEAR_FlareGunAmmoSingle"), "Instantiated fishing-boat loot recognized");
Check(WorldPolicy.IsBeachcombingObject("Root/CustomSpawnedItem_suitcase1/Container"), "Beach suitcase recognized");
Check(!WorldPolicy.IsBeachcombingObject("Design/Gear/GEAR_Arrow"), "Ordinary world arrows excluded from beach-only override");
Check(!WorldPolicy.IsBeachcombingObject("CustomSpawnedItem_droppeddecorationstorage/Container"), "Player decoration storage excluded");
Check(!WorldPolicy.IsBeachcombingObject("CustomSpawnedItem_canoe_fake/GEAR_Arrow"), "Unrelated prefix excluded");
Check(LootBalancePolicy.Retention("GEAR_PeanutButter", true) == 0.05, "Restored ordinary loot retains five percent");
foreach (string name in new[] { "GEAR_HeavyParka", "GEAR_Hacksaw", "GEAR_CowichanSweater", "GEAR_PremiumWinterCoat" })
    Check(LootBalancePolicy.Retention(name, true)==0 && LootBalancePolicy.Retention(name,true,LootAccess.Deep)==(ExplorationPolicy.IsPremium(name) ? 0.01 : 0.05), "Restored equipment requires an expedition source: " + name);
foreach (string name in new[] { "GEAR_Rifle_Curators", "GEAR_Bow_Woodwrights", "GEAR_SurvivalKnife" })
    Check(LootBalancePolicy.Retention(name, true) == 0, "Special variants are temporarily disabled: " + name);
Check(LootBalancePolicy.Retention("GEAR_HeavyParka(Clone)", true,LootAccess.Deep) == 0.01, "Cloned gear uses the same rarity tier");
Check(LootBalancePolicy.Retention("GEAR_Hacksaw (12)", true,LootAccess.Remote) == 0.01, "Authored numbered instances use the same tier");
foreach (string name in new[] { "GEAR_Hammer", "GEAR_Stick" })
    Check(LootBalancePolicy.Retention(name, false) == 1, "Original Interloper sources retain availability: " + name);
foreach (string name in new[] { "GEAR_TacticalJacket", "GEAR_RifleScabbardA", "GEAR_ToolBelt", "GEAR_TacticalGloves" })
    Check(LootBalancePolicy.Retention(name, true) == 0, "Progression rewards require an authenticated source: " + name);
foreach (double retention in new[] { 0.05, 0.01, 0.0025, 0.005 })
{
    int accepted = Enumerable.Range(0, 100000).Count(slot => LootBalancePolicy.Sample(12345, "CoastalRegion/container", slot) < retention);
    Check(Math.Abs(accepted - retention * 100000) < 100, "Retention distribution matches tier across independent slots: " + retention);
}
Check(LootBalancePolicy.Sample(123, "scene/item", 7) == LootBalancePolicy.Sample(123, "scene/item", 7), "Rarity sampling is repeatable");
Check(LootBalancePolicy.Sample(123, "scene/item", 7) != LootBalancePolicy.Sample(124, "scene/item", 7), "Different world seeds change rarity");
Check(LootBalancePolicy.Sample(123, "scene/item", 7) != LootBalancePolicy.Sample(123, "scene/other", 7), "Different locations change rarity");
foreach (string name in new[] { "GEAR_Rifle", "GEAR_Revolver", "GEAR_Rifle(Clone)" })
{
    Check(FirearmSpawnPolicy.IsStandardFirearm(name), "Standard firearm recognized: " + name);
    Check(LootBalancePolicy.Retention(name, true) == 0 && LootBalancePolicy.Retention(name, false) == 0, "Unallocated standard firearms never enter ordinary loot");
}
foreach (string name in new[] { "GEAR_Rifle_Curators", "GEAR_Rifle_Trader", "GEAR_RevolverFancy" })
    Check(!FirearmSpawnPolicy.IsStandardFirearm(name) && LootBalancePolicy.Retention(name, true) == 0, "Disabled variants remain outside the standard firearm allocation");
foreach (string name in new[] { "GEAR_RifleAmmoSingle", "GEAR_RifleAmmoBox", "GEAR_RevolverAmmoSingle", "GEAR_RevolverAmmoBox" })
    Check(LootBalancePolicy.Retention(name, true) == 0.0025 && LootBalancePolicy.Retention(name, false) == 0.0025, "Found ammunition uses quarter-percent retention");
foreach (string name in new[] { "GEAR_RifleAmmoCasing", "GEAR_RevolverAmmoCasing" })
    Check(LootBalancePolicy.Retention(name, true) == 0.005, "Found casings use half-percent retention");
Check(FirearmSpawnPolicy.LegacySites.Count(site => FirearmSpawnPolicy.IsStandardFirearm(site.Item)) == 28 && FirearmSpawnPolicy.Sites.Select(site => site.Id).Distinct().Count() == FirearmSpawnPolicy.Sites.Length, "Audited firearm site identities are unique");
Check(FirearmSpawnPolicy.Sites.All(site => !site.Path.Contains("Whiteout") && !site.Path.Contains("research")), "Challenge and quest-only positions excluded");
var plans = Enumerable.Range(0, 1000).Select(seed => FirearmRunState.Create(seed, "Sandbox/123")).ToArray();
Check(plans.All(plan => plan.Site("GEAR_Rifle").Item == "GEAR_Rifle" && plan.Site("GEAR_Revolver").Item == "GEAR_Revolver"), "Every seed allocates exactly one location for each standard firearm");
Check(plans.Select(plan => plan.RifleSite).Distinct().Count() > 1 && plans.Select(plan => plan.RevolverSite).Distinct().Count() > 1, "Both firearm locations vary between seeds");
Check(plans.All(plan => FirearmSpawnPolicy.Condition(plan.Seed, "GEAR_Rifle") is >= 1 and <= 100), "All allocated rifle conditions are usable and within range");
Check(plans.Select(plan => FirearmSpawnPolicy.Condition(plan.Seed, "GEAR_Revolver")).Distinct().Count() > 90, "Random revolver conditions cover the full range");
var allocation = FirearmRunState.Create(123, "Sandbox/123");
string before = allocation.Serialize();
allocation.SetCreated("GEAR_Rifle", true);
var loaded = FirearmRunState.Deserialize(allocation.Serialize());
Check(loaded.RifleCreated && !loaded.RevolverCreated && loaded.RifleSite == allocation.RifleSite, "Save round trip preserves independent firearm issuance and location");
Check(!FirearmRunState.Deserialize(before).RifleCreated, "Loading an older snapshot restores its own issuance state");
Check(loaded.GuidFor("GEAR_Rifle") == allocation.GuidFor("GEAR_Rifle") && loaded.GuidFor("GEAR_Rifle") != loaded.GuidFor("GEAR_Revolver"), "Managed firearm identities survive reloads and distinguish weapon types");
Check(FirearmRunState.Create(123, "Sandbox/124").GuidFor("GEAR_Rifle") != allocation.GuidFor("GEAR_Rifle"), "Different runs do not share managed firearm identities");
Check(Enumerable.Range(0, 100000).Count(aurora => LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoBox", aurora)) is > 150 and < 350,
    "Trader ammunition availability follows quarter-percent retention across auroras");
Check(LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoBox", 4) == LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoBox", 4),
    "Reloading the same trader aurora cannot reroll supply availability");
Check(Enumerable.Range(0, 1000).All(aurora => !LootBalancePolicy.KeepTraderSupply(123, "GEAR_Rifle_Trader", aurora)),
    "Weapon variants are absent from every trader aurora");
Check(FirearmSpawnPolicy.IsStandardFirearm("GearItemData_Rifle") && LootBalancePolicy.IsLiveFirearmAmmo("GearItemData_RifleAmmoBox"),
    "Trader gear-data asset names resolve to the correct firearm and ammunition identities");
Check(LootBalancePolicy.TraderItem("2e03fa64f3ab1d141a584daf81e358f5") == "GEAR_SurvivalKnife" && LootBalancePolicy.Retention("GearItemData_SurvivalKnife", false) == 0, "Variant authored identity and unrestricted source blocked: SurvivalKnife");
Check(LootBalancePolicy.TraderItem("96daa7434362e754fb7bd4fbb24c8cd4") == "GEAR_Bow_Manufactured" && LootBalancePolicy.Retention("GearItemData_Bow_Manufactured", false) == 0, "Variant authored identity and unrestricted source blocked: Bow_Manufactured");
Check(LootBalancePolicy.TraderItem("4c5b21d92f282da4594dc59dd73a9f4a") == "GEAR_Bow_Woodwrights" && LootBalancePolicy.Retention("GearItemData_Bow_Woodwrights", false) == 0, "Variant authored identity and unrestricted source blocked: Bow_Woodwrights");
Check(LootBalancePolicy.TraderItem("9554b92c571828948b07f11703c50dea") == "GEAR_RevolverFancy" && LootBalancePolicy.Retention("GearItemData_RevolverFancy", false) == 0, "Variant authored identity and unrestricted source blocked: RevolverFancy");
Check(LootBalancePolicy.TraderItem("fc0ad0fa37b22274cb0a9964f5dcbf4b") == "GEAR_RevolverGreen" && LootBalancePolicy.Retention("GearItemData_RevolverGreen", false) == 0, "Variant authored identity and unrestricted source blocked: RevolverGreen");
Check(LootBalancePolicy.TraderItem("1a5913c327c618046bdebe220935e903") == "GEAR_RevolverStubNosed" && LootBalancePolicy.Retention("GearItemData_RevolverStubNosed", false) == 0, "Variant authored identity and unrestricted source blocked: RevolverStubNosed");
Check(LootBalancePolicy.TraderItem("28e60db238cbfb549a7a1da9498f65f1") == "GEAR_Rifle_Barbs" && LootBalancePolicy.Retention("GearItemData_Rifle_Barbs", false) == 0, "Variant authored identity and unrestricted source blocked: Rifle_Barbs");
Check(LootBalancePolicy.TraderItem("118e9d9493692c44ba7a0acb6133fc21") == "GEAR_Rifle_Curators" && LootBalancePolicy.Retention("GearItemData_Rifle_Curators", false) == 0, "Variant authored identity and unrestricted source blocked: Rifle_Curators");
Check(LootBalancePolicy.TraderItem("b930bfe9aa379ec4885009a692cfea2e") == "GEAR_Rifle_Trader" && LootBalancePolicy.Retention("GearItemData_Rifle_Trader", false) == 0, "Variant authored identity and unrestricted source blocked: Rifle_Trader");
Check(LootBalancePolicy.TraderItem("32ed5798a3d3189429b3d08ef20eec63") == "GEAR_Rifle_Vaughns" && LootBalancePolicy.Retention("GearItemData_Rifle_Vaughns", false) == 0, "Variant authored identity and unrestricted source blocked: Rifle_Vaughns");
Check(!LootBalancePolicy.IsWeaponVariant("GEAR_Knife") && !LootBalancePolicy.IsWeaponVariant("GEAR_Bow") && !LootBalancePolicy.IsWeaponVariant("GEAR_TacticalJacket"), "Ordinary weapons and clothing are outside the variant ban");
foreach (string item in new[] { "GEAR_Knife", "GEAR_Hatchet" })
{
    Check(FirearmSpawnPolicy.IsFiniteItem(item) && LootBalancePolicy.Retention(item, true) == 0 && LootBalancePolicy.Retention(item, false) == 0,
        "Unallocated standard tools are excluded from all generated loot: " + item);
    Check(plans.All(plan => plan.Site(item).Item == item) && plans.Select(plan => plan.Site(item).Id).Distinct().Count() > 1,
        "Exactly one native tool placement is chosen per seed: " + item);
    allocation.SetCreated(item, true);
    var restoredPlan = FirearmRunState.Deserialize(allocation.Serialize());
    Check(restoredPlan.Created(item) && restoredPlan.Site(item).Id == allocation.Site(item).Id && restoredPlan.GuidFor(item) == allocation.GuidFor(item),
        "Tool issuance, placement, and identity survive save round trips: " + item);
}
var oldPlan = System.Text.Json.Nodes.JsonNode.Parse(before)!;
oldPlan["Version"] = 1;
foreach (string field in new[] { "KnifeSite", "HatchetSite", "KnifeCreated", "HatchetCreated", "ToolsLegacy" }) oldPlan.AsObject().Remove(field);
var migratedPlan = FirearmRunState.Deserialize(oldPlan.ToJsonString());
Check(migratedPlan.ToolsLegacy && migratedPlan.IsLegacy("GEAR_Knife") && !migratedPlan.IsLegacy("GEAR_Rifle") && migratedPlan.RifleSite == allocation.RifleSite,
    "Version-one saves retain firearm locations without receiving extra tools");
Check(!FirearmSpawnPolicy.IsFiniteItem("GEAR_KnifeImprovised") && !FirearmSpawnPolicy.IsFiniteItem("GEAR_HatchetImprovised"),
    "Improvised tools are outside the finite standard-tool allocation");
Check(ManufacturingPolicy.Recipes.Select(recipe => recipe.BlueprintName).Distinct().Count() == 8, "Manufacturing recipes have stable unique identities");
foreach (var recipe in ManufacturingPolicy.Recipes)
{
    Check(recipe.Count > 0 && recipe.Minutes > 0 && recipe.Ingredients.All(input => input.Count > 0),
        "Manufacturing requires positive quantities and crafting time: " + recipe.Id);
    Check(!ManufacturingPolicy.IsFlare(recipe.Result) && !LootBalancePolicy.IsWeaponVariant(recipe.Result) && !FirearmSpawnPolicy.IsStandardFirearm(recipe.Result),
        "Manufacturing never produces variants, common firearms, or renewable input flares: " + recipe.Id);
}
Check(ManufacturingPolicy.Recipes.Where(recipe => FirearmSpawnPolicy.IsLimitedTool(recipe.Result)).All(recipe =>
    recipe.Ingredients.Any(input => input.Item.EndsWith("Improvised") && input.Count == 1)), "Manufactured tools consume an improvised predecessor");
Check(ManufacturingPolicy.Recipes.Where(recipe => recipe.PowderResultKg > 0).All(recipe => recipe.PowderResultKg == 0.1 &&
    recipe.Ingredients.Single(input => ManufacturingPolicy.IsFlare(input.Item)).Count == 3), "Recycled powder is measured and costs three flares");
Check(ManufacturingPolicy.Recipes.Single(recipe => recipe.Id == "ShellFromPowder").PowderRequiredKg == 0.1,
    "Powder-based flare shells consume measured gunpowder");


for (int seed = 0; seed < 500; seed++)
{
    var state = FirearmRunState.Create(seed,"light-policy");
    Check(state.Lights.Count == 3 && state.Lights.Count(l => l.Item == "GEAR_Flashlight") == 2 &&
        state.Lights.Count(l => l.Item == "GEAR_Flashlight_LongLasting") == 1, "exact flashlight allocation");
    Check(state.Lights.Select(l => l.SiteId).Distinct().Count() == 3, "flashlight locations never overlap");
    var reload = FirearmRunState.Deserialize(state.Serialize());
    Check(reload.Lights.Select(l => l.SiteId).SequenceEqual(state.Lights.Select(l => l.SiteId)), "flashlight plan persists");
    Check(state.Lights.Select(l => state.GuidFor("light/"+l.Key)).Distinct().Count() == 3, "flashlight identities are unique");
}
var oldLights = System.Text.Json.Nodes.JsonNode.Parse(FirearmRunState.Create(123,"migration").Serialize())!;
oldLights["Version"] = 2;
oldLights.AsObject().Remove("Lights");
Check(FirearmRunState.Deserialize(oldLights.ToJsonString()).LightsLegacy, "old saves receive no additional flashlights");
Check(VariantPolicy.Variants.Length == 11 && VariantPolicy.Recipes.Count() == 18, "eleven upgrades and seven firearm reversals");
foreach (var variant in VariantPolicy.Variants)
{
    Check(LootBalancePolicy.IsWeaponVariant(variant.Item), "every variant is blocked as ordinary random loot");
    Check(!VariantPolicy.Unlocked(variant.Upgrade,new HashSet<string>()), "schematic is required");
    Check(VariantPolicy.Unlocked(variant.Upgrade,new HashSet<string> { variant.Item }), "learning unlocks the matching upgrade");
    Check(variant.Upgrade.Ingredients[0].Item == variant.Base && variant.Upgrade.Ingredients[0].Count == 1, "upgrade transforms one base item");
    if (variant.Reverse != null)
        Check(variant.Reverse.Ingredients.Length == 1 && variant.Reverse.Result == variant.Base && variant.Reverse.Tools.Length == 0, "reversion costs only time");
}
var workshop = new WorkshopRunState();
workshop.Learned.Add("GEAR_Rifle_Barbs");
workshop.Cards[Guid.NewGuid().ToString()] = "GEAR_SurvivalKnife";
workshop.Work.Add(new() { Recipe=VariantPolicy.Variants[4].Upgrade.BlueprintName,WorkGuid=Guid.NewGuid().ToString(),OriginalGuid=Guid.NewGuid().ToString(),Condition=0.37f });
Check(WorkshopRunState.Deserialize(workshop.Serialize()).Work.Single().Condition == 0.37f, "saved conversion preserves condition");
Check(ItemBalancePolicy.IsBook("GEAR_BookGunsmithing(Clone)") && ItemBalancePolicy.IsBook("GEAR_BookLabE_Open_01"), "research and ordinary books share the lighter weight");
Check(!ItemBalancePolicy.IsBook("GEAR_Logbook_A01") && ItemBalancePolicy.WeightKg("GEAR_Logbook_A01") < 0, "weightless narrative logs are outside book rebalance");
Check(ItemBalancePolicy.IsFlask("GEAR_InsulatedFlask_G(Clone)") && !ItemBalancePolicy.IsFlask("GEAR_CookingPot"), "all insulated flask styles are covered without changing other containers");
Check(ItemBalancePolicy.WeightKg("GEAR_ArrowManufactured") < 0 && ItemBalancePolicy.WeightKg("GEAR_ArrowHardened") < 0 && ItemBalancePolicy.WeightKg("GEAR_FlareGunAmmoSingle") < 0, "special arrows and flare shells retain their own weights");
foreach (string weapon in new[] { "GEAR_Rifle", "GEAR_Rifle_Trader", "GEAR_RevolverGreen", "GEAR_Bow_Manufactured", "GEAR_FlareGun", "GEAR_BearSpear", "GEAR_NoiseMaker" })
    Check(PermanentAccessoryPolicy.IsWeapon(weapon) && PermanentAccessoryPolicy.ToolFamily(weapon)=="", "belt excludes weapons: " + weapon);
Check(PermanentAccessoryPolicy.ToolFamily("GEAR_Knife")==PermanentAccessoryPolicy.ToolFamily("GEAR_SurvivalKnife") &&
    PermanentAccessoryPolicy.ToolFamily("GEAR_Hatchet")==PermanentAccessoryPolicy.ToolFamily("GEAR_HatchetImprovised"), "tool variants share one functional type");
Check(PermanentAccessoryPolicy.IsCollectible("GEAR_ToolBelt(Clone)") && PermanentAccessoryPolicy.IsCollectible("GEAR_RifleScabbardA") &&
    !PermanentAccessoryPolicy.IsCollectible("GEAR_MooseHideBag"), "only the belt and holster become permanent collectibles");
var accessories = new PermanentAccessoryState { ToolBelt=true,RifleHolster=true };
Check(PermanentAccessoryState.Deserialize(accessories.Serialize()).ToolBelt && PermanentAccessoryState.Deserialize(accessories.Serialize()).RifleHolster,
    "permanent accessory unlocks survive a save round trip");
Check(!new PermanentAccessoryState().ToolBelt && !new PermanentAccessoryState().RifleHolster,"new runs begin without collectible bonuses");
foreach (var food in ClothingNutritionPolicy.Calories)
{
    Check(food.Original > 0 && food.Target > food.Original, "calorie increases have valid migration baselines");
    Check(ClothingNutritionPolicy.Food(food.Item+"(Clone)")==food, "calorie lookup normalizes spawned names");
}
foreach (string ingredient in new[] { "GEAR_CookingOil", "GEAR_Flour", "GEAR_OatsTin", "GEAR_Salt", "GEAR_SaltBag" })
    Check(ItemBalancePolicy.WeightKg(ingredient)<0 && ClothingNutritionPolicy.ContentWeightScale(ingredient)>0 &&
        ClothingNutritionPolicy.ContentWeightScale(ingredient)<1, "ingredient mass uses quantity scaling without a second base weight");
Check(ClothingNutritionPolicy.Food("GEAR_CoffeeCup")==null && ClothingNutritionPolicy.ContentWeightScale("GEAR_GunpowderCan")==1,
    "unlisted calories and recipe resources remain unchanged");
foreach (string tinder in new[] { "GEAR_Newsprint", "GEAR_NewsprintRoll", "GEAR_PaperStack" })
    Check(FireBalancePolicy.TinderBonus(tinder+"(Clone)")==8 && ItemBalancePolicy.WeightKg(tinder)==0.05, "paper tinder gets eight points and fifty-gram weight");
foreach (string tinder in new[] { "GEAR_Tinder", "GEAR_BarkTinder", "GEAR_CattailTinder", "GEAR_CashBundle" })
    Check(FireBalancePolicy.TinderBonus(tinder)==4,"ordinary tinder gets four success points");
Check(FireBalancePolicy.TinderBonus("GEAR_Stick")==0 && FireBalancePolicy.TinderBonus("GEAR_BookA")==0,"fuels do not also receive a tinder bonus");
Check(FireBalancePolicy.FuelSuccessBonus("GEAR_ReclaimedWoodB")==0 && FireBalancePolicy.FuelSuccessBonus("GEAR_Firelog")==50 &&
    FireBalancePolicy.FuelSuccessBonus("GEAR_CedarFirewood")==null,"only requested fuel success modifiers change");
Check(FireBalancePolicy.BurnHours("GEAR_BookCooking")==0.5f && FireBalancePolicy.BurnHours("GEAR_Firelog")==3 &&
    FireBalancePolicy.BurnHours("GEAR_FirewoodHardwood")<0,"book and firelog burn changes leave other fuels alone");
Check(ManufacturingPolicy.Find(FoodPreparationPolicy.BundleRecipe.BlueprintName)==FoodPreparationPolicy.BundleRecipe,"bundle recipe resolves through the manufacturing catalog");
Check(FoodPreparationPolicy.BundleRecipe.Ingredients[0].Count==10 && FoodPreparationPolicy.BundleRecipe.Minutes==1,"bundle consumes ten stalks in one minute");
var saltMass=new SaltMassRecord();
saltMass.Deposit(0.25,0.1);saltMass.Deposit(10,3);
Check(Math.Abs(saltMass.MassKg-3.1)<1e-9,"salt ledger preserves distinct jar and bag conversions");
saltMass.Reconcile(5.125);
Check(Math.Abs(saltMass.MassKg-1.55)<1e-9,"mixed salt consumption scales remaining physical mass proportionally");
saltMass.Reconcile(0);
Check(saltMass.MassKg==0 && saltMass.QuantityKg==0,"empty curing box clears its salt mass ledger");
Check(MementoRewardPolicy.IsCache("Design/Mementos/CH2/Box") && !MementoRewardPolicy.IsCache("Design/TrackableHiddenCaches/Box"),"memento rewards exclude Tales radio caches");
var outcomes=new HashSet<string>();
for (int seed=0;seed<500;seed++)
{
    string reward=MementoRewardPolicy.Choose(seed,"CoastalRegion/CH2",0);
    outcomes.Add(reward);
    Check(reward==MementoRewardPolicy.Choose(seed,"CoastalRegion/CH2",0),"memento reload cannot reroll the reward");
    Check(!FirearmSpawnPolicy.IsFiniteItem(reward) && !LootBalancePolicy.IsWeaponVariant(reward) && !LootBalancePolicy.IsLiveFirearmAmmo(reward),"memento supplies respect finite equipment and ammunition balance");
}
Check(outcomes.Count==MementoRewardPolicy.Supplies.Length,"memento supply pool is reachable across runs");
Check(LootBalancePolicy.Retention("GEAR_StumpRemover",true)==1 && LootBalancePolicy.Retention("GEAR_DustingSulfur",true)==1,"ammunition chemicals bypass restored-loot rarity");
Check(StreetLootPolicy.IsFuelCover("CarSedanFuelCover_Prefab") && StreetLootPolicy.IsFuelCover("CarTruckFuelCover_Prefab(Clone)") &&
    !StreetLootPolicy.IsFuelCover("OBJ_ElevatedFuelTankA_Prefab"),"vehicle fuel loot excludes industrial tanks");
Check(StreetLootPolicy.IsMailboxSpawner("SpawnPrefab_mailbox") && !StreetLootPolicy.IsMailboxSpawner("SpawnPrefab_FrontWellLiquids"),"mail loot targets only mailbox spawners");
int papers=0,commonBooks=0,skillBooks=0,fuelFinds=0;
var mailOutcomes=new HashSet<string>();
for(int seed=0;seed<20000;seed++)
{
    string mail=StreetLootPolicy.Mail(seed,"CoastalRegion/mailbox");mailOutcomes.Add(mail);
    if(mail is "GEAR_Newsprint" or "GEAR_NewsprintRoll") papers++;
    else if(StreetLootPolicy.CommonBooks.Contains(mail)) commonBooks++;
    else if(StreetLootPolicy.SkillBooks.Contains(mail)) skillBooks++;
    double fuel=StreetLootPolicy.FuelLitres(seed,"CoastalRegion/car");
    if(fuel>0) { fuelFinds++;if(fuel<0.05 || fuel>=0.15) throw new Exception("Fuel quantity outside policy range."); }
}
Check(papers>14500 && papers<15500 && commonBooks>650 && commonBooks<950 && skillBooks>25 && skillBooks<80,"mailbox frequencies remain near seventy-five percent paper, four percent common books, and quarter-percent research books");
Check(fuelFinds>1800 && fuelFinds<2200,"one in ten vehicle tanks contains a small fuel find");
Check(mailOutcomes.Count==2+StreetLootPolicy.CommonBooks.Length+StreetLootPolicy.SkillBooks.Length+1,"all ordinary and research books are reachable without adding unrelated mailbox loot");
Check(StreetLootPolicy.Mail(2718,"MailboxA")==StreetLootPolicy.Mail(2718,"MailboxA") && StreetLootPolicy.FuelLitres(2718,"CarA")==StreetLootPolicy.FuelLitres(2718,"CarA"),"street loot selection is stable on reload");
foreach(var access in Enum.GetValues<LootAccess>())
{
    Check(LootBalancePolicy.Retention("GEAR_HeavyParka",true,access)==(access==LootAccess.Deep ? 0.01 : 0),"premium clothing is exclusive to deep exploration: "+access);
    Check(LootBalancePolicy.Retention("GEAR_Hacksaw",false,access)==1,"native Interloper sources survive geographic gating: "+access);
    Check(LootBalancePolicy.Retention("GEAR_CottonScarf",true,access,true)==(access==LootAccess.Deep ? 0.05 : access==LootAccess.Remote ? 0.01 : 0),"restored clothing cannot bypass equipment gating: "+access);
}
Check(ExplorationPolicy.Access("CrashMountainRegion","Design/Loot/PlaneTailStuff/Box")==LootAccess.Deep &&
    ExplorationPolicy.Access("CrashMountainRegion","Design/Loot/CargoCaches/cargo_creek/Box")==LootAccess.Ordinary,"summit access does not promote lower mountain cargo");
Check(ExplorationPolicy.Access("RiverValleyRegion_SANDBOX","Design/Generic",0,0,0)==LootAccess.Ordinary &&
    ExplorationPolicy.Access("RiverValleyRegion_SANDBOX","Design/Generic",1493,203,1742)==LootAccess.Deep,"HRV requires a verified destination, not just its region name");
Check(ExplorationPolicy.Access("AshMine","CustomSpawnedItem_canoe/Box")==LootAccess.Beach,"renewable source takes precedence over a deep scene");
Check(ExplorationPolicy.IsProgressionReward("GEAR_TacticalJacket","BunkerXL","Root/Design/Tale3/GEAR_TacticalJacket") &&
    !ExplorationPolicy.IsProgressionReward("GEAR_TacticalJacket","CoastalRegion","Design/Generic"),"Tales exemption requires its actual progression scene");
Check(plans.All(plan=>FirearmSpawnPolicy.Items.All(item=>FirearmSpawnPolicy.DeepSites.Any(site=>site.Id==plan.Site(item).Id))),"every new finite allocation uses expedition destinations");
Check(plans.All(plan=>FirearmSpawnPolicy.Items.Select(item=>plan.Site(item).Path.Split('/')[1]).Distinct().Count()==4),"four unique equipment families use four different expedition destinations");
var legacyAllocation=FirearmRunState.Create(42,"legacy-geography");
legacyAllocation.RifleSite=FirearmSpawnPolicy.LegacySites.First(site=>site.Item=="GEAR_Rifle").Id;
legacyAllocation.RifleCreated=true;
Check(FirearmRunState.Deserialize(legacyAllocation.Serialize()).RifleSite==legacyAllocation.RifleSite,"old allocated locations and issuance remain valid after geographic changes");
Check(ExplorationPolicy.Access("RiverValleyRegion_SANDBOX","Design/Mementos/HRV1/CONTAINER_LockBoxB")==LootAccess.Deep &&
    ExplorationPolicy.Access("RiverValleyRegion_SANDBOX","Design/Mementos/HRV1/GEAR_VisorNoteHRV1")==LootAccess.Remote,
    "audited deep memento destination does not promote its distant note location");
Check(FirearmSpawnPolicy.DeepSites.All(site=>ExplorationPolicy.Access(site.Scene,site.Path,site.X,site.Y,site.Z)==LootAccess.Deep),
    "all new finite gear placements satisfy the geographic deep-access policy");
int rareMementos=0;
for(int seed=0;seed<10000;seed++)
{
    var reward=MementoRewardPolicy.Choose(seed,"deep-cache",0,LootAccess.Deep);
    if(MementoRewardPolicy.RareEquipment.Contains(reward)) rareMementos++;
    if(MementoRewardPolicy.RareEquipment.Contains(MementoRewardPolicy.Choose(seed,"deep-cache",1,LootAccess.Deep)) ||
       MementoRewardPolicy.RareEquipment.Contains(MementoRewardPolicy.Choose(seed,"deep-cache",0,LootAccess.Remote))) throw new Exception("Memento equipment escaped its one deep slot.");
}
Check(rareMementos>65 && rareMementos<140,"deep mementos have one rare equipment opportunity per cache");
Check(ManufacturingPolicy.Recipes.Where(recipe=>FirearmSpawnPolicy.IsLimitedTool(recipe.Result)).All(recipe=>recipe.Minutes>=240 && recipe.Tools.Contains("GEAR_HighQualityTools") && recipe.Ingredients.Any(item=>item.Item=="GEAR_Leather")),"tool refining has meaningful material, tool, and time requirements");
Console.WriteLine($"PASS: {checks} policy boundary checks.");
