extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;

namespace InterloperOverhaul;
internal static class SurvivalBalance
{
    private static readonly Dictionary<int,(float Daily,float Blizzard)> shelters=new();
    private static readonly Dictionary<IntPtr,(Game.CraftingLocation Location,bool IgnoreLock)> locations=new();
    private static readonly Dictionary<int,float> temperatures=new();
    internal static void Manager(Game.SnowShelterManager manager)
    {
        int id=manager.GetInstanceID();
        if (!temperatures.ContainsKey(id)) temperatures[id]=manager.m_TemperatureIncreaseCelsius;
        manager.m_TemperatureIncreaseCelsius=ItemBalance.Active ? 30 : temperatures[id];
    }
    internal static void Shelter(Game.SnowShelter shelter)
    {
        if (!ItemBalance.Active || shelter==null || !shelter.gameObject.scene.IsValid()) return;
        int id=shelter.GetInstanceID();
        if (!shelters.TryGetValue(id,out var original))
            shelters[id]=original=(shelter.m_DailyDecayHP,shelter.m_BlizzardHourlyDecayHP);
        shelter.m_DailyDecayHP=original.Daily*0.5f;
        shelter.m_BlizzardHourlyDecayHP=original.Blizzard*0.5f;
        if (shelter.m_Bed!=null) shelter.m_Bed.m_WarmthBonusCelsius=5;
    }
    internal static void Forget(Game.SnowShelter shelter)=>shelters.Remove(shelter.GetInstanceID());
    internal static void Noise(Game.NoiseMakerItem item)
    {
        if (!ItemBalance.Active || item==null || !item.gameObject.scene.IsValid()) return;
        item.m_AIDamageInflictionInRadius=250;
        item.m_PlayerDamageInflictionInRadius=250;
        item.m_BurnAfflicationDamageAmount=250;
    }
    internal static void Recipes(Gear.BlueprintManager manager)
    {
        if (manager?.m_AllBlueprints==null) return;
        foreach (var bp in manager.m_AllBlueprints)
        {
            if (bp==null) continue;
            var result=bp.GetCraftedResult()?.TryCast<Game.GearItem>();
            if (LootBalancePolicy.ItemName(result?.name ?? bp.GetCraftedResult()?.TryCast<Gear.GearItemData>()?.name ?? "")=="GEAR_NoiseMaker")
            {
                if (!locations.ContainsKey(bp.Pointer)) locations[bp.Pointer]=(bp.m_RequiredCraftingLocation,bp.m_IgnoreLockInSurvival);
                bp.m_RequiredCraftingLocation=ItemBalance.Active ? Game.CraftingLocation.Workbench : locations[bp.Pointer].Location;
                bp.m_IgnoreLockInSurvival=ItemBalance.Active || locations[bp.Pointer].IgnoreLock;
            }
        }
    }
}
[HarmonyPatch]
internal static class BalanceSnowShelter
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.SnowShelter),"Awake");
        yield return AccessTools.Method(typeof(Game.SnowShelter),"Deserialize");
        yield return AccessTools.Method(typeof(Game.SnowShelter),"Update");
    }
    private static void Postfix(Game.SnowShelter __instance)=>SurvivalBalance.Shelter(__instance);
}
[HarmonyPatch]
internal static class BalanceShelterTemperature
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.SnowShelterManager),"Start");
        yield return AccessTools.Method(typeof(Game.SnowShelterManager),"EnterShelter");
    }
    private static void Prefix(Game.SnowShelterManager __instance)=>SurvivalBalance.Manager(__instance);
}
[HarmonyPatch]
internal static class BalanceNoisemaker
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.NoiseMakerItem),"Awake");
        yield return AccessTools.Method(typeof(Game.NoiseMakerItem),"Deserialize");
        yield return AccessTools.Method(typeof(Game.NoiseMakerItem),"PerformDamageToAllInRange");
    }
    private static void Prefix(Game.NoiseMakerItem __instance)=>SurvivalBalance.Noise(__instance);
    private static void Postfix(Game.NoiseMakerItem __instance)=>SurvivalBalance.Noise(__instance);
}
[HarmonyPatch]
internal static class OrdinaryNoisemakerWorkbench
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Gear.BlueprintManager),"GetFilteredBlueprints");
    private static void Prefix(Gear.BlueprintManager __instance)=>SurvivalBalance.Recipes(__instance);
}
[HarmonyPatch]
internal static class NoisemakerMoraleEvent
{
    [ThreadStatic] internal static bool Active;
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.PackManager),"ModifyGroupMoraleOnHitWithItem");
        yield return AccessTools.Method(typeof(Game.PackManager),"ModifyGroupMoraleOnThrownItem");
    }
    private static void Prefix(Game.GearItem item,out bool __state)
    {
        __state=Active;
        Active=ItemBalance.Active && item!=null && LootBalancePolicy.ItemName(item.name)=="GEAR_NoiseMaker";
    }
    private static void Finalizer(bool __state)=>Active=__state;
}
[HarmonyPatch]
internal static class NoisemakerBreaksMorale
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.PackManager),"ModifyGroupMorale");
    private static void Prefix(ref float percent)
    {
        if (NoisemakerMoraleEvent.Active) percent=Math.Min(-100,percent*5);
    }
}

[HarmonyPatch]
internal static class ForgetShelterBalance
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.SnowShelter),"OnDestroy");
    private static void Prefix(Game.SnowShelter __instance)=>SurvivalBalance.Forget(__instance);
}
[HarmonyPatch]
internal static class MoreAmmunitionIngredients
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.GearItem),"RollSpawnChance");
    [HarmonyPriority(Priority.First)]
    private static void Prefix(Game.GearItem __instance)
    {
        if (!ItemBalance.Active || __instance.m_RolledSpawnChance || __instance.m_HasBeenOwnedByPlayer || __instance.m_BeenInContainer) return;
        if (LootBalancePolicy.ItemName(__instance.name) is "GEAR_StumpRemover" or "GEAR_DustingSulfur")
            __instance.m_SpawnChance=Math.Max(75,__instance.m_SpawnChance);
    }
}
