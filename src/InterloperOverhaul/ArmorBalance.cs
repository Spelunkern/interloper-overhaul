extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Game = GameAssembly::Il2Cpp;

namespace InterloperOverhaul;
internal static class ArmorBalance
{
    internal static bool IsVest(Game.GearItem item) => item != null && LootBalancePolicy.ItemName(item.name) == "GEAR_BallisticVest";
    internal static void Configure(Game.GearItem item, bool materials = false)
    {
        if (!ItemBalance.Active || !IsVest(item) || !item.gameObject.scene.IsValid()) return;
        if (item.m_ClothingItem != null)
        {
            item.m_ClothingItem.m_Toughness = 20;
            item.m_ClothingItem.m_SprintBarReductionPercent = 10;
        }
        var repair = item.m_Repairable ?? item.GetComponent<Game.Repairable>() ?? item.gameObject.AddComponent<Game.Repairable>();
        item.m_Repairable = repair;
        repair.m_GearItem = item;
        repair.m_ConditionIncrease = 10;
        repair.m_RepairConditionCap = 100;
        repair.m_DurationMinutes = 30;
        repair.m_RequiresToolToRepair = false;
        repair.m_NeverFail = true;
        repair.m_RepairAudio = "Play_RepairingLeatherHide";
        repair.m_RepairToolChoices = new Il2CppReferenceArray<Game.ToolsItem>(0);
        repair.m_FilteredRepairToolChoices = new Il2CppSystem.Collections.Generic.List<Game.ToolsItem>();
        repair.m_RequiredGear ??= new Il2CppReferenceArray<Game.GearItem>(0);
        repair.m_RequiredGearUnits ??= new Il2CppStructArray<int>(0);
        if (materials)
        {
            repair.m_RequiredGear = new Il2CppReferenceArray<Game.GearItem>(new[] { Game.GearItem.LoadGearItemPrefab("GEAR_Leather") });
            repair.m_RequiredGearUnits = new Il2CppStructArray<int>(new[] { 1 });
        }
    }
    internal static void PrepareInventory()
    {
        if (!ItemBalance.Active) return;
        var inventory = Game.GameManager.GetInventoryComponent();
        if (inventory?.m_Items == null) return;
        foreach (var entry in inventory.m_Items) Configure(entry.m_GearItem,true);
    }
}
[HarmonyPatch]
internal static class PrepareBalancedArmor
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Awake));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.ManualStart));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Deserialize));
    }
    private static void Prefix(Game.GearItem __instance) => ArmorBalance.Configure(__instance);
    private static void Postfix(Game.GearItem __instance) => ArmorBalance.Configure(__instance);
}
[HarmonyPatch]
internal static class PrepareArmorRepairUI
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Panel_Actions),"RefreshScrollList");
        yield return AccessTools.Method(typeof(Game.Panel_Repair),nameof(Game.Panel_Repair.Enable));
    }
    private static void Prefix() => ArmorBalance.PrepareInventory();
}
[HarmonyPatch]
internal static class PreserveArmorRepairSetup
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Repairable),nameof(Game.Repairable.Start));
    private static void Postfix(Game.Repairable __instance) => ArmorBalance.Configure(__instance.m_GearItem);
}
[HarmonyPatch]
internal static class ArmorRepairAmount
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Repairable),nameof(Game.Repairable.GetConditionIncreaseFromRepair));
    private static bool Prefix(Game.Repairable __instance, ref float __result)
    {
        if (!ItemBalance.Active || !ArmorBalance.IsVest(__instance.m_GearItem)) return true;
        __result = Math.Max(0,Math.Min(10,100 - __instance.m_GearItem.GetNormalizedCondition()*100));
        return false;
    }
}
