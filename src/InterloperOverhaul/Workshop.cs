extern alias GameAssembly;

using System.Reflection;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Trader = GameAssembly::Il2CppTLD.Trader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterloperOverhaul;

internal static class Workshop
{
    internal const string SaveKey = "interloper-overhaul-workshop-v1";
    internal const string CardPrefab = "GEAR_BlueprintCard_BushcraftBow";
    internal static WorkshopRunState State = new();
    internal static bool Failed;
    [ThreadStatic] internal static bool TraderRewards;
    [ThreadStatic] internal static bool CreatingCard;

    internal static void Reset() { State = new(); Failed = false; }
    internal static void Read(Game.SlotData slot)
    {
        Reset();
        try
        {
            if (slot != null && slot.HasData(SaveKey))
                State = WorkshopRunState.Deserialize(Encoding.UTF8.GetString(slot.m_Dict[SaveKey].ToArray()));
        }
        catch (Exception ex) { Failed = true; MelonLogger.Error("[Workshop] Invalid saved progress; workshop disabled: " + ex.Message); }
    }
    internal static void Save(Game.SlotData slot)
    {
        if (Failed || slot == null) return;
        slot.Lock();
        try { slot.m_Dict[SaveKey] = new Il2CppStructArray<byte>(Encoding.UTF8.GetBytes(State.Serialize())); }
        finally { slot.UnLock(); }
    }
    internal static string GuidOf(Game.GearItem gear) => Game.ObjectGuid.GetGuidFromGameObject(gear.gameObject);
    internal static VariantSpec Card(Game.GearItem gear)
    {
        if (Failed || gear == null || LootBalancePolicy.ItemName(gear.name) != CardPrefab) return null;
        string guid = GuidOf(gear);
        return guid != null && State.Cards.TryGetValue(guid, out string item) ? VariantPolicy.Find(item) : null;
    }
    internal static bool Authored(Game.GearItem gear) => gear != null && !gear.name.Contains("(Clone)",StringComparison.Ordinal) &&
        gear.gameObject.scene.IsValid() && !gear.gameObject.scene.name.StartsWith("MainMenu",StringComparison.Ordinal) &&
        gear.transform.parent != null && gear.m_RadialObjectSpawnerParent == null && !WorldPolicy.IsBeachcombingObject(WorldObjects.PathOf(gear.gameObject));

    internal static Game.GearItem MakeCard(VariantSpec variant, string guid)
    {
        CreatingCard = true;
        try
        {
            var card = Game.GearItem.InstantiateGearItem(CardPrefab);
            Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(card.gameObject,guid);
            State.Cards[guid] = variant.Item;
            card.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            Configure(card);
            return card;
        }
        finally { CreatingCard = false; }
    }
    internal static void Configure(Game.GearItem card)
    {
        if (Card(card) == null) return;
        var unlock = card.m_BlueprintItemUnlock ?? card.GetComponent<Game.BlueprintItemUnlock>();
        if (unlock != null) { unlock.m_UnlockInStoryMode = false; unlock.m_UnlockInSurvival = false; }
        var note = card.m_NarrativeCollectibleItem ?? card.GetComponent<Game.NarrativeCollectibleItem>();
        if (note != null) note.enabled = false;
        // Use ordinary pickup/inspection; the native note would display the
        // Bushcraft card text and bypass inventory pickup callbacks.
        card.m_NarrativeCollectibleItem = null;
    }
    internal static bool ReplaceWorldVariant(Game.GearItem source)
    {
        var variant = VariantPolicy.Find(source.name);
        if (variant == null || !Authored(source)) return false;
        if (Failed) { source.gameObject.SetActive(false); return true; }
        string key = LootBalance.Key(source.gameObject);
        if (State.Issued.Contains(key)) { source.gameObject.SetActive(false); return true; }
        if (!source.gameObject.activeInHierarchy) return true;
        var card = MakeCard(variant, System.Guid.NewGuid().ToString());
        SceneManager.MoveGameObjectToScene(card.gameObject,source.gameObject.scene);
        card.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
        State.Issued.Add(key);
        source.m_DisableSerialization = true;
        source.gameObject.SetActive(false);
        return true;
    }
    internal static void InitializeCard(Game.GearItem gear)
    {
        if (Failed || !gear.gameObject.scene.IsValid() || !WorldObjects.IsInterloper || CreatingCard || LootBalancePolicy.ItemName(gear.name) != CardPrefab) return;
        if (Card(gear) == null && !Game.SaveGameSystem.IsRestoreInProgress())
        {
            string guid = GuidOf(gear);
            if (string.IsNullOrEmpty(guid)) { guid = System.Guid.NewGuid().ToString(); Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(gear.gameObject,guid); }
            State.Cards[guid] = "GEAR_Bow_Bushcraft";
        }
        Configure(gear);
    }
    internal static void Learn(Game.GearItem card, Game.Inventory inventory)
    {
        var variant = Card(card);
        if (variant == null) return;
        LearnVariant(variant);
        inventory.DestroyGear(card);
    }
    internal static void LearnVariant(VariantSpec variant)
    {
        bool learned = State.Learned.Add(variant.Item);
        if (learned)
        {
            var log = Game.GameManager.GetLogComponent();
            if (log != null) log.m_GeneralNotes = (log.m_GeneralNotes ?? "") + "\n" + variant.Title + ": " + variant.Description;
            Game.HUDMessage.AddMessage("Learned: " + variant.Title);
        }
    }
    internal static bool Unlocked(ManufacturingRecipe recipe) => !Failed && VariantPolicy.Unlocked(recipe,State.Learned);
    internal static bool AtCannery(Game.CraftingLocationInterface location)
    {
        var component = location?.TryCast<Component>();
        return component != null && component.gameObject.scene.name.StartsWith("CanneryRegion",StringComparison.Ordinal);
    }
}

