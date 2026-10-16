extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;

internal static class Manufacturing
{
    private static bool installing;
    [ThreadStatic] internal static ManufacturingRecipe Current;
    [ThreadStatic] internal static Gear.CraftingOperation Operation;
    internal static readonly HashSet<IntPtr> Cancelled = new();

    internal static void Install(Gear.BlueprintManager manager)
    {
        if (installing || manager == null || manager.m_AllBlueprints == null) return;
        var templates = manager.m_AddressableBlueprints;
        if (templates == null || templates.Count == 0) return;
        manager.m_UserBlueprints ??= new Il2CppSystem.Collections.Generic.List<Gear.BlueprintData>();
        installing = true;
        try
        {
            foreach (var recipe in ManufacturingPolicy.Recipes.Concat(VariantPolicy.Recipes))
            {
                bool exists = false;
                foreach (var bp in manager.m_AllBlueprints)
                    if (bp != null && bp.name == recipe.BlueprintName) { exists = true; break; }
                if (exists) continue;
                // Clone an authored blueprint so audio and Unity references use
                // current native assets, without the brittle user-JSON audio factory.
                var blueprint = UnityEngine.Object.Instantiate(templates[0]).Cast<Gear.BlueprintData>();
                blueprint.name = recipe.BlueprintName;
                blueprint.SetCraftedResult(Game.GearItem.LoadGearItemPrefab(recipe.Result));
                blueprint.m_CraftingIcon = null;
                blueprint.m_CraftedResultCount = recipe.Count;
                blueprint.m_DurationMinutes = recipe.Minutes;
                blueprint.m_RequiredCraftingLocation = Game.CraftingLocation.AmmoWorkbench;
                blueprint.m_RequiresLight = true;
                blueprint.m_RequiresLitFire = false;
                blueprint.m_AppliedSkill = Game.SkillType.None;
                blueprint.m_ImprovedSkill = Game.SkillType.None;
                blueprint.m_CanIncreaseRepairSkill = false;
                blueprint.m_UsesPhoto = false;
                blueprint.m_Locked = false;
                blueprint.Locked = false;
                blueprint.m_IgnoreLockInSurvival = true;
                blueprint.m_AppearsInStoryOnly = false;
                blueprint.m_AppearsInSurvivalOnly = true;
                blueprint.m_RequiredGear = new Il2CppReferenceArray<Gear.BlueprintData.RequiredGearItem>(
                    recipe.Ingredients.Select(input => new Gear.BlueprintData.RequiredGearItem
                    {
                        m_Item = Game.GearItem.LoadGearItemPrefab(input.Item), m_Count = input.Count,
                        m_Units = Gear.BlueprintData.RequiredGearItem.Units.Count, m_Weight = Units.ItemWeight.Zero
                    }).ToArray());
                blueprint.m_RequiredTool = recipe.Tools.Length == 0 ? null : Game.GearItem.LoadGearItemPrefab(recipe.Tools[0]).GetComponent<Game.ToolsItem>();
                blueprint.m_OptionalTools = new Il2CppReferenceArray<Game.ToolsItem>(recipe.Tools.Skip(1)
                    .Select(tool => Game.GearItem.LoadGearItemPrefab(tool).GetComponent<Game.ToolsItem>()).ToArray());
                blueprint.m_FilteredOptionalTools = null;
                blueprint.m_RequiredPowder = new Il2CppReferenceArray<Gear.BlueprintData.RequiredPowder>(0);
                blueprint.m_RequiredLiquid = new Il2CppReferenceArray<Gear.BlueprintData.RequiredLiquid>(0);
                blueprint.m_XPModesToDisable = new Il2CppSystem.Collections.Generic.List<Game.ExperienceModeType>();
                foreach (var mode in Enum.GetValues<Game.ExperienceModeType>())
                    if (mode != Game.ExperienceModeType.Interloper) blueprint.m_XPModesToDisable.Add(mode);
                if (recipe.PowderRequiredKg > 0)
                    blueprint.m_RequiredPowder = new Il2CppReferenceArray<Gear.BlueprintData.RequiredPowder>(new[]
                    {
                        new Gear.BlueprintData.RequiredPowder
                        {
                            m_Powder = Game.GearItem.LoadGearItemPrefab("GEAR_GunpowderCan").GetComponent<Gear.PowderItem>().m_Type,
                            m_Quantity = Units.ItemWeight.FromKilograms((float)recipe.PowderRequiredKg)
                        }
                    });
                manager.m_UserBlueprints.Add(blueprint);
                manager.m_AllBlueprints.Add(blueprint);
            }
        }
        finally { installing = false; }
    }

    internal static void PreserveOutput(Game.GearItem gear)
    {
        var recipe = Current;
        if (recipe == null || !WorldObjects.IsInterloper || LootBalancePolicy.ItemName(gear.name) != recipe.Result) return;
        gear.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
        gear.m_HasBeenOwnedByPlayer = true;
        if (recipe.PowderResultKg > 0)
        {
            var powder = gear.m_PowderItem ?? gear.GetComponent<Gear.PowderItem>();
            if (powder != null) powder.m_Weight = Units.ItemWeight.FromKilograms((float)recipe.PowderResultKg);
        }
    }

