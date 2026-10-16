namespace InterloperOverhaul;

public static class ItemBalancePolicy
{
    public const float LanternWarmthCelsius = 6;
    public static bool IsFlask(string name)
    {
        name = LootBalancePolicy.ItemName(name);
        return name == "GEAR_InsulatedFlask" || name.StartsWith("GEAR_InsulatedFlask_", StringComparison.Ordinal);
    }
    public static bool IsBook(string name) => LootBalancePolicy.ItemName(name).StartsWith("GEAR_Book", StringComparison.Ordinal);
    public static double WeightKg(string name)
    {
        name = LootBalancePolicy.ItemName(name);
        if (IsFlask(name)) return 0.10;
        if (IsBook(name)) return 0.20;
        return name switch
        {
            "GEAR_MooseHideBag" or "GEAR_EarMuffs" or "GEAR_CanOpener" or "GEAR_RecycledCan" or "GEAR_Stone" or "GEAR_Arrow" or "GEAR_Fuse" => 0.10,
            "GEAR_BallisticVest" => 2.00,
            "GEAR_HatchetImprovised" => 1.50,
            "GEAR_KnifeImprovised" => 0.50,
            "GEAR_FlareA" or "GEAR_BlueFlare" or "GEAR_Snare" or "GEAR_SprayPaintCan" or "GEAR_SprayPaintCanGlyphA" => 0.20,
            "GEAR_CarBattery" => 5.00,
            "GEAR_Rope" => 2.50,
            "GEAR_WoodworkingTools" => 1.00,
            _ => -1
        };
    }
}
