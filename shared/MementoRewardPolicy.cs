namespace InterloperOverhaul;

// Only physical memento caches; Tales radio caches and unique rewards keep their own policy.
public static class MementoRewardPolicy
{
    public static bool IsCache(string path) => path.Split('/').Any(part =>
        part.Equals("Mementos",StringComparison.OrdinalIgnoreCase) || part.Equals("Memento",StringComparison.OrdinalIgnoreCase));
    public static readonly string[] Supplies =
    {
        "GEAR_EnergyBar", "GEAR_BeefJerky", "GEAR_DogFood", "GEAR_PinnacleCanPeaches",
        "GEAR_TomatoSoupCan", "GEAR_CondensedMilk", "GEAR_Cloth", "GEAR_LeatherDried",
        "GEAR_ScrapMetal", "GEAR_SewingKit", "GEAR_SharpeningStone", "GEAR_HeavyBandage",
        "GEAR_BottleHydrogenPeroxide", "GEAR_WaterPurificationTablets", "GEAR_WoodMatches",
        "GEAR_StumpRemover", "GEAR_DustingSulfur", "GEAR_HookAndLine"
    };
    public static string Choose(int seed,string cache,int slot) => Supplies[(int)(
        LootBalancePolicy.Sample(seed,"memento-supplies/"+cache,slot)*Supplies.Length)];
}
