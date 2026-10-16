extern alias GameAssembly;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Units = Il2CppTLD.IntBackedUnit;

namespace InterloperOverhaul;
internal static class PermanentAccessories
{
    internal const string SaveKey = "interloper-overhaul-permanent-accessories-v1";
    internal static PermanentAccessoryState State = new();
    internal static bool Failed;
    internal static bool Active => ItemBalance.Active && !Failed;
    internal static void Reset() { State = new(); Failed = false; }
    internal static void Read(Game.SlotData slot)
    {
        Reset();
        try
        {
            if (slot != null && slot.HasData(SaveKey))
                State = PermanentAccessoryState.Deserialize(Encoding.UTF8.GetString(slot.m_Dict[SaveKey].ToArray()));
        }
        catch (Exception ex) { Failed = true; MelonLogger.Error("[Permanent accessories] Saved unlocks could not be loaded; collectible conversion disabled. " + ex.Message); }
    }
    internal static void Save(Game.SlotData slot)
    {
        if (Failed || slot == null) return;
        slot.Lock();
        try { slot.m_Dict[SaveKey] = new Il2CppStructArray<byte>(Encoding.UTF8.GetBytes(State.Serialize())); }
        finally { slot.UnLock(); }
    }
    internal static bool Grant(string name)
    {
        name = LootBalancePolicy.ItemName(name);
        bool changed;
        if (name == "GEAR_ToolBelt") { changed = !State.ToolBelt; State.ToolBelt = true; }
        else if (name == "GEAR_RifleScabbardA") { changed = !State.RifleHolster; State.RifleHolster = true; }
        else return false;
        if (changed)
        {
            string title = name == "GEAR_ToolBelt" ? "Foreman's Tool Belt" : "Rifle Holster";
            string description = Description(name);
            var log = Game.GameManager.GetLogComponent();
            if (log != null) log.m_GeneralNotes = (log.m_GeneralNotes ?? "") + "\nPermanent upgrade: " + title + ". " + description;
            Game.HUDMessage.AddMessage("Permanent upgrade unlocked: " + title);
        }
        return changed;
    }
    internal static string Description(string name) => LootBalancePolicy.ItemName(name) == "GEAR_ToolBelt"
        ? "Collect to permanently reduce carried tool weight by 50% for the heaviest item of each tool type, without a type limit. Weapons are excluded. No equipment slot or carried item is required."
        : "Collect to permanently reduce the weight of your heaviest carried rifle by 50%, including rifle variants. No equipment slot, carried item, or clothing penalty is required.";
    internal static void Collect(Game.Inventory inventory, Game.GearItem item)
    {
        if (!Active || item == null || !PermanentAccessoryPolicy.IsCollectible(item.name)) return;
        if (item.m_ClothingItem != null && item.m_ClothingItem.IsWearing()) item.m_ClothingItem.TakeOff();
        Grant(item.name);
        inventory.DestroyGear(item);
    }
    internal static void Migrate(Game.Inventory inventory)
    {
        if (!Active || inventory?.m_Items == null || Game.SaveGameSystem.IsRestoreInProgress() || Game.SaveGameSystem.IsSceneRestoreInProgress()) return;
        var pending = new List<Game.GearItem>();
        foreach (var entry in inventory.m_Items)
            if (entry.m_GearItem != null && entry.m_GearItem.m_InPlayerInventory && PermanentAccessoryPolicy.IsCollectible(entry.m_GearItem.name)) pending.Add(entry.m_GearItem);
        foreach (var item in pending) Collect(inventory,item);
    }
    internal static System.Collections.IEnumerator AfterRestore()
    {
        for (int frame=0;frame<600;frame++)
        {
            yield return null;
            if (frame%30 == 0 && Active && !Game.GameManager.s_ActiveIsMainMenu)
                Migrate(Game.GameManager.GetInventoryComponent());
        }
    }
    internal static Units.ItemWeight Reduction(Game.Inventory inventory)
    {
        if (!Active || inventory?.m_Items == null || (!State.ToolBelt && !State.RifleHolster)) return Units.ItemWeight.Zero;
        var tools = new Dictionary<string,Units.ItemWeight>();
        var rifle = Units.ItemWeight.Zero;
        foreach (var entry in inventory.m_Items)
        {
            var item = entry.m_GearItem;
            if (item == null || !item.m_InPlayerInventory || item.m_InProgressCraftItem != null) continue;
            string name = LootBalancePolicy.ItemName(item.name);
            if (PermanentAccessoryPolicy.IsRifle(name))
            {
                if (State.RifleHolster) rifle = Units.ItemWeight.Max(rifle,item.GetSingleItemWeightKG());
                continue;
            }
            if (!State.ToolBelt || item.m_GearItemData == null || (item.m_GearItemData.m_Type & Gear.GearType.Tool) == 0 ||
                PermanentAccessoryPolicy.IsWeapon(name) || item.m_AmmoItem != null || item.m_AmmoCasingItem != null) continue;
            string family = PermanentAccessoryPolicy.ToolFamily(name);
            if (family == "") continue;
            var weight = item.GetSingleItemWeightKG();
            if (!tools.TryGetValue(family,out var previous) || weight > previous) tools[family] = weight;
        }
        var reduction = rifle * 0.5f;
        foreach (var weight in tools.Values) reduction += weight * 0.5f;
        return reduction;
    }
}
[HarmonyPatch]
internal static class NewPermanentAccessories
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GameManager),nameof(Game.GameManager.LaunchSandbox));
    private static void Prefix() => PermanentAccessories.Reset();
}
[HarmonyPatch]
internal static class RestorePermanentAccessories
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem),nameof(Game.SaveGameSystem.RestoreGlobalData));
    private static void Prefix(string name) => PermanentAccessories.Read(Game.SaveGameSlots.GetSaveSlotFromName(name));
    private static void Postfix(bool __result)
    {
        if (__result && WorldObjects.IsInterloper) MelonCoroutines.Start(PermanentAccessories.AfterRestore());
    }
}
[HarmonyPatch]
internal static class SavePermanentAccessories
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem),nameof(Game.SaveGameSystem.SaveGlobalData));
    private static void Postfix(Game.SlotData slot) { if (WorldObjects.IsInterloper) PermanentAccessories.Save(slot); }
}
[HarmonyPatch]
internal static class CollectPermanentAccessories
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Inventory),nameof(Game.Inventory.AddGear));
    private static void Postfix(Game.Inventory __instance, Game.GearItem gi)
    {
        if (!Game.SaveGameSystem.IsRestoreInProgress() && !Game.SaveGameSystem.IsSceneRestoreInProgress()) PermanentAccessories.Collect(__instance,gi);
    }
}
[HarmonyPatch]
internal static class PreventPermanentAccessoryEquipment
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.ClothingItem),nameof(Game.ClothingItem.PutOn));
    private static bool Prefix(Game.ClothingItem __instance) => !PermanentAccessories.Active || __instance.m_GearItem == null ||
        !PermanentAccessoryPolicy.IsCollectible(__instance.m_GearItem.name);
}
[HarmonyPatch]
internal static class RemoveConsumedAccessoryBuff
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.WeightReductionBuff),nameof(Game.WeightReductionBuff.GetWeightReduction));
    private static bool Prefix(Game.WeightReductionBuff __instance, ref Units.ItemWeight __result)
    {
        var item = __instance.m_GearItem;
        if (!PermanentAccessories.Active || item == null || !PermanentAccessoryPolicy.IsCollectible(item.name)) return true;
        __result = Units.ItemWeight.Zero;
        return false;
    }
}
[HarmonyPatch]
internal static class PermanentAccessoryWeight
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Inventory),nameof(Game.Inventory.GetTotalWeightKG));
    private static void Postfix(Game.Inventory __instance, ref Units.ItemWeight __result)
    {
        if (__instance.m_ForceOverrideWeight || !PermanentAccessories.Active) return;
        __result = Units.ItemWeight.Max(Units.ItemWeight.Zero,__result - PermanentAccessories.Reduction(__instance));
    }
}
[HarmonyPatch]
internal static class PermanentAccessoryDescription
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetDescription));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetDescriptionToDisplayOnInventoryScreen));
        yield return AccessTools.PropertyGetter(typeof(Game.GearItem),nameof(Game.GearItem.Description));
    }
    private static void Postfix(Game.GearItem __instance, ref string __result)
    {
        if (PermanentAccessories.Active && PermanentAccessoryPolicy.IsCollectible(__instance.name)) __result = PermanentAccessories.Description(__instance.name);
    }
}
