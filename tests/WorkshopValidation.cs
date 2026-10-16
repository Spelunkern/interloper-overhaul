extern alias GameAssembly;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterloperOverhaul;
internal static class WorkshopValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var original = Workshop.State;
        var finite = FiniteFirearms.State;
        var root = new GameObject("IO_WorkshopValidation");
        root.SetActive(false);
        var items = new List<Game.GearItem>();
        string phase = "catalog";
        try
        {
            Workshop.Reset();
            var manager = Gear.BlueprintManager.Instance;
            Gear.BlueprintData Blueprint(string name) { foreach (var bp in manager.m_AllBlueprints) if (bp.name == name) return bp; throw new InvalidOperationException(name); }
            Manufacturing.Install(manager);
            foreach (var variant in VariantPolicy.Variants)
            {
                var bp = Blueprint(variant.Upgrade.BlueprintName);
                check(bp.IsLocked(false),"variant blueprint starts locked: " + variant.Name);
                Workshop.State.Learned.Add(variant.Item);
                check(!bp.IsLocked(false) && bp.GetDisplayedNameWithCount().Contains(variant.Name) && bp.m_RequiredTool != null,"learned upgrade resolves its reusable tool: " + variant.Name);
                if (variant.Reverse != null)
                {
                    var reverse = Blueprint(variant.Reverse.BlueprintName);
                    check(!reverse.IsLocked(false) && reverse.m_DurationMinutes == variant.Reverse.Minutes && reverse.m_RequiredGear.Length == 1,
                        "learned firearm family enables time-only reversal: " + variant.Name);
                }
            }
            phase = "physical schematic";
            var card = Workshop.MakeCard(VariantPolicy.Variants[4],Guid.NewGuid().ToString());
            items.Add(card);
            card.m_LastUpdatedTODHoursInitialized = true;
            card.ManualStart();
            check(Workshop.Card(card)?.Item == "GEAR_Rifle_Vaughns" && card.GetDisplayNameWithoutConditionForInventoryInterfaces().Contains("Vaughn"),"generic physical schematic has variant-specific text");
            check(card.GetComponent<Game.BlueprintItemUnlock>() == null || !card.GetComponent<Game.BlueprintItemUnlock>().ShouldUnlock(),"schematic cannot accidentally unlock the vanilla Bushcraft recipe");
            var proxy = card.Serialize();
            UnityEngine.Object.DestroyImmediate(card.gameObject);
            card = Game.GearItem.InstantiateGearItem(Workshop.CardPrefab);
            items.Add(card);
            card.Deserialize(proxy,false);
            check(Workshop.Card(card)?.Item == "GEAR_Rifle_Vaughns", "native physical schematic serialization preserves its variant");

            phase = "authored world schematic";
            var worldScene = SceneManager.CreateScene("IO_WorkshopWorld");
            var worldRoot = new GameObject("Design");
            SceneManager.MoveGameObjectToScene(worldRoot,worldScene);
            var authored = Game.GearItem.InstantiateGearItem("GEAR_Rifle_Barbs");
            items.Add(authored);
            authored.name = "GEAR_Rifle_Barbs";
            SceneManager.MoveGameObjectToScene(authored.gameObject,worldScene);
            authored.transform.SetParent(worldRoot.transform);
            authored.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            authored.m_LastUpdatedTODHoursInitialized = true;
            check(LootBalance.Retention(authored.gameObject) == 1,"authored variant sites bypass additional rarity filtering");
            FiniteFirearms.AfterStart(authored);
            check(!authored.gameObject.activeSelf && Workshop.State.Issued.Count == 1 && Workshop.State.Cards.Count == 2,
                "authored variant site issues one physical schematic");
            authored.gameObject.SetActive(true);
            FiniteFirearms.AfterStart(authored);
            check(!authored.gameObject.activeSelf && Workshop.State.Cards.Count == 2,"revisiting an authored site cannot duplicate its schematic");
            foreach (var item in Resources.FindObjectsOfTypeAll<Game.GearItem>())
                if (item.gameObject.scene == worldScene && Workshop.Card(item) != null) items.Add(item);
            UnityEngine.Object.DestroyImmediate(worldRoot);

            phase = "Tales reward schematic";
            var cacheObject = new GameObject("TrackableHiddenCaches");
            cacheObject.SetActive(false);
            cacheObject.transform.SetParent(root.transform);
            var cache = cacheObject.AddComponent<Game.Container>();
            cache.m_GearToInstantiate.Add(Game.GearItem.LoadGearItemPrefab("GEAR_SurvivalKnife").gameObject);
            LootBalance.FilterPending(cache,0);
            check(cache.m_GearToInstantiate.Count == 0 && cache.m_Items.Count == 1 && Workshop.Card(cache.m_Items[0].m_GearItem)?.Item == "GEAR_SurvivalKnife",
                "selected Tales reward is replaced by a physical schematic");
            items.Add(cache.m_Items[0].m_GearItem);

            phase = "finite flashlights";
            FiniteFirearms.State = FirearmRunState.Create(543,"Validation/lights");
            foreach (var light in FiniteFirearms.State.Lights)
            {
                var scene = SceneManager.GetSceneByName(light.Site.Scene);
                if (!scene.IsValid()) scene = SceneManager.CreateScene(light.Site.Scene);
                var gear = FiniteFirearms.CreateWeapon(light.Item,light.Site,scene,light);
                items.Add(gear);
                gear.m_LastUpdatedTODHoursInitialized = true;
                gear.ManualStart();
                check(light.Created && gear.gameObject.activeSelf && FiniteFirearms.IsManaged(gear) && gear.m_FlashlightItem != null,
                    "native flashlight allocation survives initialization: " + light.Key);
                var saved = gear.Serialize();
                string guid = Workshop.GuidOf(gear);
                UnityEngine.Object.DestroyImmediate(gear.gameObject);
                var restored = Game.GearItem.InstantiateGearItem(light.Item);
                items.Add(restored);
                restored.Deserialize(saved,false);
                check(Workshop.GuidOf(restored) == guid && FiniteFirearms.IsManaged(restored),"native flashlight save keeps allocation identity: " + light.Key);
            }
            phase = "reservation";
            var inventory = root.AddComponent<Game.Inventory>();
            var source = Game.GearItem.InstantiateGearItem("GEAR_Rifle");
            items.Add(source);
            source.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            source.SetNormalizedHP(0.37f,false);
            source.m_InPlayerInventory = source.m_BeenInPlayerInventory = source.m_HasBeenOwnedByPlayer = true;
            FiniteFirearms.Empty(source);
            inventory.m_Items.Add(new Gear.GearItemObject(source));
            var recipe = VariantPolicy.Variants[4].Upgrade;
            var bpUpgrade = Blueprint(recipe.BlueprintName);
            check(WorkshopConversions.Source(recipe,inventory)?.Pointer == source.Pointer,"unloaded base weapon is eligible for conversion");
            source.m_GunItem.m_RoundsInClip = 1;
            check(WorkshopConversions.Source(recipe,inventory) == null,"loaded firearm cannot enter conversion");
            FiniteFirearms.Empty(source);
            var reservation = WorkshopConversions.Reserve(recipe,inventory);
            items.Add(reservation.Work);
            check(reservation != null && !source.m_InPlayerInventory && inventory.m_Items.Count == 1,"conversion reserves the source and creates exactly one work item");
            check(bpUpgrade.GetInProgressItem(inventory,null)?.Pointer == reservation.Work.m_InProgressCraftItem.Pointer &&
                bpUpgrade.NumNeedsConsuming(0,reservation.Work.m_InProgressCraftItem) == 0,"reserved base is not requested or consumed twice");
            reservation.Work.m_InProgressCraftItem.IncrementProgress(40);
            string savedState = Workshop.State.Serialize();
            var savedWork = reservation.Work.Serialize();
            string originalGuid = reservation.State.OriginalGuid;
            WorkshopConversions.CloseReservation(reservation,true);
            UnityEngine.Object.DestroyImmediate(source.gameObject);
            inventory.RemoveGear(reservation.Work.gameObject,false);
            UnityEngine.Object.DestroyImmediate(reservation.Work.gameObject);
            Workshop.State = WorkshopRunState.Deserialize(savedState);
            var reloaded = Game.GearItem.InstantiateGearItem(recipe.Result);
            items.Add(reloaded);
            reloaded.Deserialize(savedWork,false);
            reloaded.m_InPlayerInventory = reloaded.m_BeenInPlayerInventory = true;
            inventory.m_Items.Add(new Gear.GearItemObject(reloaded));
            var work = bpUpgrade.GetInProgressItem(inventory,null);
            check(work != null && Math.Abs(work.GetPercentCompleteNormalized() - 0.4f) < 0.001f && WorkshopConversions.Record(work)?.Condition == 0.37f,
                "native interrupted conversion reloads its progress, recipe, and original condition");
            var wrongRecipe = Blueprint(VariantPolicy.Variants[5].Upgrade.BlueprintName);
            check(wrongRecipe.GetInProgressItem(inventory,null) == null,"other upgrades cannot claim this work item");
            WorkshopConversions.Finish(WorkshopConversions.Record(work),reloaded);
            check(Math.Abs(reloaded.GetNormalizedCondition() - 0.37f) < 0.001f && Workshop.GuidOf(reloaded) == originalGuid &&
                reloaded.m_GunItem.NumRoundsInClip() == 0 && Workshop.State.Work.Count == 0,"completion keeps condition and original identity without ammunition");
            phase = "firearm reversal";
            UnityEngine.Object.DestroyImmediate(reloaded.m_InProgressCraftItem);
            reloaded.m_InProgressCraftItem = null;
            var reverseReservation = WorkshopConversions.Reserve(VariantPolicy.Variants[4].Reverse,inventory);
            items.Add(reverseReservation.Work);
            check(reverseReservation != null && reverseReservation.State.OriginalGuid == originalGuid,
                "reversion reserves the same firearm identity");
            WorkshopConversions.CloseReservation(reverseReservation,true);
            UnityEngine.Object.DestroyImmediate(reloaded.gameObject);
            WorkshopConversions.Finish(reverseReservation.State,reverseReservation.Work);
            check(Math.Abs(reverseReservation.Work.GetNormalizedCondition() - 0.37f) < 0.001f &&
                Workshop.GuidOf(reverseReservation.Work) == originalGuid && reverseReservation.Work.m_GunItem.NumRoundsInClip() == 0,
                "time-only reversion preserves condition and identity without free ammunition");
            var reverseBlueprint = Blueprint(VariantPolicy.Variants[4].Reverse.BlueprintName);
            var completedOperation = new Gear.CraftingOperation { Blueprint=reverseBlueprint };
            ManufacturingCraftScope.Prefix(completedOperation,out var previousScope);
            try
            {
                check(reverseBlueprint.NumToConsumeNow(0,null,100) == 0,
                    "completion cannot consume the returned common firearm after the work record is removed");
            }
            finally { ManufacturingCraftScope.Finalizer(previousScope); }
            phase = "reservation rollback";
            UnityEngine.Object.DestroyImmediate(reverseReservation.Work.m_InProgressCraftItem);
            reverseReservation.Work.m_InProgressCraftItem = null;
            var rollback = WorkshopConversions.Reserve(recipe,inventory);
            items.Add(rollback.Work);
            WorkshopConversions.CloseReservation(rollback,false);
            check(reverseReservation.Work.m_InPlayerInventory && Workshop.State.Work.Count == 0 && inventory.m_Items.Count == 1,
                "a failed start returns the original weapon and removes the unfinished output");
            phase = "schematic pickup";
            Workshop.State.Learned.Remove("GEAR_Rifle_Vaughns");
            inventory.AddGear(card);
            check(Workshop.State.Learned.Contains("GEAR_Rifle_Vaughns") && inventory.m_Items.Count == 1,
                "native inventory pickup learns the schematic without retaining inventory clutter");
            phase = "Trader schematic delivery";
            Workshop.State.Learned.Remove("GEAR_Rifle_Trader");
            var traderReward = Game.GearItem.InstantiateGearItem("GEAR_Rifle_Trader");
            items.Add(traderReward);
            traderReward.m_HasBeenOwnedByPlayer = true;
            inventory.AddGear(traderReward);
            check(Workshop.State.Learned.Contains("GEAR_Rifle_Trader") && inventory.m_Items.Count == 1,
                "receiving a new Trader variant learns its schematic without issuing a firearm");
            phase = "save slot";
            var slot = new Game.SlotData();
            Workshop.Save(slot);
            Workshop.Read(slot);
            check(Workshop.State.Learned.Count == 11 && Workshop.State.Cards.Count == 3,"native save slot retains learned schematics and physical card mapping");
        }
        catch (Exception ex) { throw new InvalidOperationException("Workshop validation phase: " + phase,ex); }
        finally
        {
            Workshop.State = original;
            FiniteFirearms.State = finite;
            foreach (var gear in items) if (gear != null) UnityEngine.Object.DestroyImmediate(gear.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
