#nullable disable
namespace InterloperOverhaul;

public sealed class VariantSpec
{
    public string Item { get; }
    public string Name { get; }
    public string Base { get; }
    public ManufacturingRecipe Upgrade { get; }
    public ManufacturingRecipe Reverse { get; }
    public VariantSpec(string item, string name, string basis, int minutes, params ManufacturingIngredient[] materials)
    {
        Item=item; Name=name; Base=basis;
        string[] tools = basis == "GEAR_Bow" ? new[] { "GEAR_Knife", "GEAR_KnifeImprovised", "GEAR_SurvivalKnife" } : new[] { "GEAR_HighQualityTools" };
        Upgrade = new("Upgrade_" + item, item, 1, minutes, new[] { new ManufacturingIngredient(basis,1) }.Concat(materials).ToArray(), tools);
        if (FirearmSpawnPolicy.IsStandardFirearm(basis))
            Reverse = new("Revert_" + item, basis, 1, basis == "GEAR_Rifle" ? 60 : 30, new[] { new ManufacturingIngredient(item,1) }, Array.Empty<string>());
    }
    public string Title => "Workshop Schematic — " + Name;
    public string Description => "Technical notes describing how to modify a standard " + Base.Replace("GEAR_", "").ToLowerInvariant() + " into a " + Name + ". Requires the Cannery workshop.";
}

public static class VariantPolicy
{
    private static ManufacturingIngredient M(string item, int count) => new("GEAR_" + item,count);
    public static readonly VariantSpec[] Variants =
    {
        new("GEAR_SurvivalKnife", "Survival Knife", "GEAR_Knife",180,M("ScrapMetal",3),M("Leather",1),M("Cloth",1)),
        new("GEAR_RevolverGreen", "Forester's Revolver", "GEAR_Revolver",240,M("ScrapMetal",3),M("MapleSaplingDried",1),M("Leather",1)),
        new("GEAR_RevolverStubNosed", "Warden's Revolver", "GEAR_Revolver",180,M("ScrapMetal",2),M("Leather",1)),
        new("GEAR_RevolverFancy", "Hunter's Revolver", "GEAR_Revolver",300,M("ScrapMetal",4),M("Leather",2)),
        new("GEAR_Rifle_Vaughns", "Vaughn's Rifle", "GEAR_Rifle",300,M("ScrapMetal",3),M("Leather",1)),
        new("GEAR_Rifle_Barbs", "Barb's Rifle", "GEAR_Rifle",420,M("ScrapMetal",6),M("MapleSaplingDried",1),M("Leather",2)),
        new("GEAR_Rifle_Curators", "Curator's Rifle", "GEAR_Rifle",480,M("ScrapMetal",5),M("MapleSaplingDried",1),M("Cloth",1)),
        new("GEAR_Rifle_Trader", "Ranger's Rifle", "GEAR_Rifle",420,M("ScrapMetal",4),M("MapleSaplingDried",1),M("Leather",2)),
        new("GEAR_Bow_Woodwrights", "Woodwright's Bow", "GEAR_Bow",300,M("MapleSaplingDried",1),M("GutDried",2),M("Leather",1)),
        new("GEAR_Bow_Manufactured", "Sport Bow", "GEAR_Bow",360,M("ScrapMetal",2),M("MapleSaplingDried",1),M("GutDried",2)),
        new("GEAR_Bow_Bushcraft", "Bushcraft Bow", "GEAR_Bow",240,M("BirchSaplingDried",1),M("GutDried",2),M("Cloth",1))
    };
    public static IEnumerable<ManufacturingRecipe> Recipes => Variants.Select(v => v.Upgrade).Concat(Variants.Where(v => v.Reverse != null).Select(v => v.Reverse));
    public static VariantSpec Find(string item) => Variants.FirstOrDefault(v => v.Item == LootBalancePolicy.ItemName(item));
    public static ManufacturingRecipe Recipe(string name) => Recipes.FirstOrDefault(r => r.BlueprintName == name);
    public static bool Unlocked(ManufacturingRecipe recipe, ICollection<string> learned)
    {
        var variant = Variants.FirstOrDefault(v => v.Upgrade == recipe || v.Reverse == recipe);
        return variant != null && (variant.Upgrade == recipe ? learned.Contains(variant.Item) : Variants.Any(v => v.Base == variant.Base && learned.Contains(v.Item)));
    }
}
