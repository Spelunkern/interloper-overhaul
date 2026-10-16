extern alias GameAssembly;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
using Gear=GameAssembly::Il2CppTLD.Gear;
using Units=Il2CppTLD.IntBackedUnit;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
namespace InterloperOverhaul;
internal static class VehicleBalance
{
    internal const string TankName="IO_VehicleFuelTank";
    private static readonly Dictionary<int,float> temperatures=new();
    internal static void Temperature(Game.PlayerInVehicle vehicle)
    {
        int id=vehicle.GetInstanceID();
        if (!temperatures.ContainsKey(id)) temperatures[id]=vehicle.m_TempIncrease;
        vehicle.m_TempIncrease=ItemBalance.Active ? 15 : temperatures[id];
    }
    internal static void Bed(Game.Bed bed)
    {
        if (ItemBalance.Active && bed!=null && LootBalancePolicy.ItemName(bed.name)=="VehicleBed") bed.m_WarmthBonusCelsius=5;
    }
    internal static string GuidFor(string key)=>new Guid(MD5.HashData(Encoding.UTF8.GetBytes("InterloperOverhaul/VehicleFuelTank/"+key))).ToString();
    internal static Game.Container Tank(Game.OpenClose cover)
    {
        if (!ItemBalance.Active || cover==null || !StreetLootPolicy.IsFuelCover(cover.name) || !cover.gameObject.scene.IsValid()) return null;
        var existing=cover.transform.Find(TankName);
        if (existing!=null) return existing.GetComponent<Game.Container>();
        Game.Container template=null;
        for (var parent=cover.transform.parent;parent!=null && template==null;parent=parent.parent)
            foreach (var candidate in parent.GetComponentsInChildren<Game.Container>(true))
                if (candidate.name.Contains("GloveBox",StringComparison.Ordinal)) { template=candidate;break; }
        if (template==null) return null;
        string key=Game.ObjectGuid.MaybeGetGuidFromGameObject(cover.gameObject);
        if (string.IsNullOrEmpty(key)) key=LootBalance.Key(cover.gameObject);
        // Clone a native glove box under an inactive parent, retaining current
        // container UI, search interaction, audio, and animation references.
        var staging=new GameObject("IO_FuelTankAssembly");staging.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(staging,cover.gameObject.scene);
        var clone=UnityEngine.Object.Instantiate(template.gameObject,staging.transform);
        try
        {
            clone.name=TankName;
            int index=0;
            foreach (var guid in clone.GetComponentsInChildren<Game.ObjectGuid>(true))
            {
                guid.m_Guid=GuidFor(key+"/"+index++);
                // Runtime PDIDs are separate from serialized legacy GUIDs. Clear
                // the clone cache before registration to preserve the glove box.
                guid.SetRuntimeCachedPdid(null);guid.RuntimeRegistered=false;
                Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(guid.gameObject,guid.m_Guid);
                guid.m_HistoricalGuids=new Il2CppSystem.Collections.Generic.List<string>();
            }
            foreach (var item in clone.GetComponentsInChildren<Game.GearItem>(true)) UnityEngine.Object.DestroyImmediate(item.gameObject);
            foreach (var renderer in clone.GetComponentsInChildren<Renderer>(true)) renderer.enabled=false;
            foreach (var collider in clone.GetComponentsInChildren<Collider>(true)) collider.enabled=false;
            var box=clone.GetComponent<Game.Container>();
            box.m_Items=new Il2CppSystem.Collections.Generic.List<Gear.GearItemObject>();
            box.m_DecorationItems=new Il2CppSystem.Collections.Generic.List<Game.DecorationItem>();
            box.m_GearToInstantiate.Clear();
            box.m_ScriptedItemPrefabs=new Il2CppReferenceArray<GameObject>(0);
            box.m_ScriptedItemFilter=new Il2CppStructArray<Game.GameModeFilter>(0);
            box.m_LootTableData=null;box.m_LockedLootTableData=null;box.m_LootTablePrefab=null;box.m_LockedLootTablePrefab=null;box.m_LootTable=null;
            box.m_LocalizedDisplayName=new Game.LocalizedString { m_LocalizationID="IO_VehicleFuelTankName" };
            box.m_Capacity=Units.ItemWeight.FromKilograms(1);
            box.m_SpawnChance=100;box.m_ChanceEmpty=100;box.m_MinRandomItems=box.m_MaxRandomItems=0;
            box.m_Inspected=false;box.m_Restored=false;box.m_NotPopulated=true;
            box.m_DisableSerialization=false;box.m_MarkedInactiveInSaveData=false;box.m_StartInspected=false;
            clone.transform.SetParent(cover.transform,false);
            clone.transform.localPosition=Vector3.zero;clone.transform.localRotation=Quaternion.identity;clone.transform.localScale=Vector3.one;
            clone.SetActive(true);
            return box;
        }
        catch { UnityEngine.Object.DestroyImmediate(clone);throw; }
        finally { UnityEngine.Object.DestroyImmediate(staging); }
    }
    internal static void Populate(Game.Container tank)
    {
        if (!ItemBalance.Active || tank==null || tank.name!=TankName) return;
        double litres=StreetLootPolicy.FuelLitres(Game.GameManager.GetRandomSeed(0),LootBalance.Key(tank.gameObject));
        if (litres<=0) return;
        var item=Game.GearItem.InstantiateGearItem("GEAR_LampFuel");
        item.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
        var liquid=item.m_LiquidItem ?? item.GetComponent<Game.LiquidItem>();
        liquid.m_Liquid=liquid.m_Minimum=liquid.m_Maximum=liquid.m_LiquidCapacity*(float)(litres/0.5);
        item.ManualStart();tank.AddGear(item);
    }
}
[HarmonyPatch]
internal static class WarmerVehicleInterior
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.PlayerInVehicle),"EnterVehicle");
        yield return AccessTools.Method(typeof(Game.PlayerInVehicle),"ForceIntoVehicleSilent");
        yield return AccessTools.Method(typeof(Game.PlayerInVehicle),"OnBedroll");
    }
    private static void Prefix(Game.PlayerInVehicle __instance)=>VehicleBalance.Temperature(__instance);
    private static void Postfix(Game.PlayerInVehicle __instance)=>VehicleBalance.Bed(__instance.m_VehicleDoorUsed?.m_Bed);
}
[HarmonyPatch]
internal static class WarmerVehicleBed
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Bed),"Awake");
    private static void Postfix(Game.Bed __instance)=>VehicleBalance.Bed(__instance);
}
[HarmonyPatch]
internal static class PrepareVehicleFuelTank
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.OpenClose),"Awake");
    private static void Postfix(Game.OpenClose __instance)=>VehicleBalance.Tank(__instance);
}
[HarmonyPatch]
internal static class SearchVehicleFuelTank
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.OpenClose),"PerformInteraction");
    private static bool Prefix(Game.OpenClose __instance,ref bool __result)
    {
        var tank=VehicleBalance.Tank(__instance);
        if (tank==null) return true;
        if (!__instance.IsOpen()) __instance.Open(false);
        __result=tank.GetComponent<Game.ContainerInteraction>().PerformInteraction();
        return false;
    }
}
[HarmonyPatch]
internal static class VehicleFuelLoot
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Container),"PopulateWithRandomGear");
    private static void Prefix(Game.Container __instance,out bool __state)=>__state=ItemBalance.Active && __instance.name==VehicleBalance.TankName && __instance.m_NotPopulated && !__instance.m_Restored && !__instance.m_Inspected;
    private static void Postfix(Game.Container __instance,bool __state) { if (__state) { __instance.m_NotPopulated=false;VehicleBalance.Populate(__instance); } }
}
[HarmonyPatch]
internal static class VehicleFuelTankName
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Localization),"Get",new[] { typeof(string) });
    private static bool Prefix(string key,ref string __result)
    {
        if(key!="IO_VehicleFuelTankName") return true;
        __result="Fuel Tank";return false;
    }
}
