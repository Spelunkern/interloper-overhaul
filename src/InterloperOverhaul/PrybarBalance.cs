extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
namespace InterloperOverhaul;
internal static class PrybarBalance
{
    private static readonly Dictionary<int,(float Damage,float Flee)> originals=new();
    internal static void Configure(Game.GearItem item)
    {
        if (!ItemBalance.Active || item==null || !item.gameObject.scene.IsValid() || LootBalancePolicy.ItemName(item.name)!="GEAR_Prybar") return;
        var bonus=item.GetComponent<Game.StruggleBonus>();
        if (bonus==null) return;
        int id=bonus.GetInstanceID();
        if (!originals.TryGetValue(id,out var original)) originals[id]=original=(bonus.m_DamageScalePercent,bonus.m_FleeChanceScale);
        bonus.m_DamageScalePercent=original.Damage*2;
        bonus.m_FleeChanceScale=original.Flee*2;
    }
}
[HarmonyPatch]
internal static class DefensivePrybar
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem),"Awake");
        yield return AccessTools.Method(typeof(Game.GearItem),"ManualStart");
        yield return AccessTools.Method(typeof(Game.GearItem),"Deserialize");
    }
    private static void Postfix(Game.GearItem __instance)=>PrybarBalance.Configure(__instance);
}
