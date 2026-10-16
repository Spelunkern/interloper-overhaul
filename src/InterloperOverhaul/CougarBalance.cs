extern alias GameAssembly;
using System.Reflection;
using System.Text.Json.Nodes;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using AI = GameAssembly::Il2CppTLD.AI;

namespace InterloperOverhaul;
internal static class CougarBalance
{
    internal const float Scale=0.5f;
    private static readonly HashSet<int> initialized=new();
    internal static bool Eligible(Game.BaseAi ai) => ItemBalance.Active && ai!=null && ai.TryCast<AI.AiCougar>()!=null && ai.gameObject.scene.IsValid();
    internal static void Initialize(Game.BaseAi ai)
    {
        if (!Eligible(ai) || initialized.Contains(ai.GetInstanceID()) || ai.m_MaxHP<=0) return;
        initialized.Add(ai.GetInstanceID());
        ai.m_MaxHP*=CougarBalance.Scale;
        ai.m_CurrentHP*=CougarBalance.Scale;
    }
    internal static void Forget(Game.BaseAi ai) => initialized.Remove(ai.GetInstanceID());
    internal sealed class Swipe { internal float Minimum,Maximum,AiDamage; }
}
[HarmonyPatch]
internal static class InitializeCougarHealth
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.BaseAi),nameof(Game.BaseAi.Awake));
    private static void Postfix(Game.BaseAi __instance) => CougarBalance.Initialize(__instance);
}
[HarmonyPatch]
internal static class ForgetCougarHealth
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.BaseAi),nameof(Game.BaseAi.OnDestroy));
    private static void Prefix(Game.BaseAi __instance) => CougarBalance.Forget(__instance);
}
[HarmonyPatch]
internal static class SaveCougarHealth
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.BaseAi),nameof(Game.BaseAi.Serialize));
    private static void Prefix(Game.BaseAi __instance) => CougarBalance.Initialize(__instance);
    private static void Postfix(Game.BaseAi __instance,ref string __result)
    {
        if (!CougarBalance.Eligible(__instance) || string.IsNullOrEmpty(__result)) return;
        __result=ToNativeScale(__result);
    }
    internal static string ToNativeScale(string text)
    {
        // Keep native saves on the original HP scale. Legacy and new saves
        // share one format, without changing the live animal while saving.
        var json=JsonNode.Parse(text).AsObject();
        // The native serializer omits default-valued fields: a missing HP
        // field means zero, and must remain zero for dead animals.
        if (json.TryGetPropertyValue("m_CurrentHP",out var health) && health!=null)
            json["m_CurrentHP"]=health.GetValue<float>()/CougarBalance.Scale;
        return json.ToJsonString();
    }
}
[HarmonyPatch]
internal static class RestoreCougarHealth
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.BaseAi),nameof(Game.BaseAi.DeserializeUsingBaseAiDataProxy));
    internal sealed class Snapshot { internal float Current; }
    private static void Prefix(Game.BaseAi __instance,Game.BaseAiDataProxy proxy,out Snapshot __state)
    {
        __state=null;
        if (!CougarBalance.Eligible(__instance) || proxy==null) return;
        CougarBalance.Initialize(__instance);
        __state=new Snapshot { Current=proxy.m_CurrentHP };
        proxy.m_CurrentHP*=CougarBalance.Scale;
    }
    private static void Finalizer(Game.BaseAiDataProxy proxy,Snapshot __state)
    {
        if (__state!=null && proxy!=null) proxy.m_CurrentHP=__state.Current;
    }
}
[HarmonyPatch]
internal static class ReduceCougarSwipeDamage
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(AI.AiCougar),nameof(AI.AiCougar.DoPassingAttackDamage));
    private static void Prefix(AI.AiCougar __instance,out CougarBalance.Swipe __state)
    {
        __state=null;
        if (!CougarBalance.Eligible(__instance)) return;
        __state=new CougarBalance.Swipe { Minimum=__instance.m_PlayerPassingAttackDamageMin,
            Maximum=__instance.m_PlayerPassingAttackDamageMax,AiDamage=__instance.m_AiPassingAttackDamage };
        __instance.m_PlayerPassingAttackDamageMin*=CougarBalance.Scale;
        __instance.m_PlayerPassingAttackDamageMax*=CougarBalance.Scale;
        __instance.m_AiPassingAttackDamage*=CougarBalance.Scale;
    }
    private static void Finalizer(AI.AiCougar __instance,CougarBalance.Swipe __state)
    {
        if (__state==null || __instance==null) return;
        __instance.m_PlayerPassingAttackDamageMin=__state.Minimum;
        __instance.m_PlayerPassingAttackDamageMax=__state.Maximum;
        __instance.m_AiPassingAttackDamage=__state.AiDamage;
    }
}
