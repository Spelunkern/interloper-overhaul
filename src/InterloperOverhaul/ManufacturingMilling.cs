extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Game = GameAssembly::Il2Cpp;

namespace InterloperOverhaul;

internal static class ManufacturingMilling
{
    internal static void Attach(Game.GearItem gear)
    {
        if (!WorldObjects.IsInterloper || !ManufacturingPolicy.IsMillable(gear.name) || !gear.gameObject.scene.IsValid()) return;
        // Attach before gear deserialization so native restoration progress survives.
        // Resolve materials only when opening the milling UI, outside asset loading.
        var millable = gear.m_Millable ?? gear.GetComponent<Game.Millable>() ?? gear.gameObject.AddComponent<Game.Millable>();
        gear.m_Millable = millable;
        millable.m_CanRestoreFromWornOut = true;
    }

    internal static void Configure(Game.GearItem gear)
    {
        Attach(gear);
        if (!WorldObjects.IsInterloper || !ManufacturingPolicy.IsMillable(gear.name) || gear.m_Millable == null) return;
        var scrap = Game.GearItem.LoadGearItemPrefab("GEAR_ScrapMetal");
        var millable = gear.m_Millable;
        bool crampons = LootBalancePolicy.ItemName(gear.name) == "GEAR_Crampons";
        millable.m_RepairDurationMinutes = 30;
        millable.m_RecoveryDurationMinutes = crampons ? 210 : 180;
        millable.m_RepairRequiredGear = new Il2CppReferenceArray<Game.GearItem>(new[] { scrap });
        millable.m_RepairRequiredGearUnits = new Il2CppStructArray<int>(new[] { 1 });
        millable.m_RestoreRequiredGear = new Il2CppReferenceArray<Game.GearItem>(new[] { scrap });
        millable.m_RestoreRequiredGearUnits = new Il2CppStructArray<int>(new[] { crampons ? 4 : 3 });
        millable.m_Skill = Game.SkillType.None;
    }
}

[HarmonyPatch]
internal static class PrepareManufacturingMilling
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem), nameof(Game.GearItem.Awake));
        yield return AccessTools.Method(typeof(Game.GearItem), nameof(Game.GearItem.Deserialize));
        yield return AccessTools.Method(typeof(Game.GearItem), nameof(Game.GearItem.ManualStart));
    }
    private static void Prefix(Game.GearItem __instance) => ManufacturingMilling.Attach(__instance);
}

[HarmonyPatch]
internal static class ConfigureManufacturingMilling
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Panel_Milling), nameof(Game.Panel_Milling.DetermineRepairables));
    private static void Prefix()
    {
        if (!WorldObjects.IsInterloper) return;
        var inventory = Game.GameManager.GetInventoryComponent();
        if (inventory?.m_Items == null) return;
        foreach (var item in inventory.m_Items)
            if (item.m_GearItem != null) ManufacturingMilling.Configure(item.m_GearItem);
    }
}
