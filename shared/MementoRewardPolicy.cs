#nullable disable
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
    public static readonly string[] ExpeditionSupplies =
    {
        "GEAR_LeatherDried", "GEAR_ScrapMetal", "GEAR_SewingKit", "GEAR_SharpeningStone",
        "GEAR_HeavyBandage", "GEAR_WoodMatches", "GEAR_StumpRemover", "GEAR_DustingSulfur"
    };
    public static readonly string[] RareEquipment =
    {
        "GEAR_Firestriker", "GEAR_HighQualityTools", "GEAR_Gauntlets", "GEAR_LongUnderwearWool"
    };
    public static string Choose(int seed,string cache,int slot,LootAccess access=LootAccess.Ordinary)
    {
        double roll=LootBalancePolicy.Sample(seed,"memento-value/"+cache,slot);
        // At most the first slot can roll premium gear; ordinary and remote caches
        // never bypass the deep-exploration restriction through replacements.
        var pool=access==LootAccess.Deep && slot==0 && roll<0.01 ? RareEquipment :
            access is LootAccess.Deep or LootAccess.Remote && roll<0.70 ? ExpeditionSupplies : Supplies;
        return pool[(int)(LootBalancePolicy.Sample(seed,"memento-supplies/"+cache,slot)*pool.Length)];
    }
}
