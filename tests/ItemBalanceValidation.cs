extern alias GameAssembly;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;
internal static class ItemBalanceValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var mode = Game.ExperienceModeManager.s_CurrentGameMode;
        var items = new List<Game.GearItem>();
        string phase = "native weights";
        try
        {
            var cases = new Dictionary<string,float>
            {
                ["GEAR_CanOpener"]=0.10f,["GEAR_RecycledCan"]=0.10f,["GEAR_HatchetImprovised"]=1.50f,
                ["GEAR_KnifeImprovised"]=0.50f,["GEAR_Stone"]=0.10f,["GEAR_Arrow"]=0.10f,
                ["GEAR_FlareA"]=0.20f,["GEAR_BlueFlare"]=0.20f,["GEAR_CarBattery"]=5f,
                ["GEAR_Fuse"]=0.10f,["GEAR_Snare"]=0.20f,["GEAR_Rope"]=2.50f,
                ["GEAR_WoodworkingTools"]=1f,["GEAR_SprayPaintCan"]=0.20f,["GEAR_SprayPaintCanGlyphA"]=0.20f
            };
            foreach (string flask in new[] { "GEAR_InsulatedFlask_A","GEAR_InsulatedFlask_B","GEAR_InsulatedFlask_C",
                "GEAR_InsulatedFlask_D","GEAR_InsulatedFlask_E","GEAR_InsulatedFlask_F","GEAR_InsulatedFlask_G" }) cases[flask]=0.10f;
            foreach (string book in new[] { "BookA","BookB","BookC","BookD","BookE","BookF","BookG","BookH","BookI",
                "BookBopen","BookEopen","BookFopen","BookHopen","BookLabE_01","BookLabE_02","BookLabE_03","BookLabE_Open_01",
                "BookCarcassHarvesting","BookGunsmithing","BookRifleFirearm","BookRifleFirearmAdvanced","BookIceFishing","BookMending",
                "BookArchery","BookCooking","BookRevolverFirearm","BookManual","BookFireStarting" }) cases["GEAR_"+book]=0.20f;
            foreach (var entry in cases)
            {
                var expected = Units.ItemWeight.FromKilograms(entry.Value);
                var prefab = Game.GearItem.LoadGearItemPrefab(entry.Key);
                check(prefab != null,"balance prefab exists: " + entry.Key);
                var originalData = prefab.m_GearItemData;
                var originalWeight = prefab.WeightKG;
                var baseWeight = originalData.m_BaseWeight;
                var decay = originalData.m_DailyHPDecay;
                var preview = prefab.GetSingleItemWeightKG();
                check(preview == expected && prefab.m_GearItemData.Pointer == originalData.Pointer && prefab.WeightKG == originalWeight &&
                    originalData.m_BaseWeight == baseWeight && originalData.m_DailyHPDecay == decay,"preview uses new weight without mutating authored data: " + entry.Key);
                var item = Game.GearItem.InstantiateGearItem(entry.Key);
                items.Add(item);
                item.m_HasBeenOwnedByPlayer = true;
                item.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
                item.m_LastUpdatedTODHoursInitialized = true;
                item.ManualStart();
                check(item.GetSingleItemWeightKG() == expected && item.m_GearItemData.m_BaseWeight == expected,
                    "native initialized item has requested weight: " + entry.Key);
                var proxy = item.Serialize();
                item.WeightKG = Units.ItemWeight.FromKilograms(99);
                item.Deserialize(proxy,false);
                check(item.GetSingleItemWeightKG() == expected,"saved item retains requested weight: " + entry.Key);
                if (ItemBalancePolicy.IsFlask(entry.Key))
                {
                    item.SetNormalizedHP(0.43f,false);
                    item.DecayOverTODHours(24*365,1);
                    check(item.m_GearItemData.m_DailyHPDecay == 0 && Math.Abs(item.GetNormalizedCondition()-0.43f)<0.001,
                        "flask does not lose condition over a year: " + entry.Key);
                }
            }
            phase = "stack weight";
            var stones = items.First(i => LootBalancePolicy.ItemName(i.name)=="GEAR_Stone");
            stones.m_StackableItem.m_Units = 3;
            check(stones.GetItemWeightKG() == Units.ItemWeight.FromKilograms(0.30f) && stones.GetSingleItemWeightKG() == Units.ItemWeight.FromKilograms(0.10f),
                "stacked stones use the new per-item weight");
            phase = "flask contents";
            var flaskItem = items.First(i => LootBalancePolicy.ItemName(i.name)=="GEAR_InsulatedFlask_A");
            var contents = Game.GearItem.InstantiateGearItem("GEAR_ReishiTea");
            items.Add(contents);
            flaskItem.InsulatedFlask.m_Items.Add(new Gear.GearItemObject(contents));
            flaskItem.InsulatedFlask.UpdateWeight();
            check(flaskItem.GetSingleItemWeightKG() == Units.ItemWeight.FromKilograms(0.10f) + contents.GetSingleItemWeightKG(),
                "insulated flask still carries the weight of its contents");
            phase = "spray weight";
            var spray = items.First(i => LootBalancePolicy.ItemName(i.name)=="GEAR_SprayPaintCan");
            spray.SetNormalizedHP(0.25f,false);
            check(spray.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(0.20f) && Math.Abs(spray.GetNormalizedCondition()-0.25f)<0.001,
                "partially used spray stays at requested weight without replenishing paint");
            phase = "lantern heat";
            foreach (string name in new[] { "GEAR_KeroseneLampB","GEAR_KeroseneLamp_Spelunkers" })
            {
                var lampItem = Game.GearItem.InstantiateGearItem(name);
                items.Add(lampItem);
                var lamp = lampItem.GetComponent<Gear.KeroseneLampItem>();
                check(lamp != null && lamp.m_HeatSource != null && lamp.m_HeatSource.m_MaxTempIncrease == 6,
                    "native lantern heat source supplies six degrees: " + name);
                var fuel = lamp.m_CurrentFuelLiters;
                string saved = lamp.Serialize();
                lamp.m_HeatSource.m_MaxTempIncrease=3;
                lamp.Deserialize(saved);
                check(lamp.m_HeatSource.m_MaxTempIncrease==6 && lamp.m_CurrentFuelLiters==fuel,
                    "lantern heat survives reload without changing fuel: " + name);
            }
            phase = "difficulty isolation";
            foreach (string difficulty in new[] { "Stalker","Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode = Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                var opener = Game.GearItem.InstantiateGearItem("GEAR_CanOpener");
                items.Add(opener);
                var flask = Game.GearItem.InstantiateGearItem("GEAR_InsulatedFlask_A");
                items.Add(flask);
                var lamp = Game.GearItem.InstantiateGearItem("GEAR_KeroseneLampB");
                items.Add(lamp);
                check(opener.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(0.25f) && flask.m_GearItemData.m_DailyHPDecay>0 &&
                    lamp.GetComponent<Gear.KeroseneLampItem>().m_HeatSource.m_MaxTempIncrease==3,
                    difficulty + " retains authored weight, flask decay, and lantern warmth");
            }
        }
        catch (Exception ex) { throw new InvalidOperationException("Item balance validation phase: " + phase,ex); }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            foreach (var item in items) if (item != null) UnityEngine.Object.DestroyImmediate(item.gameObject);
        }
    }
}
