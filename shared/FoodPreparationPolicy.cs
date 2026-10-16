namespace InterloperOverhaul;

public static class FoodPreparationPolicy
{
    public const string Bundle = "GEAR_IO_CattailBundle";
    public const string Cattail = "GEAR_CattailStalk";
    public static readonly ManufacturingRecipe BundleRecipe = new("CattailBundle",Bundle,1,1,
        new[] { new ManufacturingIngredient(Cattail,10) },Array.Empty<string>());
    public static bool IsBundle(string name) => LootBalancePolicy.ItemName(name)==Bundle;
}
