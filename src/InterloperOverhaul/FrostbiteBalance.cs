extern alias GameAssembly;
using System.Reflection;
using System.Text.Json.Nodes;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
namespace InterloperOverhaul;
internal static class FrostbiteBalance
{
    internal const string Marker="IO_TemporaryFrostbite";
    internal static void Configure(Game.Frostbite frost)
    {
        if (!ItemBalance.Active || frost==null) return;
        frost.m_MaxPlayerHPReductionPerInstance=0;
        float maximum=1;
        if (frost.m_MaxHPList!=null) foreach(float value in frost.m_MaxHPList) maximum=Math.Max(maximum,value);
        // At least four full gauges per hour: no more than fifteen warm minutes.
        frost.m_HealPerHourBase=maximum*4;
        frost.m_MaxHealPerHour=Math.Max(frost.m_MaxHealPerHour,maximum*4);
        frost.m_LocalizedDescription=new Game.LocalizedString { m_LocalizationID="IO_FrostbiteDescription" };
    }
    internal static bool Valid(Game.Frostbite frost,int location)=>location>=0 && frost.m_MaxHPList!=null &&
        location<frost.m_MaxHPList.Length && frost.m_LocationsCurrentFrostbiteDamage!=null && location<frost.m_LocationsCurrentFrostbiteDamage.Count;
    internal static void FullGauge(Game.Frostbite frost,int location)
    {
        if (!Valid(frost,location)) return;
        frost.m_LocationsCurrentFrostbiteDamage[location]=frost.m_MaxHPList[location];
        if (!frost.m_LocationsWithFrostbiteRisk.Contains(location)) frost.m_LocationsWithFrostbiteRisk.Add(location);
    }
    internal sealed class Tick
    {
        internal Game.Frostbite Frost;
        internal float Hours;
        internal Dictionary<int,float> Fractions=new();
    }
    [ThreadStatic] internal static Tick Current;
    internal static Tick Begin(Game.Frostbite frost,float hours)
    {
        Configure(frost);
        var tick=new Tick { Frost=frost,Hours=Math.Max(0,hours) };
        foreach (int location in frost.m_LocationsWithActiveFrostbite) tick.Fractions[location]=1;
        return tick;
    }
    internal static void End(Tick tick)
    {
        if (tick==null) return;
        float damage=tick.Hours*10*tick.Fractions.Values.Sum();
        if (damage>0) Game.GameManager.GetConditionComponent().AddHealth(-damage,Game.DamageSource.FrostBite);
    }
    internal static void ActiveGauge(Game.Frostbite frost,int location,float delta)
    {
        float previous=frost.m_LocationsCurrentFrostbiteDamage[location];
        float next=Math.Clamp(previous+delta,0,frost.m_MaxHPList[location]);
        frost.m_LocationsCurrentFrostbiteDamage[location]=next;
        if (next<=0)
        {
            if (Current?.Frost?.Pointer==frost.Pointer && delta<0 && Current.Fractions.ContainsKey(location))
                Current.Fractions[location]=Math.Clamp(previous/-delta,0,1);
            frost.m_LocationsWithActiveFrostbite.Remove(location);
            frost.m_LocationsWithFrostbiteRisk.Remove(location);
        }
        else if (!frost.m_LocationsWithFrostbiteRisk.Contains(location)) frost.m_LocationsWithFrostbiteRisk.Add(location);
    }
}
[HarmonyPatch]
internal static class TemporaryFrostbiteStart
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Frostbite),"FrostbiteStart");
    private static bool Prefix(Game.Frostbite __instance,int location,out bool __state)
    {
        __state=ItemBalance.Active && __instance.m_LocationsWithActiveFrostbite.Contains(location);
        FrostbiteBalance.Configure(__instance);
        return !__state;
    }
    private static void Postfix(Game.Frostbite __instance,int location,bool __state)
    {
        if (ItemBalance.Active && !__state && __instance.m_LocationsWithActiveFrostbite.Contains(location)) FrostbiteBalance.FullGauge(__instance,location);
    }
}
[HarmonyPatch]
internal static class RecoverFrostbittenBodyAreas
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Frostbite),"DealFrostbiteDamageToLocation");
    private static bool Prefix(Game.Frostbite __instance,float damageValue,Game.AfflictionBodyArea bodyArea)
    {
        int location=(int)bodyArea;
        if (!ItemBalance.Active || !FrostbiteBalance.Valid(__instance,location) || !__instance.m_LocationsWithActiveFrostbite.Contains(location)) return true;
        FrostbiteBalance.ActiveGauge(__instance,location,damageValue);
        return false;
    }
}
[HarmonyPatch]
internal static class FrostbiteHealthDrain
{
    internal sealed class Scope { internal FrostbiteBalance.Tick Previous,Tick; }
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Frostbite),"UpdateFrostbite");
    private static void Prefix(Game.Frostbite __instance,out Scope __state)
    {
        __state=null;
        if (!ItemBalance.Active) return;
        __state=new Scope { Previous=FrostbiteBalance.Current,Tick=FrostbiteBalance.Begin(__instance,
            Game.GameManager.GetTimeOfDayComponent().GetTODHours(UnityEngine.Time.deltaTime)) };
        FrostbiteBalance.Current=__state.Tick;
    }
    private static void Postfix(Scope __state) { if (__state!=null) FrostbiteBalance.End(__state.Tick); }
    private static void Finalizer(Scope __state) { if (__state!=null) FrostbiteBalance.Current=__state.Previous; }
}
[HarmonyPatch]
internal static class InitializeTemporaryFrostbite
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Frostbite),"Start");
    private static void Postfix(Game.Frostbite __instance)=>FrostbiteBalance.Configure(__instance);
}
[HarmonyPatch]
internal static class SaveTemporaryFrostbite
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Frostbite),"Serialize");
    private static void Postfix(ref string __result)
    {
        if (!ItemBalance.Active || string.IsNullOrEmpty(__result)) return;
        var json=JsonNode.Parse(__result);json[FrostbiteBalance.Marker]=true;__result=json.ToJsonString();
    }
}
[HarmonyPatch]
internal static class LoadTemporaryFrostbite
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Frostbite),"Deserialize");
    private static void Postfix(Game.Frostbite __instance,string text)
    {
        if (!ItemBalance.Active) return;
        FrostbiteBalance.Configure(__instance);
        var unique=__instance.m_LocationsWithActiveFrostbite.ToArray().Distinct().ToArray();
        __instance.m_LocationsWithActiveFrostbite.Clear();
        bool legacy=string.IsNullOrEmpty(text) || JsonNode.Parse(text)?[FrostbiteBalance.Marker]==null;
        foreach (int location in unique)
        {
            if (!FrostbiteBalance.Valid(__instance,location)) continue;
            __instance.m_LocationsWithActiveFrostbite.Add(location);
            if (legacy) FrostbiteBalance.FullGauge(__instance,location);
            else if (__instance.m_LocationsCurrentFrostbiteDamage[location]<=0) __instance.m_LocationsWithActiveFrostbite.Remove(location);
        }
    }
}
