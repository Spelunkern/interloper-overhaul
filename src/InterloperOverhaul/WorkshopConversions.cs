extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using UnityEngine;

namespace InterloperOverhaul;

internal static class WorkshopConversions
{
    internal sealed class Reservation
    {
        internal Game.GearItem Source;
        internal Game.GearItem Work;
        internal Game.Inventory Inventory;
        internal ConversionState State;
        internal bool Committed;
    }
    internal static ConversionState Record(Game.InProgressCraftItem work) => work == null ? null :
        Workshop.State.Work.FirstOrDefault(w => w.WorkGuid == Game.ObjectGuid.GetGuidFromGameObject(work.gameObject));
    internal static Game.InProgressCraftItem Find(ManufacturingRecipe recipe, Game.Inventory inventory, Game.Container container)
    {
        if (inventory?.m_Items != null)
            foreach (var entry in inventory.m_Items)
                if (entry.m_GearItem != null && entry.m_GearItem.m_InProgressCraftItem is Game.InProgressCraftItem work && Record(work)?.Recipe == recipe.BlueprintName)
                    return work;
        if (container?.m_Items != null)
            foreach (var entry in container.m_Items)
                if (entry.m_GearItem != null && entry.m_GearItem.m_InProgressCraftItem is Game.InProgressCraftItem work && Record(work)?.Recipe == recipe.BlueprintName)
                    return work;
        return null;
    }
    internal static bool Eligible(Game.GearItem item, string name)
    {
        if (item == null || !item.m_InPlayerInventory || LootBalancePolicy.ItemName(item.name) != name || item.IsWornOut() || item.m_InProgressCraftItem != null) return false;
        var gun = item.m_GunItem;
        return gun == null || (gun.m_RoundsInClip == 0 && gun.NumRoundsInClip() == 0 && gun.NumSpentCasingsInClip() == 0);
    }
    internal static Game.GearItem Source(ManufacturingRecipe recipe, Game.Inventory inventory)
    {
        if (inventory?.m_Items != null)
            foreach (var entry in inventory.m_Items)
                if (Eligible(entry.m_GearItem,recipe.Ingredients[0].Item)) return entry.m_GearItem;
        return null;
    }
    internal static Reservation Reserve(ManufacturingRecipe recipe, Game.Inventory inventory)
    {
        var source = Source(recipe,inventory);
        if (source == null) return null;
        string originalGuid = Workshop.GuidOf(source);
        if (string.IsNullOrEmpty(originalGuid))
        {
            originalGuid = System.Guid.NewGuid().ToString();
            Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(source.gameObject,originalGuid);
        }
        var previous = Manufacturing.Current;
        Game.GearItem work = null;
        try
        {
            Manufacturing.Current = recipe;
            work = Game.GearItem.InstantiateGearItem(recipe.Result);
            work.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            work.m_HasBeenOwnedByPlayer = true;
            work.SetNormalizedHP(source.GetNormalizedCondition(),false);
            FiniteFirearms.Empty(work);
            string workGuid = System.Guid.NewGuid().ToString();
            Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(work.gameObject,workGuid);
            work.m_InProgressCraftItem = work.InitializeCraftInProgress();
            var state = new ConversionState { Recipe=recipe.BlueprintName,WorkGuid=workGuid,OriginalGuid=originalGuid,Condition=source.GetNormalizedCondition() };
            Workshop.State.Work.Add(state);
            inventory.AddGear(work);
            inventory.RemoveGear(source.gameObject,false);
            source.gameObject.SetActive(false);
            return new() { Source=source,Work=work,Inventory=inventory,State=state };
        }
        catch
        {
            if (work != null)
            {
                Workshop.State.Work.RemoveAll(w => w.WorkGuid == Workshop.GuidOf(work));
                inventory.DestroyGear(work);
            }
            if (!source.m_InPlayerInventory) inventory.AddGear(source);
            throw;
        }
        finally { Manufacturing.Current = previous; }
    }
    internal static void Finish(ConversionState state, Game.GearItem result)
    {
        if (state == null || result == null) return;
        result.SetNormalizedHP(state.Condition,false);
        FiniteFirearms.Empty(result);
        result.m_HasBeenOwnedByPlayer = true;
        result.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
        Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(result.gameObject,state.OriginalGuid);
        Workshop.State.Work.Remove(state);
    }
    internal static void CloseReservation(Reservation reservation, bool success)
    {
        if (reservation == null || reservation.Committed) return;
        reservation.Committed = true;
        if (success) UnityEngine.Object.Destroy(reservation.Source.gameObject);
        else
        {
            Workshop.State.Work.Remove(reservation.State);
            reservation.Inventory.DestroyGear(reservation.Work);
            reservation.Inventory.AddGear(reservation.Source);
        }
    }
}