[HarmonyPatch]
internal static class WorkshopSave
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem),nameof(Game.SaveGameSystem.SaveGlobalData));
    private static void Postfix(Game.SlotData slot) { if (WorldObjects.IsInterloper) Workshop.Save(slot); }
}
[HarmonyPatch]
internal static class WorkshopRestore
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem),nameof(Game.SaveGameSystem.RestoreGlobalData));
    private static void Prefix(string name) => Workshop.Read(Game.SaveGameSlots.GetSaveSlotFromName(name));
}
[HarmonyPatch]
internal static class LearnWorkshopCard
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Inventory),nameof(Game.Inventory.AddGear));
    private static void Prefix(Game.GearItem gi, out VariantSpec __state)
    {
        if (WorldObjects.IsInterloper && gi != null) Workshop.InitializeCard(gi);
        __state = LootBalance.Active && Manufacturing.Current == null && !Workshop.Failed && gi != null &&
            !Game.SaveGameSystem.IsRestoreInProgress() && !gi.m_BeenInPlayerInventory && gi.m_InProgressCraftItem == null
            ? VariantPolicy.Find(gi.name) : null;
    }
    private static void Postfix(Game.Inventory __instance, Game.GearItem gi, VariantSpec __state)
    {
        if (__state != null) { Workshop.LearnVariant(__state); __instance.DestroyGear(gi); }
        else if (WorldObjects.IsInterloper) Workshop.Learn(gi,__instance);
    }
}
[HarmonyPatch]
internal static class WorkshopCardName
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(Game.GearItem),nameof(Game.GearItem.DisplayName));
        yield return AccessTools.PropertyGetter(typeof(Game.GearItem),nameof(Game.GearItem.DisplayNameWithCondition));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetDisplayNameWithoutConditionForInventoryInterfaces));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetDisplayNameForInventoryInterfaces));
    }
    private static void Postfix(Game.GearItem __instance, ref string __result)
    {
        if (WorldObjects.IsInterloper && Workshop.Card(__instance) is VariantSpec variant) __result = variant.Title;
    }
}
[HarmonyPatch]
internal static class WorkshopCardDescription
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(Game.GearItem),nameof(Game.GearItem.Description));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetDescription));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetDescriptionToDisplayOnInventoryScreen));
    }
    private static void Postfix(Game.GearItem __instance, ref string __result)
    {
        if (WorldObjects.IsInterloper && Workshop.Card(__instance) is VariantSpec variant) __result = variant.Description;
    }
}
[HarmonyPatch]
internal static class RestoreWorkshopCard
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Deserialize));
    private static void Postfix(Game.GearItem __instance) { if (WorldObjects.IsInterloper) Workshop.Configure(__instance); }
}
[HarmonyPatch]
internal static class LockWorkshopRecipes
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.IsLocked));
    private static void Postfix(Gear.BlueprintData __instance, ref bool __result)
    {
        var recipe = VariantPolicy.Recipe(__instance.name);
        if (recipe != null && WorldObjects.IsInterloper) __result = !Workshop.Unlocked(recipe);
    }
}
[HarmonyPatch]
internal static class CanneryWorkshopLocation
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.CanCraftBlueprint));
    private static void Postfix(Gear.BlueprintData __instance, Game.CraftingLocationInterface cli, ref bool __result)
    {
        var recipe = VariantPolicy.Recipe(__instance.name);
        if (recipe != null) __result &= WorldObjects.IsInterloper && Workshop.Unlocked(recipe) && Workshop.AtCannery(cli);
    }
}
[HarmonyPatch]
internal static class TraderSchematicHeader
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Trader.TradeExchangeData),nameof(Trader.TradeExchangeData.GetTradeExchangeHeader));
    private static void Postfix(Trader.TradeExchangeData __instance, ref string __result)
    {
        if (WorldObjects.IsInterloper && __instance.m_GainedItem != null && VariantPolicy.Find(LootBalancePolicy.TraderItem(__instance.m_GainedItem.AssetGUID)) is VariantSpec variant)
            __result = variant.Title;
    }
}
[HarmonyPatch]
internal static class TraderSchematicDelivery
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Trader.ExchangeBox),"PrepareAndShowRewards");
    private static void Prefix(out bool __state) { __state = Workshop.TraderRewards; Workshop.TraderRewards = LootBalance.Active; }
    private static void Finalizer(bool __state) => Workshop.TraderRewards = __state;
}
[HarmonyPatch]
internal static class RewardSchematics
{
    private static IEnumerable<MethodBase> TargetMethods() => AccessTools.GetDeclaredMethods(typeof(Game.GearItem)).Where(m => m.Name == nameof(Game.GearItem.InstantiateGearItem));
    private static void Postfix(ref Game.GearItem __result)
    {
        if (!LootBalance.Active || Workshop.CreatingCard || Workshop.Failed || !(RewardScope.Active || Workshop.TraderRewards) || __result == null) return;
        var variant = VariantPolicy.Find(__result.name);
        if (variant == null) return;
        var original = __result;
        __result = Workshop.MakeCard(variant,System.Guid.NewGuid().ToString());
        original.m_DisableSerialization = true;
        original.gameObject.SetActive(false);
        UnityEngine.Object.Destroy(original.gameObject);
    }
}

