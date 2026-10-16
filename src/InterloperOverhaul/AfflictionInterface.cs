extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
namespace InterloperOverhaul;
internal static class AfflictionInterface
{
    private static readonly Dictionary<int,Game.AfflictionDefinition> definitions=new();
    internal static Game.AfflictionDefinition Definition(Game.AfflictionDefinition original)
    {
        if (!ItemBalance.Active || original==null || (original.m_AfflictionType!=Game.AfflictionType.Frostbite && original.m_AfflictionType!=Game.AfflictionType.IntestinalParasites)) return original;
        if (definitions.TryGetValue(original.GetInstanceID(),out var cached) && cached!=null) return cached;
        var clone=UnityEngine.Object.Instantiate(original).Cast<Game.AfflictionDefinition>();
        clone.hideFlags=UnityEngine.HideFlags.HideAndDontSave;
        clone.name=original.name;
        bool frost=original.m_AfflictionType==Game.AfflictionType.Frostbite;
        clone.m_HPReduction=frost ? 0 : 20;
        clone.m_DescriptionLocId=frost ? "IO_FrostbiteDescription" : "IO_ParasiteDescription";
        clone.m_TreatmentDescriptionLocId=frost ? "IO_FrostbiteTreatment" : "IO_ParasiteTreatment";
        clone.m_RestHoursRequired=frost ? 0 : 24;
        definitions[original.GetInstanceID()]=clone;
        definitions[clone.GetInstanceID()]=clone;
        return clone;
    }
}
[HarmonyPatch]
internal static class RevisedAfflictionDefinitions
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.AfflictionDefinitionTable),"GetAfflictionDefinitionByType");
        yield return AccessTools.Method(typeof(Game.AfflictionDefinitionTable),"GetAfflictionDefinitionByName");
    }
    private static void Postfix(ref Game.AfflictionDefinition __result)=>__result=AfflictionInterface.Definition(__result);
}
[HarmonyPatch]
internal static class AfflictionRecoveryText
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Localization),"Get",new[] { typeof(string) });
    private static bool Prefix(string key,ref string __result)
    {
        if (key=="IO_ParasiteDescription") __result=ParasiteBalance.Description(Game.GameManager.GetIntestinalParasitesComponent());
        else if (key=="IO_ParasiteTreatment") __result="One normal dose of antibiotics or reishi tea, followed by 24 hours of sleeping or passing time.";
        else if (key=="IO_FrostbiteTreatment") __result="Warm up until this body area's frostbite meter empties.";
        else return true;
        return false;
    }
}
[HarmonyPatch]
internal static class DisplayAfflictionRecovery
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Panel_FirstAid),"RefreshRightPage");
    private static void Postfix(Game.Panel_FirstAid __instance)
    {
        if (!ItemBalance.Active) return;
        var type=__instance.GetSelectedAfflictionType();
        if (type==Game.AfflictionType.IntestinalParasites)
        {
            var parasites=Game.GameManager.GetIntestinalParasitesComponent();
            if (parasites==null) return;
            string text=ParasiteBalance.Description(parasites);
            if (__instance.m_LabelAfflictionDescription!=null) __instance.m_LabelAfflictionDescription.text=text;
            if (__instance.m_LabelAfflictionDescriptionNoRest!=null) __instance.m_LabelAfflictionDescriptionNoRest.text=text;
            if (__instance.m_ObjectRestRemaining!=null) __instance.m_ObjectRestRemaining.SetActive(true);
            if (__instance.m_LabelRestRemaining!=null) __instance.m_LabelRestRemaining.text=$"{Math.Max(0,24-ParasiteBalance.RestHours(parasites)):0.0} h";
            if (__instance.m_LabelRestRequired!=null) __instance.m_LabelRestRequired.text="24 h";
        }
        else if (type==Game.AfflictionType.Frostbite)
        {
            string text=Game.Localization.Get("IO_FrostbiteDescription");
            if (__instance.m_LabelAfflictionDescription!=null) __instance.m_LabelAfflictionDescription.text=text;
            if (__instance.m_LabelAfflictionDescriptionNoRest!=null) __instance.m_LabelAfflictionDescriptionNoRest.text=text;
        }
    }
}
