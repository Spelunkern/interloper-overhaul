extern alias GameAssembly;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;
internal static class PermanentAccessoryValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var previousState = PermanentAccessories.State;
        bool previousFailed = PermanentAccessories.Failed;
        var mode = Game.ExperienceModeManager.s_CurrentGameMode;
        var root = new GameObject("IO_AccessoryValidation");
        root.SetActive(false);
        var items = new List<Game.GearItem>();
        var inventory = root.AddComponent<Game.Inventory>();
        string phase = "armor";
        bool Close(Units.ItemWeight a, Units.ItemWeight b) => Math.Abs((a-b)/Units.ItemWeight.FromKilograms(1)) < 0.000001f;
        Game.GearItem Create(string name, bool add = true)
        {
            var item = Game.GearItem.InstantiateGearItem(name);
            if (item == null) throw new InvalidOperationException("Missing prefab: " + name);
            items.Add(item);
            item.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            item.m_HasBeenOwnedByPlayer = true;
            item.m_LastUpdatedTODHoursInitialized = true;
            if (add)
            {
                item.m_InPlayerInventory = item.m_BeenInPlayerInventory = true;
                inventory.m_Items.Add(new Gear.GearItemObject(item));
            }
            return item;
        }
        try
        {
            PermanentAccessories.Reset();
            var vest = Create("GEAR_BallisticVest");
            vest.ManualStart();
            check(vest.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(2) && vest.m_ClothingItem.m_Toughness==20 &&
                vest.m_ClothingItem.m_SprintBarReductionPercent==10,"riot vest has two-kilogram weight, twenty protection, and ten-percent sprint penalty");
            check(vest.m_Repairable != null,"riot vest has a native portable repair component");
            ArmorBalance.Configure(vest,true);
            var repair = vest.m_Repairable;
            repair.Start();
            ArmorBalance.Configure(vest,true);
            check(repair.GetNumMaterialsRequired()==1 && repair.GetMaterialRequired(0)!=null &&
                LootBalancePolicy.ItemName(repair.GetMaterialRequired(0).name)=="GEAR_Leather" && repair.GetRequiredGearUnits(0)==1,
                "riot vest repair consumes exactly one cured leather");
            check(!repair.RequiresToolToRepair() && repair.GetNumRepairToolChoices()==0 && repair.GetDurationMinutes()==30 && repair.GetChanceActionSuccess(null)>=100,
                "riot vest repair requires no tool or workshop and takes thirty minutes");
            vest.SetNormalizedHP(0.50f,false);
            check(Math.Abs(repair.GetConditionIncreaseFromRepair()-10)<0.001,"one leather restores ten percentage points despite the vest's 2500 maximum HP");
            vest.SetNormalizedHP(0.96f,false);
            check(Math.Abs(repair.GetConditionIncreaseFromRepair()-4)<0.001,"vest repair caps at full condition");
            var savedVest = vest.Serialize();
            vest.Deserialize(savedVest,false);
            ArmorBalance.Configure(vest,true);
            check(vest.m_ClothingItem.m_Toughness==20 && vest.m_ClothingItem.m_SprintBarReductionPercent==10 && vest.m_Repairable.GetRequiredGearUnits(0)==1,
                "native vest save preserves its balanced stats and portable repair setup");
            foreach (string name in new[] { "GEAR_MooseHideBag","GEAR_EarMuffs" })
                check(Create(name).GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(0.10f),"lighter accessory has the requested weight: " + name);
            inventory.m_Items.Clear();

            phase = "weight selection";
            var common = Create("GEAR_Rifle");
            var heavy = Create("GEAR_Rifle_Barbs");
            var light = Create("GEAR_Rifle_Vaughns");
            var revolver = Create("GEAR_Revolver");
            var bow = Create("GEAR_Bow");
            foreach (var gun in new[] { common,heavy,light,revolver }) FiniteFirearms.Empty(gun);
            PermanentAccessories.State.ToolBelt = true;
            check(PermanentAccessories.Reduction(inventory)==Units.ItemWeight.Zero,"tool belt never discounts rifles, revolvers, or bows");
            PermanentAccessories.State.RifleHolster = true;
            var expectedRifle = Units.ItemWeight.Max(common.GetSingleItemWeightKG(),Units.ItemWeight.Max(heavy.GetSingleItemWeightKG(),light.GetSingleItemWeightKG())) * 0.5f;
            check(PermanentAccessories.Reduction(inventory)==expectedRifle,"holster discounts only the heaviest rifle, across common and variant rifles");
            PermanentAccessories.State.RifleHolster = false;
            var hatchet = Create("GEAR_Hatchet");
            var improvised = Create("GEAR_HatchetImprovised");
            improvised.WeightKG=Units.ItemWeight.FromKilograms(9); // Queries restore the requested item balance.
            var hammer = Create("GEAR_Hammer");
            var hacksaw = Create("GEAR_Hacksaw");
            var prybar = Create("GEAR_Prybar");
            var woodworking = Create("GEAR_WoodworkingTools");
            var knife = Create("GEAR_Knife");
            var survivalKnife = Create("GEAR_SurvivalKnife");
            var expectedTools = (Units.ItemWeight.Max(hatchet.GetSingleItemWeightKG(),improvised.GetSingleItemWeightKG()) + hammer.GetSingleItemWeightKG() +
                hacksaw.GetSingleItemWeightKG() + prybar.GetSingleItemWeightKG() + woodworking.GetSingleItemWeightKG() +
                Units.ItemWeight.Max(knife.GetSingleItemWeightKG(),survivalKnife.GetSingleItemWeightKG()))*0.5f;
            check(Close(PermanentAccessories.Reduction(inventory),expectedTools),"tool belt has no three-type limit and selects one heaviest tool per functional type");
            inventory.m_TotalWeight=Units.ItemWeight.FromKilograms(100);
            var total = inventory.GetTotalWeightKG();
            check(Close(total,Units.ItemWeight.FromKilograms(100)-expectedTools) && inventory.GetTotalWeightKG()==total,
                "native carried weight applies the permanent reduction exactly once per query");
            PermanentAccessories.State.RifleHolster=true;
            check(Close(inventory.GetTotalWeightKG(),total-expectedRifle),"belt and holster affect separate equipment without double-discounting weapons");
            inventory.m_Items.RemoveAt(inventory.m_Items.Count-1); // Remove the heaviest knife.
            var afterDrop = PermanentAccessories.Reduction(inventory);
            check(Close(afterDrop,expectedTools+expectedRifle-(survivalKnife.GetSingleItemWeightKG()-knife.GetSingleItemWeightKG())*0.5f),
                "dropping the heaviest tool immediately selects the remaining tool");
            inventory.m_Items.Clear();
            check(PermanentAccessories.Reduction(inventory)==Units.ItemWeight.Zero,"stored or dropped tools receive no inventory reduction");

            phase = "collectible pickup";
            PermanentAccessories.Reset();
            var belt = Create("GEAR_ToolBelt",false);
            check(belt.m_WeightReductionBuff.GetWeightReduction()==Units.ItemWeight.Zero,"native wearable belt bonus cannot duplicate the permanent bonus");
            inventory.AddGear(belt);
            check(PermanentAccessories.State.ToolBelt && inventory.m_Items.Count==0,"picking up the belt permanently unlocks its bonus and consumes the physical item");
            var holster = Create("GEAR_RifleScabbardA",false);
            holster.m_ClothingItem.PutOn();
            check(!holster.m_ClothingItem.IsWearing(),"collectible holster cannot occupy an equipment slot");
            inventory.AddGear(holster);
            check(PermanentAccessories.State.RifleHolster && inventory.m_Items.Count==0,"picking up the holster permanently unlocks its bonus without a carried item");
            inventory.AddGear(Create("GEAR_ToolBelt",false));
            check(inventory.m_Items.Count==0 && PermanentAccessories.State.ToolBelt,"duplicate collectibles cannot stack unlocks or leave inventory clutter");
            var slot = new Game.SlotData();
            PermanentAccessories.Save(slot);
            check(Game.Utils.SerializeObject(slot).Contains(PermanentAccessories.SaveKey),"native whole-slot serialization retains permanent accessory unlocks");
            PermanentAccessories.Reset();
            PermanentAccessories.Read(slot);
            check(PermanentAccessories.State.ToolBelt && PermanentAccessories.State.RifleHolster,"both permanent unlocks survive native save-slot serialization");
            phase = "existing inventory migration";
            PermanentAccessories.Reset();
            var oldBelt = Create("GEAR_ToolBelt");
            var oldHolster = Create("GEAR_RifleScabbardA");
            oldBelt.m_ClothingItem.m_EquippedLayer = (Game.ClothingLayer)0;
            PermanentAccessories.Migrate(inventory);
            check(PermanentAccessories.State.ToolBelt && PermanentAccessories.State.RifleHolster && inventory.m_Items.Count==0 && !oldBelt.m_ClothingItem.IsWearing(),
                "existing carried and equipped accessories migrate into permanent unlocks without occupying a slot");
            phase = "difficulty isolation";
            foreach (string difficulty in new[] { "Stalker","Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                var otherVest=Create("GEAR_BallisticVest",false);
                check(otherVest.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(6) && otherVest.m_ClothingItem.m_Toughness==50 &&
                    otherVest.m_ClothingItem.m_SprintBarReductionPercent==40 && otherVest.m_Repairable==null,difficulty+" keeps original riot vest stats and repair restrictions");
                var otherBelt=Create("GEAR_ToolBelt",false);
                inventory.AddGear(otherBelt);
                check(inventory.m_Items.Count>0 && !PermanentAccessories.Active,difficulty+" retains physical accessory behavior");
                inventory.m_Items.Clear();
            }
        }
        catch (Exception ex) { throw new InvalidOperationException("Permanent accessory validation phase: "+phase,ex); }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            PermanentAccessories.State=previousState;
            PermanentAccessories.Failed=previousFailed;
            foreach (var item in items) if (item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