[HarmonyPatch]
internal static class ReserveWorkshopConversion
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.CraftingOperation),nameof(Gear.CraftingOperation.StartCrafting));
    private static bool Prefix(Gear.BlueprintData blueprintData, Game.CraftingLocationInterface craftingLocation,
        ref Game.InProgressCraftItem workInProgress, ref int targetQuantity, out WorkshopConversions.Reservation __state)
    {
        __state = null;
        var recipe = blueprintData == null ? null : VariantPolicy.Recipe(blueprintData.name);
        if (recipe == null) return true;
        if (!WorldObjects.IsInterloper || !Workshop.Unlocked(recipe) || !Workshop.AtCannery(craftingLocation)) return false;
        targetQuantity = 1;
        if (workInProgress != null) return WorkshopConversions.Record(workInProgress)?.Recipe == recipe.BlueprintName;
        var inventory = Game.GameManager.GetInventoryComponent();
        if (!blueprintData.HasRequiredMaterials(inventory,null,Game.GameManager.GetPlayerManagerComponent())) return false;
        __state = WorkshopConversions.Reserve(recipe,inventory);
        if (__state == null) return false;
        workInProgress = __state.Work.m_InProgressCraftItem;
        return true;
    }
    private static void Postfix(Gear.CraftingOperation __instance, WorkshopConversions.Reservation __state) =>
        WorkshopConversions.CloseReservation(__state,__instance.InProgress);
    private static void Finalizer(Exception __exception, WorkshopConversions.Reservation __state)
    {
        if (__exception != null) WorkshopConversions.CloseReservation(__state,false);
    }
}
[HarmonyPatch]
internal static class WorkshopProgress
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.CraftingOperation),"ApplyCraftingProgress");
    private static void Prefix(Gear.CraftingOperation __instance, out ConversionState __state) =>
        __state = WorldObjects.IsInterloper ? WorkshopConversions.Record(__instance.CurrentWorkInProgress) : null;
    private static void Postfix(Gear.CraftingOperation __instance, ConversionState __state)
    {
        if (__state == null) return;
        if (__instance.m_UnitsCompleted > 0)
            WorkshopConversions.Finish(__state,__instance.LastCraftedItem?.TryCast<Game.GearItem>());
        else if (__instance.CurrentWorkInProgress != null)
            __instance.CurrentWorkInProgress.GetComponent<Game.GearItem>()?.SetNormalizedHP(__state.Condition,false);
    }
}
[HarmonyPatch]
internal static class WorkshopInProgress
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.GetInProgressItem),new[] { typeof(Game.Inventory),typeof(Game.Container) });
    private static bool Prefix(Gear.BlueprintData __instance, Game.Inventory inventory, Game.Container container, ref Game.InProgressCraftItem __result)
    {
        var recipe = VariantPolicy.Recipe(__instance.name);
        if (recipe == null || !WorldObjects.IsInterloper) return true;
        __result = WorkshopConversions.Find(recipe,inventory,container);
        return false;
    }
}
[HarmonyPatch]
internal static class ReservedWorkshopIngredient
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.NumNeedsConsuming));
        yield return AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.NumToConsumeNow));
    }
    private static void Postfix(Gear.BlueprintData __instance, int requiredGearIndex, Game.InProgressCraftItem wip, ref int __result)
    {
        if (requiredGearIndex == 0 && WorldObjects.IsInterloper && VariantPolicy.Recipe(__instance.name) != null &&
            (WorkshopConversions.Record(wip)?.Recipe == __instance.name || Manufacturing.Current?.BlueprintName == __instance.name)) __result = 0;
    }
}
[HarmonyPatch]
internal static class WorkshopSourceRequirement
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.HasRequiredMaterials));
    private static void Postfix(Gear.BlueprintData __instance, Game.Inventory inventory, Game.Container container, ref bool __result)
    {
        var recipe = VariantPolicy.Recipe(__instance.name);
        if (!__result || recipe == null || !WorldObjects.IsInterloper) return;
        __result = !Workshop.Failed && (WorkshopConversions.Find(recipe,inventory,container) != null || WorkshopConversions.Source(recipe,inventory) != null);
    }
}
[HarmonyPatch]
internal static class DisableVanillaVariantRecipes
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.ShouldDisableForCurrentMode));
    private static void Postfix(Gear.BlueprintData __instance, ref bool __result)
    {
        if (WorldObjects.IsInterloper && VariantPolicy.Recipe(__instance.name) == null && __instance.m_CraftedResultGear != null &&
            LootBalancePolicy.IsWeaponVariant(__instance.m_CraftedResultGear.name)) __result = true;
    }
}
