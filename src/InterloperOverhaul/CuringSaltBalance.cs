extern alias GameAssembly;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Game = GameAssembly::Il2Cpp;
using Units = Il2CppTLD.IntBackedUnit;

namespace InterloperOverhaul;
internal static class CuringSaltBalance
{
    internal const string SaveKey="interloper-overhaul-curing-salt-mass-v1";
    internal static Dictionary<string,SaltMassRecord> Records=new();
    private static bool failed;
    internal static double Kg(Units.ItemWeight weight) => weight/Units.ItemWeight.FromKilograms(1);
    internal static bool Active(Game.CuringStorage box) => ItemBalance.Active && !failed && box?.m_State!=null;
    internal static SaltMassRecord Record(Game.CuringStorage box)
    {
        if (!Active(box)) return null;
        string guid=box.m_State.m_GUID;
        if (string.IsNullOrEmpty(guid)) return new SaltMassRecord { QuantityKg=Kg(box.m_State.m_SaltStorage),MassKg=Kg(box.m_State.m_SaltStorage)*SaltMassRecord.LegacyScale };
        if (!Records.TryGetValue(guid,out var record)) Records[guid]=record=new SaltMassRecord();
        record.Reconcile(Kg(box.m_State.m_SaltStorage));
        return record;
    }
    internal static (double Quantity,double Mass) Pending(Game.CuringStorage box)
    {
        double quantity=0,mass=0;
        foreach (var salt in box.m_AddedSalt)
        {
            if (salt?.m_PowderItem==null) continue;
            double amount=Kg(salt.m_PowderItem.m_Weight);
            quantity+=amount;
            mass+=amount*ClothingNutritionPolicy.ContentWeightScale(salt.name);
        }
        return (quantity,mass);
    }
    internal sealed class Deposit
    {
        internal SaltMassRecord Record;
        internal double Quantity,Mass,AddedQuantity,AddedMass;
    }
    internal static Deposit BeforeClose(Game.CuringStorage box)
    {
        var record=Record(box);
        if (record==null) return null;
        var snapshot=new Deposit { Record=record,Quantity=record.QuantityKg,Mass=record.MassKg };
        double available=Math.Max(0,Kg(box.m_MaxSalt)-snapshot.Quantity);
        // Native close transfers pending salt in reverse order, possibly
        // returning a partially used jar or bag when the box fills up.
        for (int i=box.m_AddedSalt.Count-1;i>=0 && available>0;i--)
        {
            var salt=box.m_AddedSalt[i];
            if (salt?.m_PowderItem==null) continue;
            double quantity=Math.Min(available,Kg(salt.m_PowderItem.m_Weight));
            snapshot.AddedQuantity+=quantity;
            snapshot.AddedMass+=quantity*ClothingNutritionPolicy.ContentWeightScale(salt.name);
            available-=quantity;
        }
        return snapshot;
    }
    internal static void AfterClose(Game.CuringStorage box,Deposit state)
    {
        if (state==null || box?.m_State==null) return;
        double after=Kg(box.m_State.m_SaltStorage);
        double accepted=Math.Max(0,after-state.Quantity);
        state.Record.QuantityKg=state.Quantity;
        state.Record.MassKg=state.Mass;
        if (accepted>0)
        {
            double mass=state.AddedQuantity>0 ? state.AddedMass*Math.Min(1,accepted/state.AddedQuantity) : accepted*SaltMassRecord.LegacyScale;
            state.Record.Deposit(accepted,mass);
        }
        state.Record.Reconcile(after);
    }
    internal static string Label(Game.CuringStorage box)
    {
        var record=Record(box);
        if (record==null) return null;
        var pending=Pending(box);
        double capacity=Kg(box.m_MaxSalt);
        double fill=capacity>0 ? Math.Clamp((record.QuantityKg+pending.Quantity)/capacity,0,1) : 0;
        return $"Salt: {record.MassKg+pending.Mass:0.00} kg ({fill:0%})";
    }
    internal static void Reset() { Records=new();failed=false; }
    internal static void Read(Game.SlotData slot)
    {
        Reset();
        try
        {
            if (slot!=null && slot.HasData(SaveKey))
                Records=JsonSerializer.Deserialize<Dictionary<string,SaltMassRecord>>(Encoding.UTF8.GetString(slot.m_Dict[SaveKey].ToArray())) ?? new();
        }
        catch (Exception ex) { failed=true;MelonLogger.Error("[Curing salt] Stored mass could not be loaded; native salt display retained. "+ex.Message); }
    }
    internal static void Save(Game.SlotData slot)
    {
        if (failed || slot==null) return;
        foreach (var box in UnityEngine.Resources.FindObjectsOfTypeAll<Game.CuringStorage>())
            if (box.gameObject.scene.IsValid()) Record(box);
        slot.Lock();
        try { slot.m_Dict[SaveKey]=new Il2CppStructArray<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Records))); }
        finally { slot.UnLock(); }
    }
}
[HarmonyPatch]
internal static class StoreCuringSaltMass
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.CuringStorage),nameof(Game.CuringStorage.CloseCuringStorage));
    private static void Prefix(Game.CuringStorage __instance,out CuringSaltBalance.Deposit __state) => __state=CuringSaltBalance.BeforeClose(__instance);
    private static void Finalizer(Game.CuringStorage __instance,CuringSaltBalance.Deposit __state) => CuringSaltBalance.AfterClose(__instance,__state);
}
[HarmonyPatch]
internal static class DisplayCuringSaltMass
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Panel_CuringStorage),nameof(Game.Panel_CuringStorage.RefreshCuringStorage));
    private static void Postfix(Game.Panel_CuringStorage __instance)
    {
        if (__instance.m_SaltCapacityLabel==null) return;
        string label=CuringSaltBalance.Label(__instance.m_CuringStorage);
        if (label!=null) __instance.m_SaltCapacityLabel.text=label;
    }
}
[HarmonyPatch]
internal static class HoverCuringSaltMass
{
    internal sealed class Snapshot
    {
        internal Units.ItemWeight Salt,Pending,Capacity,Minimum;
        internal Game.CuringStorageState State;
    }
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.CuringStorage),nameof(Game.CuringStorage.UpdateHoverText));
    private static void Prefix(Game.CuringStorage __instance,out Snapshot __state)
    {
        __state=null;
        var record=CuringSaltBalance.Record(__instance);
        if (record==null) return;
        var pending=CuringSaltBalance.Pending(__instance);
        double quantity=record.QuantityKg+pending.Quantity;
        double ratio=quantity>0 ? (record.MassKg+pending.Mass)/quantity : SaltMassRecord.LegacyScale;
        __state=new Snapshot { State=__instance.m_State,Salt=__instance.m_State.m_SaltStorage,Pending=__instance.AddedSaltWeight,
            Capacity=__instance.m_MaxSalt,Minimum=__instance.m_MinSaltForCuring };
        // Only the hover formatter sees physical mass. Native transfers and
        // curing keep their original quantities, thresholds, rates, and yields.
        __instance.m_State.m_SaltStorage=Units.ItemWeight.FromKilograms((float)record.MassKg);
        __instance.AddedSaltWeight=Units.ItemWeight.FromKilograms((float)pending.Mass);
        __instance.m_MaxSalt*= (float)ratio;
        __instance.m_MinSaltForCuring*= (float)ratio;
    }
    private static void Finalizer(Game.CuringStorage __instance,Snapshot __state)
    {
        if (__state==null || __instance==null) return;
        __state.State.m_SaltStorage=__state.Salt;
        __instance.AddedSaltWeight=__state.Pending;
        __instance.m_MaxSalt=__state.Capacity;
        __instance.m_MinSaltForCuring=__state.Minimum;
    }
}
[HarmonyPatch]
internal static class ResetCuringSaltMass
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GameManager),nameof(Game.GameManager.LaunchSandbox));
    private static void Prefix() => CuringSaltBalance.Reset();
}
[HarmonyPatch]
internal static class LoadCuringSaltMass
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem),nameof(Game.SaveGameSystem.RestoreGlobalData));
    private static void Prefix(string name) => CuringSaltBalance.Read(Game.SaveGameSlots.GetSaveSlotFromName(name));
}
[HarmonyPatch]
internal static class SaveCuringSaltMass
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.SaveGameSystem),nameof(Game.SaveGameSystem.SaveGlobalData));
    private static void Postfix(Game.SlotData slot) { if (WorldObjects.IsInterloper) CuringSaltBalance.Save(slot); }
}