    internal static List<Game.GearItem> FreshFlares(Game.Inventory inventory, string name)
    {
        var result = new List<Game.GearItem>();
        if (inventory?.m_Items == null) return result;
        foreach (var entry in inventory.m_Items)
        {
            var gear = entry.m_GearItem;
            if (gear != null && gear.m_InPlayerInventory && LootBalancePolicy.ItemName(gear.name) == name && !gear.IsWornOut() &&
                gear.m_FlareItem != null && gear.m_FlareItem.IsFresh()) result.Add(gear);
        }
        return result;
    }

    internal static int UnitsOf(Game.GearItem gear) => gear.m_StackableItem?.m_Units ?? 1;
}

[HarmonyPatch]
internal static class RegisterManufacturing
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintManager), nameof(Gear.BlueprintManager.LoadAllUserBlueprints));
    private static void Postfix(Gear.BlueprintManager __instance) => Manufacturing.Install(__instance);
}

[HarmonyPatch]
internal static class EnsureManufacturingCatalog
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintManager), nameof(Gear.BlueprintManager.GetFilteredBlueprints));
    private static void Prefix(Gear.BlueprintManager __instance) => Manufacturing.Install(__instance);
}

[HarmonyPatch]
internal static class ManufacturingCraftScope
{
    internal readonly struct Previous
    {
        internal readonly ManufacturingRecipe Recipe;
        internal readonly Gear.CraftingOperation Operation;
        internal Previous(ManufacturingRecipe recipe, Gear.CraftingOperation operation) { Recipe = recipe; Operation = operation; }
    }
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Gear.CraftingOperation), "ApplyCraftingProgress");
        yield return AccessTools.Method(typeof(Gear.CraftingOperation), "ConsumeMaterialsUsedForCrafting");
    }
    internal static bool Prefix(Gear.CraftingOperation __instance, out Previous __state)
    {
        __state = new(Manufacturing.Current, Manufacturing.Operation);
        Manufacturing.Current = WorldObjects.IsInterloper && __instance.Blueprint != null ? ManufacturingPolicy.Find(__instance.Blueprint.name) : null;
        Manufacturing.Operation = __instance;
        return !Manufacturing.Cancelled.Contains(__instance.Pointer);
    }
    internal static void Finalizer(Previous __state)
    {
        Manufacturing.Current = __state.Recipe;
        Manufacturing.Operation = __state.Operation;
    }
}

[HarmonyPatch]
internal static class BeginManufacturing
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.CraftingOperation), nameof(Gear.CraftingOperation.StartCrafting));
    private static void Prefix(Gear.CraftingOperation __instance) => Manufacturing.Cancelled.Remove(__instance.Pointer);
}

[HarmonyPatch]
internal static class RequireFreshManufacturingFlares
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData), nameof(Gear.BlueprintData.HasRequiredMaterials));
    internal static void Postfix(Gear.BlueprintData __instance, Game.Inventory inventory, Game.Container container, ref bool __result)
    {
        if (!__result || !WorldObjects.IsInterloper) return;
        var recipe = ManufacturingPolicy.Find(__instance.name);
        if (recipe == null || recipe.FlareInput == "") return;
        int needed = 0;
        for (int i = 0; i < __instance.m_RequiredGear.Length; i++)
            if (LootBalancePolicy.ItemName(__instance.m_RequiredGear[i].m_Item.name) == recipe.FlareInput)
                needed += __instance.NumNeedsConsuming(i, __instance.GetInProgressItem(inventory, container));
        __result = Manufacturing.FreshFlares(inventory, recipe.FlareInput).Sum(Manufacturing.UnitsOf) >= needed;
    }
}

[HarmonyPatch]
internal static class ConsumeFreshManufacturingFlares
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Inventory), nameof(Game.Inventory.RemoveGearFromInventory));
    private static bool Prefix(Game.Inventory __instance, string gearName, int numUnits)
    {
        if (Manufacturing.Current == null || LootBalancePolicy.ItemName(gearName) != Manufacturing.Current.FlareInput || numUnits <= 0) return true;
        var flares = Manufacturing.FreshFlares(__instance, LootBalancePolicy.ItemName(gearName));
        if (flares.Sum(Manufacturing.UnitsOf) < numUnits)
        {
            var operation = Manufacturing.Operation;
            if (operation != null && Manufacturing.Cancelled.Add(operation.Pointer)) operation.Interrupt(Gear.CraftingInterruptReason.Cancelled);
            return false;
        }
        foreach (var flare in flares)
        {
            int consume = Math.Min(numUnits, Manufacturing.UnitsOf(flare));
            __instance.RemoveUnits(flare, consume);
            numUnits -= consume;
            if (numUnits == 0) break;
        }
        return false;
    }
}
