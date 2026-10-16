namespace InterloperOverhaul;

public static class FireBalancePolicy
{
    public static int TinderBonus(string name) => LootBalancePolicy.ItemName(name) switch
    {
        "GEAR_Newsprint" or "GEAR_NewsprintRoll" or "GEAR_PaperStack" => 8,
        "GEAR_Tinder" or "GEAR_BarkTinder" or "GEAR_CattailTinder" or "GEAR_CashBundle" => 4,
        _ => 0
    };
    public static float BurnHours(string name)
    {
        if (ItemBalancePolicy.IsBook(name)) return 0.5f;
        return LootBalancePolicy.ItemName(name) switch
        {
            "GEAR_Firelog" => 3,
            "GEAR_Stick" => 10f/60,
            _ => -1
        };
    }
    public static float? FuelSuccessBonus(string name) => LootBalancePolicy.ItemName(name) switch
    {
        "GEAR_Firelog" => 50,
        "GEAR_ReclaimedWoodB" => 0,
        _ => null
    };
}
