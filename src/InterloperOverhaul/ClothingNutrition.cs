extern alias GameAssembly;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Units = Il2CppTLD.IntBackedUnit;

namespace InterloperOverhaul;
internal static class ClothingNutrition
{
    internal const string CalorieMarker = "InterloperOverhaulCalorieBase";
    internal static void Configure(Game.GearItem item)
    {
        if (!ItemBalance.Active || item==null || !item.gameObject.scene.IsValid()) return;
        string name=LootBalancePolicy.ItemName(item.name);
        var adjustment=ClothingNutritionPolicy.Clothing.FirstOrDefault(c => c.Item==name);
        var clothing=item.m_ClothingItem ?? item.GetComponent<Game.ClothingItem>();
        if (adjustment!=null && clothing!=null)
        {
            if (adjustment.Warmth.HasValue) clothing.m_Warmth=adjustment.Warmth.Value;
            if (adjustment.Wind.HasValue) clothing.m_Windproof=adjustment.Wind.Value;
            if (adjustment.Protection.HasValue) clothing.m_Toughness=adjustment.Protection.Value;
            if (adjustment.SprintPenalty.HasValue) clothing.m_SprintBarReductionPercent=adjustment.SprintPenalty.Value;
        }
        if (name=="GEAR_AcornCoffeeCup")
        {
            var caffeine=item.m_FatigueBuff ?? item.GetComponent<Game.FatigueBuff>();
            if (caffeine!=null)
            {
                caffeine.m_InitialPercentDecrease=10;
                caffeine.m_RateOfIncreaseScale=0.5f;
                caffeine.m_DurationHours=1;
            }
        }
        if (name=="GEAR_BurdockTea")
        {
            var recovery=item.m_ConditionOverTimeBuff ?? item.GetComponent<Game.ConditionOverTimeBuff>() ?? item.gameObject.AddComponent<Game.ConditionOverTimeBuff>();
            recovery.m_ConditionIncreasePerHour=2.5f;
            recovery.m_NumHours=2;
            recovery.m_HUD=(item.m_FreezingBuff ?? item.GetComponent<Game.FreezingBuff>())?.m_HUD;
            item.m_ConditionOverTimeBuff=recovery;
        }
    }
    internal static void Calories(Game.FoodItem food,float previousBase)
    {
        if (!ItemBalance.Active || food==null || !food.gameObject.scene.IsValid()) return;
        var adjustment=ClothingNutritionPolicy.Food(food.name);
        if (adjustment==null) return;
        float scale=adjustment.Target/(previousBase>0 && float.IsFinite(previousBase) ? previousBase : adjustment.Original);
        // Scale both values so partial consumption and native cooking bonuses
        // survive migration. Never refill food or repeatedly normalize its total.
        food.m_CaloriesRemaining*=scale;
        food.m_CaloriesTotal*=scale;
    }
}
[HarmonyPatch]
internal static class ConfigureClothingNutrition
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Awake));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.ManualStart));
        yield return AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.Deserialize));
    }
    private static void Prefix(Game.GearItem __instance) => ClothingNutrition.Configure(__instance);
    private static void Postfix(Game.GearItem __instance) => ClothingNutrition.Configure(__instance);
}
[HarmonyPatch]
internal static class BalanceFreshFoodCalories
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FoodItem),nameof(Game.FoodItem.Awake));
    private static void Postfix(Game.FoodItem __instance) => ClothingNutrition.Calories(__instance,0);
}
[HarmonyPatch]
internal static class SaveBalancedFoodCalories
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FoodItem),nameof(Game.FoodItem.Serialize));
    private static void Postfix(Game.FoodItem __instance,ref string __result)
    {
        if (!ItemBalance.Active || !__instance.gameObject.scene.IsValid()) return;
        var adjustment=ClothingNutritionPolicy.Food(__instance.name);
        if (adjustment==null || string.IsNullOrEmpty(__result)) return;
        var json=JsonNode.Parse(__result).AsObject();
        json[ClothingNutrition.CalorieMarker]=adjustment.Target;
        __result=json.ToJsonString();
    }
}
[HarmonyPatch]
internal static class RestoreBalancedFoodCalories
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.FoodItem),nameof(Game.FoodItem.Deserialize));
    private static void Postfix(Game.FoodItem __instance,string text)
    {
        if (!ItemBalance.Active || ClothingNutritionPolicy.Food(__instance.name)==null) return;
        float previous=0;
        if (!string.IsNullOrEmpty(text))
        {
            using var json=JsonDocument.Parse(text);
            if (json.RootElement.TryGetProperty(ClothingNutrition.CalorieMarker,out var marker)) marker.TryGetSingle(out previous);
        }
        ClothingNutrition.Calories(__instance,previous);
    }
}
[HarmonyPatch]
internal static class BalanceIngredientCarriedWeight
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GearItem),nameof(Game.GearItem.GetItemWeightKG),new[] { typeof(float),typeof(bool) });
    private static void Postfix(Game.GearItem __instance,float stackMultiplier,ref Units.ItemWeight __result)
    {
        if (!ItemBalance.Active) return;
        if (LootBalancePolicy.ItemName(__instance.name)=="GEAR_CookingOil" && __instance.m_LiquidItem!=null &&
            __instance.m_LiquidItem.m_LiquidCapacity>Units.ItemLiquidVolume.Zero)
        {
            // Native content mass rounds to 10 g before scaling. Use the actual
            // volume fraction so a half-full bottle weighs exactly half as much.
            var liquid=__instance.m_LiquidItem;
            __result=Units.ItemWeight.FromKilograms(0.5f)*(liquid.m_Liquid/liquid.m_LiquidCapacity)*stackMultiplier;
        }
        else
        {
            float scale=ClothingNutritionPolicy.ContentWeightScale(__instance.name);
            if (scale!=1 && __instance.m_PowderItem!=null)
                __result=__instance.m_PowderItem.m_Weight*scale*stackMultiplier;
        }
    }
}