[HarmonyPatch]
internal static class RestoreSchematicTradeEligibility
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Trader.TradeExchangeData),nameof(Trader.TradeExchangeData.IsValidInCurrentGamemode));
    private static void Prefix(Trader.TradeExchangeData __instance, out bool __state)
    {
        __state = WorldObjects.IsInterloper && __instance.m_GainedItem != null && VariantPolicy.Find(LootBalancePolicy.TraderItem(__instance.m_GainedItem.AssetGUID)) != null;
        if (__state) RewardScope.Enter();
    }
    private static void Finalizer(bool __state) { if (__state) RewardScope.Leave(); }
}

[HarmonyPatch]
internal static class PreventVanillaSchematicUnlock
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.BlueprintItemUnlock),nameof(Game.BlueprintItemUnlock.ShouldUnlock));
    private static bool Prefix(Game.BlueprintItemUnlock __instance, ref bool __result)
    {
        var gear = __instance.GetComponent<Game.GearItem>();
        if (!WorldObjects.IsInterloper || gear == null || LootBalancePolicy.ItemName(gear.name) != Workshop.CardPrefab) return true;
        Workshop.InitializeCard(gear);
        __result = false;
        return false;
    }
}

[HarmonyPatch]
internal static class WorkshopRecipeName
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Gear.BlueprintData),nameof(Gear.BlueprintData.GetDisplayedNameWithCount));
    private static void Postfix(Gear.BlueprintData __instance, ref string __result)
    {
        if (!WorldObjects.IsInterloper) return;
        var recipe = VariantPolicy.Recipe(__instance.name);
        if (recipe == null) return;
        var variant = VariantPolicy.Variants.First(v => v.Upgrade == recipe || v.Reverse == recipe);
        __result = variant.Upgrade == recipe ? "Modify: " + variant.Name : "Revert: " + variant.Name;
    }
}
