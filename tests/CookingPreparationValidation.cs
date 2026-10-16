extern alias GameAssembly;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;
internal static class CookingPreparationValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var mode=Game.ExperienceModeManager.s_CurrentGameMode;
        var skillsBefore=Game.GameManager.m_SkillsManager;
        var inventoryBefore=Game.GameManager.m_Inventory;
        var playerBefore=Game.GameManager.m_PlayerManager;
        var saltBefore=CuringSaltBalance.Records;
        var root=new GameObject("IO_CookingPreparationValidation");
        root.SetActive(false);
        var items=new List<Game.GearItem>();
        string phase="Cooking tiers";
        bool Close(double a,double b) => Math.Abs(a-b)<0.00001;
        Game.GearItem Create(string name)
        {
            var item=Game.GearItem.InstantiateGearItem(name);
            if (item==null) throw new InvalidOperationException("Missing item: "+name);
            items.Add(item);
            item.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            item.m_HasBeenOwnedByPlayer=true;
            item.m_LastUpdatedTODHoursInitialized=true;
            item.ManualStart();
            return item;
        }
        try
        {
            var skills=root.AddComponent<Game.SkillsManager>();
            var cook=root.AddComponent<Game.Skill_Cooking>();
            cook.m_TierPoints=new[] { 0,25,75,150,350 };
            cook.m_LevelWhereNoParasitesOrFoodPoisoning=5;
            skills.m_Skill_Cooking=cook;
            Game.GameManager.m_SkillsManager=skills;
            cook.m_CurrentPoints=249;
            check(cook.GetCurrentTierNumber()==3 && cook.m_TierPoints[4]==250,"Cooking remains level four at 249 points with the new 250-point threshold");
            cook.m_CurrentPoints=250;
            check(cook.GetCurrentTierNumber()==4 && cook.NoParasitesOrFoodPosioning(),"Cooking level five and its native immunity activate at 250 points");
            var meat=Create("GEAR_CookedMeatWolf");
            meat.SetNormalizedHP(0.01f,false);
            var scope=CookingBalance.RuinedFood(meat);
            check(scope==null && cook.NoParasitesOrFoodPosioning(),"one-percent food retains Cooking-five protection");
            meat.SetNormalizedHP(0,false);
            scope=CookingBalance.RuinedFood(meat);
            try { check(scope!=null && !cook.NoParasitesOrFoodPosioning() && cook.GetCurrentTierNumber()==4,"zero-percent food bypasses immunity without removing other Cooking-five benefits"); }
            finally { CookingBalance.Restore(scope); }
            check(cook.NoParasitesOrFoodPosioning(),"Cooking immunity is restored immediately after ruined-food checks");
            var savedCooking=cook.Serialize();
            cook.Deserialize(savedCooking);
            check(cook.GetCurrentTierNumber()==4 && cook.m_TierPoints[4]==250,"native Cooking saves retain level five at the reduced threshold");
            phase="pemmican decay";
            var pemmican=Create("GEAR_CookedBarPemmican");
            check(Close(pemmican.m_FoodItem.m_DailyHPDecayInside/pemmican.m_GearItemData.m_MaxHP,0.001) &&
                Close(pemmican.m_FoodItem.m_DailyHPDecayOutside/pemmican.m_GearItemData.m_MaxHP,0.001),"pemmican loses one tenth of one percent per day indoors and outdoors");
            pemmican.SetNormalizedHP(1,false);
            pemmican.m_HaltDecay=false;
            pemmican.m_DecayAccumulator=0;
            pemmican.DecayOverTODHours(24,1);
            check(Close(pemmican.GetNormalizedCondition()-pemmican.m_DecayAccumulator/pemmican.m_GearItemData.m_MaxHP,0.999),"native one-day pemmican decay uses the requested percentage rather than raw HP units");
            pemmican.Deserialize(pemmican.Serialize(),false);
            check(Close(pemmican.m_FoodItem.m_DailyHPDecayInside,0.025) && Close(pemmican.GetNormalizedCondition()-pemmican.m_DecayAccumulator/pemmican.m_GearItemData.m_MaxHP,0.999),"pemmican retains its condition and decay settings through native saves");
            phase="bundle properties";
            var stalk=Create(FoodPreparationPolicy.Cattail);
            var bundle=Create(FoodPreparationPolicy.Bundle);
            check(bundle.m_FoodItem.m_CaloriesTotal==1500 && bundle.m_FoodItem.m_CaloriesRemaining==1500 &&
                Close(bundle.GetSingleItemWeightKG()/Units.ItemWeight.FromKilograms(1),0.5),"new cattail bundle contains ten stalks' calories and mass");
            check(bundle.m_FoodItem.m_TimeToEatSeconds==30 && bundle.m_FoodItem.m_Nutrients.Count==stalk.m_FoodItem.m_Nutrients.Count &&
                bundle.m_FoodItem.m_Nutrients[0].m_Amount==stalk.m_FoodItem.m_Nutrients[0].m_Amount*10 &&
                (bundle.m_GearItemData.m_IconData?.Pointer ?? IntPtr.Zero)==(stalk.m_GearItemData.m_IconData?.Pointer ?? IntPtr.Zero),"bundle retains the cattail icon and ten stalks' eating time and nutrients");
            check(bundle.DisplayName.Contains("Cattail Bundle") && bundle.Description.Contains("Ten cattail"),"native bundle name and description resolve custom localization");
            bundle.m_FoodItem.m_CaloriesRemaining=600;
            var data=bundle.CreateSaveData();
            check(data.m_PrefabName.Contains("IO_CattailBundle") && data.m_SearializedGear.Contains(CattailBundle.Marker),"native world save retains custom bundle identity and marker");
            for (int reload=0;reload<3;reload++)
            {
                var restored=Game.GearItem.InstantiateAndDeserializeGearItem(data.m_PrefabName,data.m_SearializedGear);
                items.Add(restored);
                check(restored!=null && FoodPreparationPolicy.IsBundle(restored.name) && restored.m_FoodItem.m_CaloriesTotal==1500 &&
                    restored.m_FoodItem.m_CaloriesRemaining==600 && Close(restored.GetSingleItemWeightKG()/Units.ItemWeight.FromKilograms(1),0.2),
                    "native bundle factory reload preserves identity, partial calories, and proportional weight");
            }
            var nativeProxy=bundle.Serialize();
            var fromProxy=Game.GearItem.InstantiateAndDeserializeGearItem(FoodPreparationPolicy.Bundle,nativeProxy);
            items.Add(fromProxy);
            check(FoodPreparationPolicy.IsBundle(fromProxy.name) && fromProxy.m_FoodItem.m_CaloriesRemaining==600,"native proxy overload also restores custom bundle identity");
            phase="bundle recipe";
            var manager=Gear.BlueprintManager.Instance;
            Manufacturing.Install(manager);
            var blueprint=manager.m_AllBlueprints.ToArray().First(b=>b.name==FoodPreparationPolicy.BundleRecipe.BlueprintName);
            check(blueprint.CanCraftAtLocation(Game.CraftingLocation.Anywhere) && !blueprint.m_RequiresLight && !blueprint.m_RequiresLitFire &&
                blueprint.m_DurationMinutes==1 && blueprint.m_RequiredTool==null && blueprint.m_RequiredGear[0].m_Count==10,
                "bundle recipe requires ten cattails, one minute, and no tools, fire, light, or workbench");
            var inventory=root.AddComponent<Game.Inventory>();
            var player=root.AddComponent<Game.PlayerManager>();
            Game.GameManager.m_Inventory=inventory;
            Game.GameManager.m_PlayerManager=player;
            stalk.m_InPlayerInventory=true;
            stalk.m_StackableItem.m_Units=10;
            inventory.m_Items.Add(new Gear.GearItemObject(stalk));
            check(blueprint.HasRequiredMaterials(inventory,null,player),"ten whole stalks satisfy native bundle crafting requirements");
            stalk.m_FoodItem.m_CaloriesRemaining=75;
            check(!blueprint.HasRequiredMaterials(inventory,null,player),"partially eaten stalks cannot create extra calories by bundling");
            stalk.m_FoodItem.m_CaloriesRemaining=150;
            var made=player.InstantiateCraftedItemInPlayerInventory(blueprint.GetCraftedResult(),1,1,(Game.PlayerManager.InventoryInstantiateFlags)9).TryCast<Game.GearItem>();
            items.Add(made);
            check(made!=null && FoodPreparationPolicy.IsBundle(made.name) && made.m_FoodItem.m_CaloriesTotal==1500 &&
                made.m_GearItemData.Pointer!=stalk.m_GearItemData.Pointer,"native crafted-item inventory factory produces a distinct bundle rather than a single stalk");
            var craft=new Gear.CraftingOperation { Blueprint=blueprint };
            ManufacturingCraftScope.Prefix(craft,out var previous);
            try { inventory.RemoveGearFromInventory(FoodPreparationPolicy.Cattail,10); }
            finally { ManufacturingCraftScope.Finalizer(previous); }
            check(!inventory.m_Items.ToArray().Any(e=>e.m_GearItem!=null && LootBalancePolicy.ItemName(e.m_GearItem.name)==FoodPreparationPolicy.Cattail),"bundle crafting consumes exactly the ten whole stalks");
            phase="curing salt";
            CuringSaltBalance.Reset();
            var box=root.AddComponent<Game.CuringStorage>();
            box.m_State=new Game.CuringStorageState { m_GUID="IO_TestCuringBox" };
            box.m_MaxSalt=Units.ItemWeight.FromKilograms(20);
            var jar=Create("GEAR_Salt");
            var bag=Create("GEAR_SaltBag");
            box.m_AddedSalt.Add(jar);box.m_AddedSalt.Add(bag);
            check(CuringSaltBalance.Label(box).Contains("3.10 kg"),"pending jar and bag show their actual reduced combined salt mass");
            var pending=CuringSaltBalance.BeforeClose(box);
            box.m_State.m_SaltStorage=Units.ItemWeight.FromKilograms(10.25f);
            box.m_AddedSalt.Clear();
            CuringSaltBalance.AfterClose(box,pending);
            check(Close(CuringSaltBalance.Record(box).MassKg,3.1) && CuringSaltBalance.Label(box).Contains("3.10 kg"),"deposited salt preserves mixed-source mass without changing native curing quantity");
            box.m_State.m_SaltStorage*=0.5f;
            check(Close(CuringSaltBalance.Record(box).MassKg,1.55),"curing consumption reduces tracked salt mass proportionally");
            var slot=new Game.SlotData();
            CuringSaltBalance.Save(slot);
            CuringSaltBalance.Read(slot);
            check(Close(CuringSaltBalance.Record(box).MassKg,1.55) && Game.Utils.SerializeObject(slot).Contains(CuringSaltBalance.SaveKey),"mixed salt mass persists inside the native save slot");
            phase="difficulty isolation";
            foreach (string difficulty in new[] { "Stalker","Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                cook.m_CurrentPoints=250;
                var originalPemmican=Create("GEAR_CookedBarPemmican");
                check(cook.GetCurrentTierNumber()==3 && cook.m_TierPoints[4]==350 && originalPemmican.m_FoodItem.m_DailyHPDecayInside==1 &&
                    CookingBalance.RuinedFood(meat)==null && blueprint.ShouldDisableForCurrentMode() && CuringSaltBalance.Label(box)==null,
                    difficulty+" retains authored Cooking, pemmican decay, crafting catalog, and salt display");
            }
        }
        catch (Exception ex) { throw new InvalidOperationException("Cooking/preparation validation phase: "+phase,ex); }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            Game.GameManager.m_SkillsManager=skillsBefore;
            Game.GameManager.m_Inventory=inventoryBefore;
            Game.GameManager.m_PlayerManager=playerBefore;
            CuringSaltBalance.Records=saltBefore;
            foreach (var item in items) if (item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
