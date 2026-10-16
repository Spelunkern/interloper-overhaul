extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gear = GameAssembly::Il2CppTLD.Gear;

namespace InterloperOverhaul;
internal static class CookingBalance
{
    private static readonly Dictionary<int,Gear.GearItemData> pemmicanData=new();
    internal sealed class Immunity { internal Game.Skill_Cooking Skill; internal int Level; }
    internal static Immunity RuinedFood(Game.GearItem food)
    {
        if (!ItemBalance.Active || food==null || food.m_FoodItem==null || food.CurrentHP>0) return null;
        var skill=Game.GameManager.GetSkillCooking();
        if (skill==null) return null;
        var snapshot=new Immunity { Skill=skill,Level=skill.m_LevelWhereNoParasitesOrFoodPoisoning };
        // Native risk paths inline this threshold. Preserve all other Cooking
        // benefits and restore the immunity threshold after this food's checks.
        skill.m_LevelWhereNoParasitesOrFoodPoisoning=int.MaxValue;
        return snapshot;
    }
    internal static void Restore(Immunity state)
    {
        if (state?.Skill!=null) state.Skill.m_LevelWhereNoParasitesOrFoodPoisoning=state.Level;
    }
    internal static void Pemmican(Game.GearItem item)
    {
        if (!ItemBalance.Active || item==null || !item.gameObject.scene.IsValid() || LootBalancePolicy.ItemName(item.name)!="GEAR_CookedBarPemmican") return;
        var data=item.m_GearItemData;
        if (data==null) return;
        if (!pemmicanData.TryGetValue(data.GetInstanceID(),out var balanced) || balanced == null)
        {
            balanced=UnityEngine.Object.Instantiate(data).Cast<Gear.GearItemData>();
            balanced.hideFlags=UnityEngine.HideFlags.HideAndDontSave;
            balanced.name=data.name;
            balanced.m_DailyHPDecay=data.m_MaxHP*0.001f;
            pemmicanData[data.GetInstanceID()]=balanced;
            pemmicanData[balanced.GetInstanceID()]=balanced;
        }
        item.m_GearItemData=balanced;
        var food=item.m_FoodItem ?? item.GetComponent<Game.FoodItem>();
        if (food!=null) food.m_DailyHPDecayInside=food.m_DailyHPDecayOutside=balanced.m_DailyHPDecay;
    }
}
[HarmonyPatch]
internal static class BalancePemmicanDecay
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Awake));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.ManualStart));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Deserialize));
    }
    private static void Prefix(Game.GearItem __instance) => CookingBalance.Pemmican(__instance);
    private static void Postfix(Game.GearItem __instance) => CookingBalance.Pemmican(__instance);
}
[HarmonyPatch]
internal static class CheckRuinedFoodPoisoning
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.RollForFoodPoisoning));
    private static void Prefix(Game.GearItem __instance,out CookingBalance.Immunity __state) => __state=CookingBalance.RuinedFood(__instance);
    private static void Finalizer(CookingBalance.Immunity __state) => CookingBalance.Restore(__state);
}
[HarmonyPatch]
internal static class CheckRuinedFoodParasites
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.PlayerManager),nameof(Game.PlayerManager.EatingComplete_Internal));
    private static void Prefix(Game.PlayerManager __instance,out CookingBalance.Immunity __state) => __state=CookingBalance.RuinedFood(__instance.m_FoodItemEaten);
    private static void Finalizer(CookingBalance.Immunity __state) => CookingBalance.Restore(__state);
}
[HarmonyPatch]
internal static class DescribeMasterCooking
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Skill_Cooking),nameof(Game.Skill_Cooking.GetTierBenefits));
    private static void Postfix(int index,ref string __result)
    {
        if (ItemBalance.Active && index==4) __result+="\nRuined food can still cause food poisoning or parasites.";
    }
}
[HarmonyPatch]
internal static class PemmicanDailyDecayRate
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.GearItem),"DecayOverTODHours");
    private static void Prefix(Game.GearItem __instance,ref float scale)
    {
        if (!ItemBalance.Active || LootBalancePolicy.ItemName(__instance.name)!="GEAR_CookedBarPemmican") return;
        CookingBalance.Pemmican(__instance);
        float difficulty=Game.ExperienceModeManager.s_CurrentGameMode.m_XPMode.m_DecayScale;
        if (difficulty>0) scale/=difficulty;
    }
}
