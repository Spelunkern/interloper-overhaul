extern alias GameAssembly;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;
internal static class CattailBundle
{
    internal const string Marker="InterloperOverhaulCattailBundle";
    private static Game.GearItem source,prefab;
    private static Gear.GearItemData data;
    [ThreadStatic] internal static bool Crafting;
    internal static Game.GearItem Prefab()
    {
        if (prefab!=null) return prefab;
        source=Game.GearItem.LoadGearItemPrefab(FoodPreparationPolicy.Cattail);
        if (source==null) throw new InvalidOperationException("Cattail stalk prefab is unavailable.");
        data=UnityEngine.Object.Instantiate(source.m_GearItemData).Cast<Gear.GearItemData>();
        data.hideFlags=HideFlags.HideAndDontSave;
        data.name="GearItemData_IO_CattailBundle";
        data.m_BaseWeight=source.m_GearItemData.m_BaseWeight*10;
        data.m_LocalizedName=new Game.LocalizedString { m_LocalizationID="IO_CattailBundle_Name" };
        data.m_LocalizedDescription=new Game.LocalizedString { m_LocalizationID="IO_CattailBundle_Description" };
        var root=new GameObject("IO_CattailBundlePrefab");
        root.SetActive(false);
        root.hideFlags=HideFlags.HideAndDontSave;
        UnityEngine.Object.DontDestroyOnLoad(root);
        prefab=UnityEngine.Object.Instantiate(source.gameObject,root.transform).GetComponent<Game.GearItem>();
        Configure(prefab,true);
        return prefab;
    }
    internal static void Configure(Game.GearItem item,bool fresh)
    {
        if (item==null) return;
        // Asset loading happens in the factory/catalog, never recursively in Awake.
        if (data==null) Prefab();
        item.name=FoodPreparationPolicy.Bundle;
        item.m_GearItemData=data;
        item.WeightKG=data.m_BaseWeight;
        var stack=item.m_StackableItem ?? item.GetComponent<Game.StackableItem>();
        if (stack!=null)
        {
            stack.m_LocalizedSingleUnitText=new Game.LocalizedString { m_LocalizationID="IO_CattailBundle_Name" };
            stack.m_LocalizedMultipleUnitText=new Game.LocalizedString { m_LocalizationID="IO_CattailBundle_Plural" };
        }
        var food=item.m_FoodItem ?? item.GetComponent<Game.FoodItem>();
        if (food==null) throw new InvalidOperationException("Cattail bundle has no FoodItem component.");
        if (fresh) food.m_CaloriesRemaining=food.m_CaloriesTotal=source.m_FoodItem.m_CaloriesTotal*10;
        food.m_TimeToEatSeconds=source.m_FoodItem.m_TimeToEatSeconds*10;
        food.m_ReduceThirst=source.m_FoodItem.m_ReduceThirst*10;
        food.m_Nutrients=new Il2CppSystem.Collections.Generic.List<Game.FoodItem.Nutrient>();
        foreach (var nutrient in source.m_FoodItem.m_Nutrients)
            food.m_Nutrients.Add(new Game.FoodItem.Nutrient { m_Nutrient=nutrient.m_Nutrient,m_Amount=nutrient.m_Amount*10 });
        item.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
        item.m_HasBeenOwnedByPlayer=true;
    }
    internal static Game.GearItem Create()
    {
        var item=UnityEngine.Object.Instantiate(Prefab().gameObject).GetComponent<Game.GearItem>();
        item.name=FoodPreparationPolicy.Bundle;
        Configure(item,false);
        return item;
    }
    internal static List<Game.GearItem> FullStalks(Game.Inventory inventory)
    {
        var result=new List<Game.GearItem>();
        if (inventory?.m_Items==null) return result;
        foreach (var entry in inventory.m_Items)
        {
            var item=entry.m_GearItem;
            if (item!=null && item.m_InPlayerInventory && LootBalancePolicy.ItemName(item.name)==FoodPreparationPolicy.Cattail &&
                item.m_FoodItem!=null && item.m_FoodItem.m_CaloriesRemaining>=item.m_FoodItem.m_CaloriesTotal-0.001f) result.Add(item);
        }
        return result;
    }
}
[HarmonyPatch]
internal static class LoadCattailBundle
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.LoadGearItemPrefab));
    private static bool Prefix(string name,ref Game.GearItem __result)
    {
        if (!FoodPreparationPolicy.IsBundle(name)) return true;
        __result=CattailBundle.Prefab();
        return false;
    }
}
[HarmonyPatch]
internal static class InstantiateCattailBundle
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.InstantiateGearItem),new[] { typeof(string) });
    private static bool Prefix(string prefabName,ref Game.GearItem __result)
    {
        if (!FoodPreparationPolicy.IsBundle(prefabName)) return true;
        __result=CattailBundle.Create();
        return false;
    }
}
[HarmonyPatch]
internal static class ReloadCattailBundle
{
    private static IEnumerable<MethodBase> TargetMethods() => AccessTools.GetDeclaredMethods(typeof(Game.GearItem))
        .Where(m => m.Name==nameof(Game.GearItem.InstantiateAndDeserializeGearItem) && m.GetParameters()[0].ParameterType==typeof(string));
    private static void Prefix(ref string prefabName)
    {
        // Native inventory, container, and world serializers keep the custom
        // name. The FoodItem marker restores identity through every proxy path.
        if (FoodPreparationPolicy.IsBundle(prefabName)) prefabName=FoodPreparationPolicy.Cattail;
    }
}
[HarmonyPatch]
internal static class CraftCattailBundleScope
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.PlayerManager),nameof(Game.PlayerManager.InstantiateCraftedItemInPlayerInventory));
        yield return AccessTools.Method(typeof(Game.Container),nameof(Game.Container.InstantiateCraftedItem));
    }
    private static void Prefix(Gear.ICraftingResult __0,out bool __state)
    {
        __state=CattailBundle.Crafting;
        var gear=__0?.TryCast<Game.GearItem>();
        var resultData=__0?.TryCast<Gear.GearItemData>();
        CattailBundle.Crafting=ItemBalance.Active && FoodPreparationPolicy.IsBundle(gear?.name ?? resultData?.name ?? "");
        if (CattailBundle.Crafting) CattailBundle.Prefab();
    }
    private static void Finalizer(bool __state) => CattailBundle.Crafting=__state;
}
[HarmonyPatch]
internal static class ConfigureCraftedBundle
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Awake));
    [HarmonyPriority(Priority.First)]
    private static void Prefix(Game.GearItem __instance)
    {
        if (CattailBundle.Crafting && LootBalancePolicy.ItemName(__instance.name)==FoodPreparationPolicy.Cattail)
            CattailBundle.Configure(__instance,true);
    }
}
[HarmonyPatch]
internal static class SaveCattailBundleFood
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FoodItem),nameof(Game.FoodItem.Serialize));
    private static void Postfix(Game.FoodItem __instance,ref string __result)
    {
        if (!FoodPreparationPolicy.IsBundle(__instance.name) || string.IsNullOrEmpty(__result)) return;
        var json=JsonNode.Parse(__result).AsObject();
        json[CattailBundle.Marker]=true;
        __result=json.ToJsonString();
    }
}
[HarmonyPatch]
internal static class RestoreCattailBundleFood
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FoodItem),nameof(Game.FoodItem.Deserialize));
    private static void Postfix(Game.FoodItem __instance,string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        using var json=JsonDocument.Parse(text);
        if (json.RootElement.TryGetProperty(CattailBundle.Marker,out var marker) && marker.ValueKind==JsonValueKind.True)
            CattailBundle.Configure(__instance.GetComponent<Game.GearItem>(),false);
    }
}
[HarmonyPatch]
internal static class BundleLocalization
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Localization),nameof(Game.Localization.Get),new[] { typeof(string) });
    private static bool Prefix(string key,ref string __result)
    {
        if (key=="IO_CattailBundle_Name") __result="Cattail Bundle";
        else if (key=="IO_CattailBundle_Plural") __result="Cattail Bundles";
        else if (key=="IO_CattailBundle_Description") __result="Ten cattail stalks bundled into one convenient meal. The same food, weight, and nutrients as ten individual stalks.";
        else return true;
        return false;
    }
}
[HarmonyPatch]
internal static class RequireWholeBundleIngredients
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.HasRequiredMaterials));
    private static void Postfix(Gear.BlueprintData __instance,Game.Inventory inventory,Game.Container container,ref bool __result)
    {
        if (ItemBalance.Active && __instance.name==FoodPreparationPolicy.BundleRecipe.BlueprintName)
            __result &= CattailBundle.FullStalks(inventory).Sum(Manufacturing.UnitsOf)>=__instance.NumNeedsConsuming(0,__instance.GetInProgressItem(inventory,container));
    }
}
[HarmonyPatch]
internal static class ConsumeWholeBundleIngredients
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Inventory),nameof(Game.Inventory.RemoveGearFromInventory));
    private static bool Prefix(Game.Inventory __instance,string gearName,int numUnits)
    {
        if (Manufacturing.Current!=FoodPreparationPolicy.BundleRecipe || LootBalancePolicy.ItemName(gearName)!=FoodPreparationPolicy.Cattail || numUnits<=0) return true;
        var stalks=CattailBundle.FullStalks(__instance);
        if (stalks.Sum(Manufacturing.UnitsOf)<numUnits)
        {
            var operation=Manufacturing.Operation;
            if (operation!=null && Manufacturing.Cancelled.Add(operation.Pointer)) operation.Interrupt(Gear.CraftingInterruptReason.Cancelled);
            return false;
        }
        foreach (var stalk in stalks)
        {
            int consume=Math.Min(numUnits,Manufacturing.UnitsOf(stalk));
            __instance.RemoveUnits(stalk,consume);
            numUnits-=consume;
            if (numUnits==0) break;
        }
        return false;
    }
}
[HarmonyPatch]
internal static class PreserveCraftedBundleIdentity
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Inventory),nameof(Game.Inventory.AddGear));
        yield return AccessTools.Method(typeof(Game.Container),nameof(Game.Container.AddGear));
    }
    private static void Prefix(Game.GearItem __0)
    {
        // Native asset-reference instantiation renames a new object after Awake.
        // Restore its custom identity before inventory/container stack matching.
        if (__0==null) return;
        var data=__0.m_GearItemData;
        // A managed wrapper can outlive its native Unity object. ?. tests CLR
        // null only and calls Object.name on an already-unloaded asset.
        if (data!=null && FoodPreparationPolicy.IsBundle(data.name))
            CattailBundle.Configure(__0,false);
    }
}
