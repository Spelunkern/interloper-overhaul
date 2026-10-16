extern alias GameAssembly;

using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;

internal static class ManufacturingValidation
{
    internal static void Run(Action<bool, string> check)
    {
        var mode = Game.ExperienceModeManager.s_CurrentGameMode;
        var current = Manufacturing.Current;
        var operation = Manufacturing.Operation;
        var root = new GameObject("IO_ManufacturingValidation");
        root.SetActive(false);
        var items = new List<Game.GearItem>();
        string phase = "catalog";
        Game.GearItem Create(string name)
        {
            var gear = Game.GearItem.InstantiateGearItem(name);
            gear.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            gear.m_HasBeenOwnedByPlayer = true;
            gear.m_LastUpdatedTODHoursInitialized = true;
            items.Add(gear);
            return gear;
        }
        try
        {
            var manager = Gear.BlueprintManager.Instance;
            Manufacturing.Install(manager);
            int count = manager.m_AllBlueprints.Count;
            Manufacturing.Install(manager);
            check(manager.m_AllBlueprints.Count == count, "manufacturing catalog registration is idempotent");
            var blueprints = new Dictionary<string, Gear.BlueprintData>();
            foreach (var bp in manager.m_AllBlueprints)
                if (bp != null && ManufacturingPolicy.Find(bp.name) != null) blueprints.Add(bp.name, bp);
            check(blueprints.Count == 8, "all eight native manufacturing recipes are registered without duplicate names");
            foreach (var recipe in ManufacturingPolicy.Recipes)
            {
                var bp = blueprints[recipe.BlueprintName];
                check(!bp.ShouldDisableForCurrentMode() && bp.m_AppearsInSurvivalOnly && !bp.IsLocked(false),
                    "native manufacturing blueprint is available in Interloper: " + recipe.Id);
                check(bp.CanCraftAtLocation(Game.CraftingLocation.AmmoWorkbench) && !bp.CanCraftAtLocation(Game.CraftingLocation.Workbench) &&
                    !bp.CanCraftAtLocation(Game.CraftingLocation.Anywhere), "manufacturing uses native ammunition-workbench location gating: " + recipe.Id);
                check(bp.m_CraftedResultCount == recipe.Count && bp.m_DurationMinutes == recipe.Minutes &&
                    bp.m_RequiredGear.Length == recipe.Ingredients.Length, "native recipe retains its output, duration, and material requirements: " + recipe.Id);
                check(recipe.Tools.Length == 0 || bp.m_RequiredTool != null,
                    "native recipe resolves the required cutting tool: " + recipe.Id);
                for (int i = 0; i < recipe.Ingredients.Length; i++)
                    check(bp.m_RequiredGear[i].m_Item != null && LootBalancePolicy.ItemName(bp.m_RequiredGear[i].m_Item.name) == recipe.Ingredients[i].Item &&
                        bp.m_RequiredGear[i].m_Count == recipe.Ingredients[i].Count, "native recipe consumes the authored ingredient quantity: " + recipe.Id + "/" + i);
                if (recipe.PowderRequiredKg > 0)
                    check(bp.m_RequiredPowder.Length == 1 && bp.m_RequiredPowder[0].m_Quantity == Units.ItemWeight.FromKilograms(0.05f),
                        "flare-shell recipe consumes measured native gunpowder rather than a whole can");
            }
            foreach (string difficulty in new[] { "Stalker", "Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode = Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                check(blueprints.Values.All(bp => bp.ShouldDisableForCurrentMode()), difficulty + " excludes every manufacturing recipe");
            }
            Game.ExperienceModeManager.s_CurrentGameMode = mode;

            var state = FiniteFirearms.State;
            string allocation = state?.Serialize();
            foreach (string id in new[] { "HuntingKnife", "Hatchet", "PowderFromRedFlares" })
            {
                phase = "native output " + id;
                var recipe = ManufacturingPolicy.Recipes.Single(candidate => candidate.Id == id);
                var bp = blueprints[recipe.BlueprintName];
                var craft = new Gear.CraftingOperation { Blueprint = bp };
                ManufacturingCraftScope.Prefix(craft, out var previous);
                Game.GearItem made;
                try
                {
                    var prefab = Game.GearItem.LoadGearItemPrefab(recipe.Result);
                    check(prefab != null, "manufacturing output prefab exists: " + id);
                    // Exercise the native gear factory under the same production
                    // scope without opening a survival inventory or advancing time.
                    made = Game.GearItem.InstantiateGearItem(recipe.Result);
                }
                finally { ManufacturingCraftScope.Finalizer(previous); }
                items.Add(made);
                check(made != null && made.m_HasBeenOwnedByPlayer && made.m_RolledSpawnChance && LootBalance.Retention(made.gameObject) == 1,
                    "native crafted output survives restored-loot and finite-item filtering: " + id);
                made.m_LastUpdatedTODHoursInitialized = true;
                made.ManualStart();
                check(made.gameObject.activeSelf, "crafted item survives native initialization: " + id);
                if (recipe.PowderResultKg > 0)
                {
                    check(made.m_PowderItem.m_Weight == Units.ItemWeight.FromKilograms(0.1f), "recycled flares produce exactly 100 grams of powder");
                    string serialized = made.m_PowderItem.Serialize();
                    made.m_PowderItem.m_Weight = Units.ItemWeight.Zero;
                    made.m_PowderItem.Deserialize(serialized);
                    check(made.m_PowderItem.m_Weight == Units.ItemWeight.FromKilograms(0.1f), "native powder serialization preserves the manufactured amount");
                }
            }
            check(FiniteFirearms.State == state && FiniteFirearms.State?.Serialize() == allocation,
                "manufacturing does not consume, reroll, or replace the world allocation");
            check(Manufacturing.Current == current && Manufacturing.Operation == operation, "manufacturing scope closes after native output generation");

            phase = "fresh flare inventory";
            var inventoryGo = new GameObject("IO_ManufacturingMaterials");
            inventoryGo.transform.SetParent(root.transform, false);
            var inventory = inventoryGo.AddComponent<Game.Inventory>();
            var fresh = Create("GEAR_FlareA");
            var burnt = Create("GEAR_FlareA");
            burnt.m_FlareItem.m_State = Game.FlareState.BurnedOut;
            fresh.m_InPlayerInventory = burnt.m_InPlayerInventory = true;
            fresh.m_BeenInPlayerInventory = burnt.m_BeenInPlayerInventory = true;
            inventory.m_Items.Add(new Gear.GearItemObject(burnt));
            inventory.m_Items.Add(new Gear.GearItemObject(fresh));
            var recipeWithFlare = ManufacturingPolicy.Recipes.Single(recipe => recipe.Id == "ShellFromRedFlare");
            var flareBlueprint = blueprints[recipeWithFlare.BlueprintName];
            bool allowed = true;
            RequireFreshManufacturingFlares.Postfix(flareBlueprint, inventory, null, ref allowed);
            check(allowed && Manufacturing.FreshFlares(inventory, "GEAR_FlareA").Count == 1,
                "unused flare accepted while an exhausted flare in the same inventory is excluded");
            Manufacturing.Current = recipeWithFlare;
            try { inventory.RemoveGearFromInventory("GEAR_FlareA", 1); }
            finally { Manufacturing.Current = current; }
            check(Manufacturing.FreshFlares(inventory, "GEAR_FlareA").Count == 0 && inventory.m_Items.Count == 1 &&
                inventory.m_Items[0].m_GearItem.Pointer == burnt.Pointer, "native manufacturing consumption uses the fresh flare and preserves the exhausted one");
            allowed = true;
            RequireFreshManufacturingFlares.Postfix(flareBlueprint, inventory, null, ref allowed);
            check(!allowed, "exhausted flares alone cannot satisfy manufacturing requirements");

            foreach (string name in new[] { "GEAR_FlareGun", "GEAR_Crampons" })
            {
                phase = "milling " + name;
                var gear = Create(name);
                ManufacturingMilling.Configure(gear);
                var millable = gear.m_Millable;
                check(millable != null && millable.m_CanRestoreFromWornOut && millable.m_RepairDurationMinutes == 30 &&
                    millable.m_RepairRequiredGearUnits[0] == 1, "native repair component and scrap-metal requirement configured: " + name);
                millable.ApplyRestorationProgress(0.25f);
                float progress = millable.m_RestorationProgress;
                var proxy = gear.Serialize();
                UnityEngine.Object.DestroyImmediate(gear.gameObject);
                var restored = Game.GearItem.InstantiateAndDeserializeGearItem(name, proxy);
                items.Add(restored);
                ManufacturingMilling.Configure(restored);
                check(restored.m_Millable != null && Math.Abs(restored.m_Millable.m_RestorationProgress - progress) < 0.0001f,
                    "native gear serialization preserves interrupted milling progress: " + name);
            }
            Game.ExperienceModeManager.s_CurrentGameMode = Game.ExperienceModeManager.GetGameModeFromName("Stalker");
            var ordinary = Create("GEAR_FlareGun");
            var existing = ordinary.m_Millable;
            ManufacturingMilling.Configure(ordinary);
            check(ordinary.m_Millable == existing, "milling integration does not add or modify components outside Interloper");
        }
        catch (Exception ex) { throw new InvalidOperationException("Manufacturing validation phase: " + phase, ex); }
        finally
        {
            Manufacturing.Current = current;
            Manufacturing.Operation = operation;
            Game.ExperienceModeManager.s_CurrentGameMode = mode;
            foreach (var item in items) if (item != null) UnityEngine.Object.Destroy(item.gameObject);
            UnityEngine.Object.Destroy(root);
        }
    }
}
