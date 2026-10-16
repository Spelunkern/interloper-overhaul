extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;

internal static class ItemBalance
{
    // Native weight/decay paths inline gear-data fields. Private shared clones
    // update those paths without modifying the authored assets for other modes.
    private static readonly Dictionary<int,Gear.GearItemData> data = new();
    internal static bool Active => LootBalance.Active;
    internal static Gear.GearItemData BalancedData(Gear.GearItemData original, double weight, bool flask)
    {
        if (data.TryGetValue(original.GetInstanceID(),out var cached)) return cached;
        var clone = UnityEngine.Object.Instantiate(original).Cast<Gear.GearItemData>();
        clone.name = original.name;
        clone.m_BaseWeight = Units.ItemWeight.FromKilograms((float)weight);
        if (flask) clone.m_DailyHPDecay = 0;
        data[original.GetInstanceID()] = clone;
        data[clone.GetInstanceID()] = clone;
        return clone;
    }
    internal static void Apply(Game.GearItem item)
    {
        if (!Active || item == null || !item.gameObject.scene.IsValid()) return;
        double weight = ItemBalancePolicy.WeightKg(item.name);
        if (weight < 0 || item.m_GearItemData == null) return;
        bool flask = ItemBalancePolicy.IsFlask(item.name);
        item.m_GearItemData = BalancedData(item.m_GearItemData,weight,flask);
        item.WeightKG = Units.ItemWeight.FromKilograms((float)weight);
        if (item.m_SprayPaintCan != null) item.m_SprayPaintCan.m_FullCanLiquidWeightKG = Units.ItemWeight.Zero;
        if (flask && item.InsulatedFlask != null && item.InsulatedFlask.m_GearItem != null)
            item.InsulatedFlask.UpdateWeight(); // Preserve the native weight of its contents.
    }
    internal static void Lantern(Gear.KeroseneLampItem lamp)
    {
        if (Active && lamp != null && lamp.m_HeatSource != null)
            lamp.m_HeatSource.m_MaxTempIncrease = ItemBalancePolicy.LanternWarmthCelsius;
    }
}

[HarmonyPatch]
internal static class BalanceItemInstances
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Awake));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.ManualStart));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Deserialize));
    }
    private static void Prefix(Game.GearItem __instance) => ItemBalance.Apply(__instance);
    private static void Postfix(Game.GearItem __instance) => ItemBalance.Apply(__instance);
}

[HarmonyPatch]
internal static class BalanceItemWeightQueries
{
    internal sealed class Preview
    {
        internal Gear.GearItemData Data;
        internal Units.ItemWeight Weight;
        internal Units.ItemWeight PaintWeight;
    }
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetItemWeightKG),new[] { typeof(float),typeof(bool) });
    private static void Prefix(Game.GearItem __instance, out Preview __state)
    {
        __state = null;
        if (!ItemBalance.Active) return;
        double weight = ItemBalancePolicy.WeightKg(__instance.name);
        if (weight < 0 || __instance.m_GearItemData == null) return;
        if (__instance.gameObject.scene.IsValid()) { ItemBalance.Apply(__instance); return; }
        // Item previews use canonical prefabs: scope changes to this synchronous
        // query so switching difficulty cannot inherit altered prefab settings.
        __state = new() { Data=__instance.m_GearItemData,Weight=__instance.WeightKG,
            PaintWeight=__instance.m_SprayPaintCan?.m_FullCanLiquidWeightKG ?? Units.ItemWeight.Zero };
        __instance.m_GearItemData = ItemBalance.BalancedData(__instance.m_GearItemData,weight,ItemBalancePolicy.IsFlask(__instance.name));
        __instance.WeightKG = Units.ItemWeight.FromKilograms((float)weight);
        if (__instance.m_SprayPaintCan != null) __instance.m_SprayPaintCan.m_FullCanLiquidWeightKG = Units.ItemWeight.Zero;
    }
    private static void Finalizer(Game.GearItem __instance, Preview __state)
    {
        if (__state == null) return;
        __instance.m_GearItemData = __state.Data;
        __instance.WeightKG = __state.Weight;
        if (__instance.m_SprayPaintCan != null) __instance.m_SprayPaintCan.m_FullCanLiquidWeightKG = __state.PaintWeight;
    }
}

[HarmonyPatch]
internal static class BalanceLanternHeat
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Gear.KeroseneLampItem),nameof(Gear.KeroseneLampItem.Awake));
        yield return AccessTools.Method(typeof(Gear.KeroseneLampItem),nameof(Gear.KeroseneLampItem.Deserialize));
        yield return AccessTools.Method(typeof(Gear.KeroseneLampItem),"TurnOn");
    }
    private static void Postfix(Gear.KeroseneLampItem __instance) => ItemBalance.Lantern(__instance);
}
