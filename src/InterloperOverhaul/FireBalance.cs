extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;

namespace InterloperOverhaul;
internal static class FireBalance
{
    [ThreadStatic] internal static Game.FuelSourceItem SelectedTinder;
    internal static void Configure(Game.FuelSourceItem fuel)
    {
        if (!ItemBalance.Active || fuel==null || !fuel.gameObject.scene.IsValid()) return;
        string name=LootBalancePolicy.ItemName(fuel.name);
        int tinder=FireBalancePolicy.TinderBonus(name);
        if (fuel.m_IsTinder && tinder>0) fuel.m_FireStartSkillModifier=tinder;
        float hours=FireBalancePolicy.BurnHours(name);
        if (hours>=0) fuel.m_BurnDurationHours=hours;
        var bonus=FireBalancePolicy.FuelSuccessBonus(name);
        if (bonus.HasValue) fuel.m_FireStartSkillModifier=bonus.Value;
        if (name=="GEAR_Firelog") fuel.m_HeatIncrease=10;
    }
}
[HarmonyPatch]
internal static class ConfigureFireMaterials
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Awake));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.ManualStart));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Deserialize));
    }
    private static void Postfix(Game.GearItem __instance) => FireBalance.Configure(__instance.m_FuelSourceItem ?? __instance.GetComponent<Game.FuelSourceItem>());
}
[HarmonyPatch]
internal static class BalanceFuelDuration
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FuelSourceItem),nameof(Game.FuelSourceItem.GetModifiedBurnDurationHours));
    private static void Prefix(Game.FuelSourceItem __instance) => FireBalance.Configure(__instance);
}
[HarmonyPatch]
internal static class PreviewTinderBonus
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Panel_FireStart),nameof(Game.Panel_FireStart.RefreshChanceOfSuccessLabel));
    private static void Prefix(Game.Panel_FireStart __instance,out Game.FuelSourceItem __state)
    {
        __state=FireBalance.SelectedTinder;
        FireBalance.SelectedTinder=ItemBalance.Active ? __instance.GetSelectedTinder() : null;
    }
    private static void Finalizer(Game.FuelSourceItem __state) => FireBalance.SelectedTinder=__state;
}
[HarmonyPatch]
internal static class ApplyTinderBonus
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FireManager),nameof(Game.FireManager.PlayerStartFire));
    private static void Prefix(Game.FuelSourceItem tinder,out Game.FuelSourceItem __state)
    {
        __state=FireBalance.SelectedTinder;
        FireBalance.SelectedTinder=ItemBalance.Active ? tinder : null;
    }
    private static void Finalizer(Game.FuelSourceItem __state) => FireBalance.SelectedTinder=__state;
}
[HarmonyPatch]
internal static class CalculateTinderBonus
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FireManager),nameof(Game.FireManager.CalculateFireStartSuccess));
    internal sealed class Snapshot { internal float Bonus; }
    private static void Prefix(Game.FuelSourceItem fuel,out Snapshot __state)
    {
        __state=null;
        if (!ItemBalance.Active || fuel==null) return;
        FireBalance.Configure(fuel);
        var tinder=FireBalance.SelectedTinder;
        int bonus=tinder!=null && tinder.m_IsTinder ? FireBalancePolicy.TinderBonus(tinder.name) : 0;
        if (bonus==0) return;
        // Add before the game's 0..100 clamp. Restore immediately, including
        // exception paths, so repeated previews cannot accumulate the bonus.
        __state=new Snapshot { Bonus=fuel.m_FireStartSkillModifier };
        fuel.m_FireStartSkillModifier+=bonus;
    }
    private static void Finalizer(Game.FuelSourceItem fuel,Snapshot __state)
    {
        if (__state!=null && fuel!=null) fuel.m_FireStartSkillModifier=__state.Bonus;
    }
}
[HarmonyPatch]
internal static class DescribeTinderBonus
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.Panel_FireStart),nameof(Game.Panel_FireStart.RefreshTinderLabel));
    private static void Postfix(Game.Panel_FireStart __instance)
    {
        if (!ItemBalance.Active || __instance.m_Label_Tinder==null) return;
        var tinder=__instance.GetSelectedTinder();
        int bonus=tinder==null ? 0 : FireBalancePolicy.TinderBonus(tinder.name);
        if (bonus>0) __instance.m_Label_Tinder.text+=" (+"+bonus+"%)";
    }
}
