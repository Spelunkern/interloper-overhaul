extern alias GameAssembly;
using System.Text.Json.Nodes;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;
internal static class ClothingNutritionValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var mode=Game.ExperienceModeManager.s_CurrentGameMode;
        var items=new List<Game.GearItem>();
        var root=new GameObject("IO_NutritionValidation");
        root.SetActive(false);
        var inventory=root.AddComponent<Game.Inventory>();
        var ingredientTotal=Units.ItemWeight.Zero;
        string phase="clothing";
        bool Close(Units.ItemWeight a,Units.ItemWeight b) => Math.Abs((a-b)/Units.ItemWeight.FromKilograms(1))<0.00001f;
        Game.GearItem Create(string name)
        {
            var gear=Game.GearItem.InstantiateGearItem(name);
            if (gear==null) throw new InvalidOperationException("Missing prefab: "+name);
            items.Add(gear);
            gear.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            gear.m_HasBeenOwnedByPlayer=true;
            gear.m_LastUpdatedTODHoursInitialized=true;
            gear.ManualStart();
            return gear;
        }
        try
        {
            var weights=new Dictionary<string,float>
            {
                ["GEAR_RabbitskinHat"]=0.25f,["GEAR_MinersHelmet"]=0.50f,["GEAR_BearSkinCoat"]=4f,
                ["GEAR_MooseHideCloak"]=3.50f,["GEAR_WolfSkinCape"]=2.50f,["GEAR_WolfSkinPant"]=2f,
                ["GEAR_DeerSkinPants"]=1.50f,["GEAR_ImprovisedCrampons"]=1f,
                ["GEAR_CondensedMilk"]=0.20f,["GEAR_DogFood"]=0.20f,["GEAR_PinnacleCanPeaches"]=0.25f,
                ["GEAR_Soda"]=0.20f,["GEAR_SodaOrange"]=0.20f,["GEAR_SodaGrape"]=0.20f,["GEAR_SodaEnergy"]=0.20f,
                ["GEAR_CannedCorn"]=0.25f,["GEAR_CannedPineapple"]=0.25f,["GEAR_Pickles"]=0.50f,
                ["GEAR_Potato"]=0.10f,["GEAR_PotatoCooked"]=0.10f
            };
            foreach (var entry in weights)
            {
                var gear=Create(entry.Key);
                var expected=Units.ItemWeight.FromKilograms(entry.Value);
                check(Close(gear.GetSingleItemWeightKG(),expected),"requested native clothing/food weight: "+entry.Key);
                var saved=gear.Serialize();
                gear.Deserialize(saved,false);
                check(Close(gear.GetSingleItemWeightKG(),expected),"clothing/food weight persists through native gear saves: "+entry.Key);
            }
            foreach (var adjustment in ClothingNutritionPolicy.Clothing)
            {
                var gear=items.FirstOrDefault(i => LootBalancePolicy.ItemName(i.name)==adjustment.Item) ?? Create(adjustment.Item);
                var cloth=gear.m_ClothingItem;
                check(cloth!=null && (!adjustment.Warmth.HasValue || cloth.m_Warmth==adjustment.Warmth) &&
                    (!adjustment.Wind.HasValue || cloth.m_Windproof==adjustment.Wind) &&
                    (!adjustment.Protection.HasValue || cloth.m_Toughness==adjustment.Protection) &&
                    (!adjustment.SprintPenalty.HasValue || cloth.m_SprintBarReductionPercent==adjustment.SprintPenalty),
                    "native clothing stats match the requested changes: "+adjustment.Item);
            }
            var crampons=items.First(i => LootBalancePolicy.ItemName(i.name)=="GEAR_ImprovisedCrampons");
            check(crampons.m_ClimbingBuff.m_SprainRollOddsModifier==30 && crampons.m_ClimbingBuff.m_StaminaPerSecondModifier==40 &&
                crampons.m_ClimbingBuff.m_WeakIceTimeSecondsModifier==0.5f,"improvised crampon climbing benefits survive the protection and mobility changes");
            phase="food calorie migration";
            foreach (var adjustment in ClothingNutritionPolicy.Calories)
            {
                var gear=Create(adjustment.Item);
                var food=gear.m_FoodItem;
                check(food!=null && Math.Abs(food.m_CaloriesTotal-adjustment.Target)<0.01 && Math.Abs(food.m_CaloriesRemaining-adjustment.Target)<0.01,
                    "fresh food has the requested total calories: "+adjustment.Item);
                food.m_CaloriesTotal*=1.25f; // Native cooking bonus must survive.
                food.m_CaloriesRemaining=food.m_CaloriesTotal*0.40f;
                string saved=food.Serialize();
                check(saved.Contains(ClothingNutrition.CalorieMarker),"native food save records its calorie balance baseline: "+adjustment.Item);
                for (int reload=0;reload<3;reload++) food.Deserialize(saved);
                check(Math.Abs(food.m_CaloriesTotal-adjustment.Target*1.25f)<0.01 && Math.Abs(food.m_CaloriesRemaining-adjustment.Target*0.50f)<0.01,
                    "repeated reloads preserve partial food and cooking bonuses without multiplying calories: "+adjustment.Item);
                var legacy=JsonNode.Parse(saved).AsObject();
                legacy.Remove(ClothingNutrition.CalorieMarker);
                legacy["m_CaloriesTotal"]=adjustment.Original*1.25f;
                legacy["m_CaloriesRemainingProxy"]=adjustment.Original*0.50f;
                food.Deserialize(legacy.ToJsonString());
                check(Math.Abs(food.m_CaloriesTotal-adjustment.Target*1.25f)<0.01 && Math.Abs(food.m_CaloriesRemaining-adjustment.Target*0.50f)<0.01,
                    "legacy food migrates once while preserving the consumed proportion and cooking bonus: "+adjustment.Item);
                food.m_CaloriesRemaining=0;
                food.Deserialize(food.Serialize());
                check(food.m_CaloriesRemaining==0,"empty food is never refilled during save migration: "+adjustment.Item);
            }
            phase="ingredient mass";
            foreach (var entry in new Dictionary<string,float> { ["GEAR_CookingOil"]=0.50f,["GEAR_Flour"]=1f,["GEAR_OatsTin"]=0.75f,["GEAR_Salt"]=0.10f,["GEAR_SaltBag"]=3f })
            {
                var gear=Create(entry.Key);
                var desired=Units.ItemWeight.FromKilograms(entry.Value);
                check(Close(gear.GetSingleItemWeightKG(),desired),"full ingredient container has the requested carried weight: "+entry.Key);
                if (gear.m_PowderItem!=null)
                {
                    var capacity=gear.m_PowderItem.m_WeightLimit;
                    var original=gear.m_PowderItem.m_Weight;
                    gear.m_PowderItem.RemovePowder(original*0.5f);
                    check(Close(gear.GetSingleItemWeightKG(),desired*0.5f) && gear.m_PowderItem.m_WeightLimit==capacity &&
                        Close(gear.m_PowderItem.m_Weight,original*0.5f),"partial ingredient weight scales without altering recipe quantities or capacity: "+entry.Key);
                }
                else
                {
                    var liquid=gear.m_LiquidItem;
                    var capacity=liquid.m_LiquidCapacity;
                    liquid.RemoveLiquid(liquid.m_Liquid*0.5f,out _);
                    check(Close(gear.GetSingleItemWeightKG(),desired*0.5f) && liquid.m_LiquidCapacity==capacity,
                        "partial oil weighs proportionally without changing bottle capacity");
                }
                var saved=gear.Serialize();
                gear.Deserialize(saved,false);
                check(Close(gear.GetSingleItemWeightKG(),desired*0.5f),"ingredient quantities and reduced carried weight survive native saves: "+entry.Key);
                gear.m_InPlayerInventory=gear.m_BeenInPlayerInventory=true;
                inventory.m_Items.Add(new Gear.GearItemObject(gear));
                ingredientTotal+=desired*0.5f;
            }
            inventory.ProcessItems();
            check(Close(inventory.GetTotalWeightKG(),ingredientTotal),"native inventory aggregation uses the reduced ingredient weights");
            phase="drink effects";
            var coffee=Create("GEAR_CoffeeCup");
            var acorn=Create("GEAR_AcornCoffeeCup");
            check(acorn.m_FatigueBuff.m_InitialPercentDecrease==coffee.m_FatigueBuff.m_InitialPercentDecrease &&
                acorn.m_FatigueBuff.m_RateOfIncreaseScale==coffee.m_FatigueBuff.m_RateOfIncreaseScale &&
                acorn.m_FatigueBuff.m_DurationHours==coffee.m_FatigueBuff.m_DurationHours,"acorn coffee matches normal coffee intensity and duration");
            var birch=Create("GEAR_BirchbarkTea");
            var burdock=Create("GEAR_BurdockTea");
            check(burdock.m_ConditionOverTimeBuff!=null && burdock.m_ConditionOverTimeBuff.m_HUD!=null &&
                burdock.m_ConditionOverTimeBuff.m_ConditionIncreasePerHour==birch.m_ConditionOverTimeBuff.m_ConditionIncreasePerHour &&
                burdock.m_ConditionOverTimeBuff.m_NumHours==birch.m_ConditionOverTimeBuff.m_NumHours && burdock.m_FirstAidItem!=null,
                "burdock retains its medicine and gains the complete native birch-bark restoration buff");
            var savedTea=burdock.Serialize();
            burdock.Deserialize(savedTea,false);
            check(burdock.m_ConditionOverTimeBuff.m_ConditionIncreasePerHour==2.5f && burdock.m_ConditionOverTimeBuff.m_NumHours==2,
                "burdock's added restoration component survives native gear deserialization");
            phase="difficulty isolation";
            foreach (string difficulty in new[] { "Stalker","Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                var bear=Create("GEAR_BearSkinCoat");
                var ham=Create("GEAR_CannedHam");
                var plainAcorn=Create("GEAR_AcornCoffeeCup");
                var plainTea=Create("GEAR_BurdockTea");
                var salt=Create("GEAR_Salt");
                check(bear.m_ClothingItem.m_Warmth==6 && bear.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(5) &&
                    ham.m_FoodItem.m_CaloriesTotal==550 && plainAcorn.m_FatigueBuff.m_InitialPercentDecrease==5 &&
                    plainTea.m_ConditionOverTimeBuff==null && salt.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(0.25f),
                    difficulty+" retains authored clothing, calories, drink effects, and ingredient mass");
            }
        }
        catch (Exception ex) { throw new InvalidOperationException("Clothing/nutrition validation phase: "+phase,ex); }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            UnityEngine.Object.DestroyImmediate(root);
            foreach (var item in items) if (item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
        }
    }
}
