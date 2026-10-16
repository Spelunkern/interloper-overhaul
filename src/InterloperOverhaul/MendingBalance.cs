extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Game = GameAssembly::Il2Cpp;

namespace InterloperOverhaul;
internal static class SkillTierBalance
{
    private sealed class Tiers
    {
        internal Il2CppStructArray<int> Original,Balanced;
    }
    private static readonly Dictionary<int,Tiers> tiers=new();
    internal static void Configure(Game.Skill skill)
    {
        if (skill==null || (skill.TryCast<Game.Skill_ClothingRepair>()==null && skill.TryCast<Game.Skill_Cooking>()==null) || skill.m_TierPoints==null || skill.m_TierPoints.Length<5) return;
        int id=skill.GetInstanceID();
        if (!tiers.TryGetValue(id,out var state))
        {
            var balanced=skill.m_TierPoints.ToArray();
            balanced[4]=250;
            tiers[id]=state=new Tiers { Original=skill.m_TierPoints,Balanced=balanced };
        }
        skill.m_TierPoints=ItemBalance.Active ? state.Balanced : state.Original;
    }
}
internal static class MendingBalance
{
    internal static void Configure(Game.Skill skill) => SkillTierBalance.Configure(skill);
    internal static bool CanRestore(Game.GearItem gear)
    {
        if (!ItemBalance.Active || gear==null || gear.m_Repairable==null || !gear.m_WornOut || gear.CurrentHP>0) return false;
        var skill=Game.GameManager.GetSkillClothingRepair();
        Configure(skill);
        return skill!=null && skill.GetCurrentTierNumber()>=4;
    }
    internal sealed class RepairState { internal Game.GearItem Gear; }
}
[HarmonyPatch]
internal static class ConfigureMendingTiers
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Skill),"Start");
        yield return AccessTools.Method(typeof(Game.Skill),nameof(Game.Skill.SetPoints));
        yield return AccessTools.Method(typeof(Game.Skill),nameof(Game.Skill.IncrementPoints));
        yield return AccessTools.Method(typeof(Game.Skill),nameof(Game.Skill.GetCurrentTierNumber));
        yield return AccessTools.Method(typeof(Game.Skill_ClothingRepair),nameof(Game.Skill_ClothingRepair.Deserialize));
        yield return AccessTools.Method(typeof(Game.Skill_Cooking),nameof(Game.Skill_Cooking.Deserialize));
    }
    private static void Prefix(Game.Skill __instance) => SkillTierBalance.Configure(__instance);
}
[HarmonyPatch]
internal static class RepairRuinedItems
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Panel_Inventory_Examine),nameof(Game.Panel_Inventory_Examine.RefreshRepairPanel));
        yield return AccessTools.Method(typeof(Game.Panel_Inventory_Examine),nameof(Game.Panel_Inventory_Examine.RepairSuccessful));
    }
    private static void Prefix(Game.Panel_Inventory_Examine __instance,out MendingBalance.RepairState __state)
    {
        __state=null;
        var gear=__instance.m_GearItem;
        if (!MendingBalance.CanRestore(gear)) return;
        __state=new MendingBalance.RepairState { Gear=gear };
        // Only bypass the ruined flag inside the native repair path. Keep zero
        // HP until the normal paid repair succeeds; other actions stay blocked.
        gear.m_WornOut=false;
    }
    private static void Finalizer(MendingBalance.RepairState __state)
    {
        if (__state?.Gear!=null) __state.Gear.m_WornOut=__state.Gear.CurrentHP<=0;
    }
}
[HarmonyPatch]
internal static class DescribeMasterMending
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Skill_ClothingRepair),nameof(Game.Skill_ClothingRepair.GetTierBenefits));
    private static void Postfix(int index,ref string __result)
    {
        if (ItemBalance.Active && index==4) __result+="\nCan repair ruined items with a native repair action.";
    }
}
