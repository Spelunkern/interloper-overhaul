extern alias GameAssembly;
using System.Reflection;
using System.Text.Json.Nodes;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
namespace InterloperOverhaul;
internal static class ParasiteBalance
{
    internal const string RestKey="IO_ParasiteRestHours";
    private static readonly Dictionary<int,float> rest=new();
    internal static float RestHours(Game.IntestinalParasites parasites)=>parasites!=null && rest.TryGetValue(parasites.GetInstanceID(),out float value) ? value : 0;
    internal static void SetRest(Game.IntestinalParasites parasites,float hours)=>rest[parasites.GetInstanceID()]=Math.Clamp(hours,0,24);
    internal static void Configure(Game.IntestinalParasites parasites)
    {
        if (!ItemBalance.Active || parasites==null) return;
        parasites.m_MaxHitPointReductionPerDay=20;
        parasites.m_MaxFatigueReductionPerDay=20;
        if (!parasites.m_HasParasites) return;
        // Native cap calculations inline elapsed-days * penalty-per-day.
        // One fixed synthetic day keeps both caps at 20 without daily escalation.
        parasites.m_ParasitesElapsedHours=24;
        parasites.m_NumDosesTaken=Math.Clamp(parasites.m_NumDosesTaken,0,1);
        if (parasites.m_NumDosesTaken>0)
        {
            parasites.m_HasTakenDoseToday=true;
            parasites.m_DayToAllowNextDose=int.MaxValue;
        }
    }
    internal static void AddRest(Game.IntestinalParasites parasites,float hours)
    {
        if (!ItemBalance.Active || parasites==null || !parasites.m_HasParasites || parasites.m_NumDosesTaken<1 || hours<=0) return;
        SetRest(parasites,RestHours(parasites)+hours);
        if (RestHours(parasites)>=24) parasites.IntestinalParasitesEnd();
    }
    internal static string Description(Game.IntestinalParasites parasites)=>
        "Intestinal parasites reduce maximum condition and maximum fatigue capacity by 20%. The penalties do not worsen. " +
        "Take one normal dose of antibiotics or reishi tea, then rest for 24 hours. " +
        $"Rest remaining: {Math.Max(0,24-RestHours(parasites)):0.0} hours.";
}
[HarmonyPatch]
internal static class FixedParasitePenalties
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.IntestinalParasites),"IntestinalParasitesStart");
    private static void Postfix(Game.IntestinalParasites __instance)
    {
        if (!ItemBalance.Active) return;
        ParasiteBalance.SetRest(__instance,0);
        ParasiteBalance.Configure(__instance);
    }
}
[HarmonyPatch]
internal static class StopParasiteEscalation
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.IntestinalParasites),"UpdateIntestinalParasites");
    private static bool Prefix(Game.IntestinalParasites __instance)
    {
        if (!ItemBalance.Active) return true;
        ParasiteBalance.Configure(__instance);
        if (Game.GameManager.GetPassTime()?.IsPassingTime()==true && Game.GameManager.GetRestComponent()?.IsSleeping()!=true)
            ParasiteBalance.AddRest(__instance,Game.GameManager.GetTimeOfDayComponent().GetTODHours(UnityEngine.Time.deltaTime));
        return false;
    }
}
[HarmonyPatch]
internal static class OneParasiteDose
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.IntestinalParasites),"GetNumDosesRequired");
    private static bool Prefix(ref int __result)
    {
        if (!ItemBalance.Active) return true;
        __result=1;return false;
    }
}
[HarmonyPatch]
internal static class RegisterParasiteTreatment
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.IntestinalParasites),"TakeAntibiotics");
    private static bool Prefix(Game.IntestinalParasites __instance)
    {
        if (!ItemBalance.Active || !__instance.m_HasParasites) return true;
        // The native method immediately ends infection after its last dose.
        // Register that same treatment here while retaining the rest requirement.
        __instance.m_NumDosesTaken=1;
        ParasiteBalance.Configure(__instance);
        return false;
    }
    private static void Postfix(Game.IntestinalParasites __instance)=>ParasiteBalance.Configure(__instance);
}
[HarmonyPatch]
internal static class RestToCureParasites
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Rest),"UpdateRestForCures");
    private static void Postfix(float todSeconds)
    {
        if (ItemBalance.Active) ParasiteBalance.AddRest(Game.GameManager.GetIntestinalParasitesComponent(),todSeconds/3600);
    }
}
[HarmonyPatch]
internal static class ParasitesNeedRest
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Rest),"RestNeededForAffliction");
        yield return AccessTools.Method(typeof(Game.Rest),"AllowUnlimitedSleep");
    }
    private static void Postfix(ref bool __result)
    {
        if (ItemBalance.Active && Game.GameManager.GetIntestinalParasitesComponent()?.m_HasParasites==true) __result=true;
    }
}
[HarmonyPatch]
internal static class SaveParasiteRecovery
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.IntestinalParasites),"Serialize");
    private static void Postfix(Game.IntestinalParasites __instance,ref string __result)
    {
        if (!ItemBalance.Active || string.IsNullOrEmpty(__result)) return;
        var json=JsonNode.Parse(__result);json[ParasiteBalance.RestKey]=ParasiteBalance.RestHours(__instance);__result=json.ToJsonString();
    }
}
[HarmonyPatch]
internal static class LoadParasiteRecovery
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.IntestinalParasites),"Deserialize");
    private static void Postfix(Game.IntestinalParasites __instance,string text)
    {
        if (!ItemBalance.Active) return;
        ParasiteBalance.SetRest(__instance,string.IsNullOrEmpty(text) ? 0 : JsonNode.Parse(text)?[ParasiteBalance.RestKey]?.GetValue<float>() ?? 0);
        ParasiteBalance.Configure(__instance);
    }
}
[HarmonyPatch]
internal static class InstantSevereLacerations
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.SevereLacerations),"ApplySevereLacerations");
    private static void Postfix(Game.SevereLacerations __instance)
    {
        // Keep the initial attack damage and wounds; end only the ongoing syndrome.
        if (ItemBalance.Active && __instance.HasAffliction()) __instance.Cure(Game.AfflictionOptions.None);
    }
}
[HarmonyPatch]
internal static class NoRecurringLacerations
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.SevereLacerations),"Update");
    private static bool Prefix(Game.SevereLacerations __instance)
    {
        if (!ItemBalance.Active) return true;
        if (__instance.HasAffliction()) __instance.Cure(Game.AfflictionOptions.None);
        return false;
    }
}
[HarmonyPatch]
internal static class LoadInstantLacerations
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.SevereLacerations),"Deserialize");
    private static void Postfix(Game.SevereLacerations __instance)
    {
        if (ItemBalance.Active && __instance.HasAffliction()) __instance.Cure(Game.AfflictionOptions.None);
    }
}
internal static class CabinFeverBalance
{
    internal static void Clear(Game.CabinFever fever)
    {
        fever.m_Active=false;fever.m_RiskActive=false;fever.m_ElapsedHours=0;
        fever.m_HoursIndoorsExcludingLastHourCached=0;fever.m_IndoorTimeTracked.Clear();
    }
}
[HarmonyPatch]
internal static class DisableCabinFever
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.CabinFever),"Update");
        yield return AccessTools.Method(typeof(Game.CabinFever),"CabinFeverStart");
        yield return AccessTools.Method(typeof(Game.CabinFever),"CheckForCabinFever");
    }
    private static bool Prefix(Game.CabinFever __instance)
    {
        if (!ItemBalance.Active) return true;
        CabinFeverBalance.Clear(__instance);return false;
    }
}
[HarmonyPatch]
internal static class LoadWithoutCabinFever
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.CabinFever),"Deserialize");
    private static void Postfix(Game.CabinFever __instance) { if (ItemBalance.Active) CabinFeverBalance.Clear(__instance); }
}
[HarmonyPatch]
internal static class CabinFeverModeDisabled
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.CabinFever),"DisabledForGameMode");
    private static void Postfix(ref bool __result) { if (ItemBalance.Active) __result=true; }
}
[HarmonyPatch]
internal static class TemporaryFrostbiteDescription
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Localization),"Get",new[] { typeof(string) });
    private static bool Prefix(string key,ref string __result)
    {
        if (key!="IO_FrostbiteDescription") return true;
        __result="Frostbite drains 10% condition per hour for each affected body area. Warm up to reduce the meter rapidly. Each area heals when its meter empties; there is no permanent condition penalty.";
        return false;
    }
}
