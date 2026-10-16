#nullable disable
namespace InterloperOverhaul;

public sealed class ClothingAdjustment
{
    public string Item { get; }
    public float? Warmth { get; }
    public float? Wind { get; }
    public float? Protection { get; }
    public float? SprintPenalty { get; }
    public ClothingAdjustment(string item,float? warmth=null,float? wind=null,float? protection=null,float? sprintPenalty=null)
    { Item=item; Warmth=warmth; Wind=wind; Protection=protection; SprintPenalty=sprintPenalty; }
}
public sealed class CalorieAdjustment
{
    public string Item { get; }
    public float Original { get; }
    public float Target { get; }
    public CalorieAdjustment(string item,float original,float target) { Item=item; Original=original; Target=target; }
}
public static class ClothingNutritionPolicy
{
    public static readonly ClothingAdjustment[] Clothing =
    {
        new("GEAR_MinersHelmet",protection:10),
        new("GEAR_CottonScarf",wind:1),
        new("GEAR_BearSkinCoat",warmth:5,sprintPenalty:15),
        new("GEAR_MooseHideCloak",warmth:4,protection:25),
        new("GEAR_WolfSkinCape",protection:10,sprintPenalty:5),
        new("GEAR_ImprovisedCrampons",protection:0,sprintPenalty:10)
    };
    public static readonly CalorieAdjustment[] Calories =
    {
        new("GEAR_TomatoSoupCan",300,600), new("GEAR_CannedHam",550,1500),
        new("GEAR_CannedPineapple",295,600), new("GEAR_Cereal_A",700,1000),
        new("GEAR_DriedApples",400,600), new("GEAR_Pickles",650,800)
    };
    public static CalorieAdjustment Food(string name) => Calories.FirstOrDefault(c => c.Item==LootBalancePolicy.ItemName(name));
    // These containers measure recipe resources directly. Scale carried mass,
    // preserving their native powder/liquid amounts, capacities, and recipes.
    public static float ContentWeightScale(string name) => LootBalancePolicy.ItemName(name) switch
    {
        "GEAR_CookingOil" => 0.5f/0.75f,
        "GEAR_Flour" => 1f/1.5f,
        "GEAR_OatsTin" => 0.75f/0.80f,
        "GEAR_Salt" => 0.10f/0.25f,
        "GEAR_SaltBag" => 3f/10f,
        _ => 1
    };
}
