#nullable disable

namespace InterloperOverhaul;

public sealed class ManufacturingIngredient
{
    public string Item { get; }
    public int Count { get; }
    public ManufacturingIngredient(string item, int count) { Item = item; Count = count; }
}
public sealed class ManufacturingRecipe
{
    public string Id { get; }
    public string Result { get; }
    public int Count { get; }
    public int Minutes { get; }
    public ManufacturingIngredient[] Ingredients { get; }
    public string[] Tools { get; }
    public double PowderRequiredKg { get; }
    public double PowderResultKg { get; }
    public ManufacturingRecipe(string id, string result, int count, int minutes, ManufacturingIngredient[] ingredients,
        string[] tools, double PowderRequiredKg = 0, double PowderResultKg = 0)
    {
        Id = id; Result = result; Count = count; Minutes = minutes; Ingredients = ingredients; Tools = tools;
        this.PowderRequiredKg = PowderRequiredKg; this.PowderResultKg = PowderResultKg;
    }
    public string BlueprintName => "IO_MANUFACTURING_" + Id;
    public string FlareInput => Ingredients.FirstOrDefault(item => ManufacturingPolicy.IsFlare(item.Item))?.Item ?? "";


}

public static class ManufacturingPolicy
{
    public static bool IsFlare(string name) => LootBalancePolicy.ItemName(name) is "GEAR_FlareA" or "GEAR_BlueFlare";
    public static bool IsMillable(string name) => LootBalancePolicy.ItemName(name) is "GEAR_FlareGun" or "GEAR_Crampons";
    public static readonly ManufacturingRecipe[] Recipes =
    {
        new("HuntingKnife", "GEAR_Knife", 1, 240,
            new[] { new ManufacturingIngredient("GEAR_KnifeImprovised", 1), new ManufacturingIngredient("GEAR_ScrapMetal", 4), new ManufacturingIngredient("GEAR_Leather", 1) }, new[] { "GEAR_HighQualityTools" }),
        new("Hatchet", "GEAR_Hatchet", 1, 360,
            new[] { new ManufacturingIngredient("GEAR_HatchetImprovised", 1), new ManufacturingIngredient("GEAR_ScrapMetal", 6), new ManufacturingIngredient("GEAR_Leather", 2) }, new[] { "GEAR_HighQualityTools" }),
        new("ArrowShafts", "GEAR_ArrowShaft", 4, 60,
            new[] { new ManufacturingIngredient("GEAR_BirchSaplingDried", 1) },
            new[] { "GEAR_Knife", "GEAR_KnifeImprovised", "GEAR_Hatchet", "GEAR_HatchetImprovised" }),
        new("ShellFromRedFlare", "GEAR_FlareGunAmmoSingle", 1, 90,
            new[] { new ManufacturingIngredient("GEAR_FlareA", 2), new ManufacturingIngredient("GEAR_ScrapMetal", 1), new ManufacturingIngredient("GEAR_Cloth", 1) },
            new[] { "GEAR_Knife", "GEAR_KnifeImprovised" }),
        new("ShellFromBlueFlare", "GEAR_FlareGunAmmoSingle", 1, 90,
            new[] { new ManufacturingIngredient("GEAR_BlueFlare", 2), new ManufacturingIngredient("GEAR_ScrapMetal", 1), new ManufacturingIngredient("GEAR_Cloth", 1) },
            new[] { "GEAR_Knife", "GEAR_KnifeImprovised" }),
        new("ShellFromPowder", "GEAR_FlareGunAmmoSingle", 1, 90,
            new[] { new ManufacturingIngredient("GEAR_ScrapMetal", 1), new ManufacturingIngredient("GEAR_Cloth", 1) }, Array.Empty<string>(), PowderRequiredKg: 0.1),
        new("PowderFromRedFlares", "GEAR_GunpowderCan", 1, 90,
            new[] { new ManufacturingIngredient("GEAR_FlareA", 3) }, new[] { "GEAR_Knife", "GEAR_KnifeImprovised" }, PowderResultKg: 0.1),
        new("PowderFromBlueFlares", "GEAR_GunpowderCan", 1, 90,
            new[] { new ManufacturingIngredient("GEAR_BlueFlare", 3) }, new[] { "GEAR_Knife", "GEAR_KnifeImprovised" }, PowderResultKg: 0.1)
    };
    public static ManufacturingRecipe Find(string blueprintName) => Recipes.FirstOrDefault(recipe => recipe.BlueprintName == blueprintName) ?? VariantPolicy.Recipe(blueprintName) ?? (blueprintName==FoodPreparationPolicy.BundleRecipe.BlueprintName ? FoodPreparationPolicy.BundleRecipe : null);
}
