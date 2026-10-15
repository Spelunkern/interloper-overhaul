namespace InterloperOverhaul;

// Retention per newly generated restored item, relative to the restoration baseline.
public static class LootBalancePolicy
{
    public static string ItemName(string name) => name.Replace("(Clone)", "").Split(" (")[0].Trim();

    public static double Retention(string name, bool restored)
    {
        name = ItemName(name);
        if (!restored || WorldPolicy.IsTaleReward(name) || WorldPolicy.IsSpentCasing(name)) return 1;
        return name switch
        {
            "GEAR_Rifle_Barbs" or "GEAR_Rifle_Curators" or "GEAR_Rifle_Vaughns" or "GEAR_Rifle_Trader" or
            "GEAR_RevolverGreen" or "GEAR_RevolverFancy" or "GEAR_RevolverStubNosed" or
            "GEAR_Bow_Woodwrights" or "GEAR_Bow_Manufactured" or "GEAR_SurvivalKnife" => 0.0025,
            "GEAR_Rifle" or "GEAR_Revolver" or "GEAR_Knife" or "GEAR_Hatchet" or
            "GEAR_Hammer" or "GEAR_Hacksaw" or "GEAR_MagnifyingLens" or "GEAR_Firestriker" or
            "GEAR_Balaclava" or "GEAR_HeavyParka" or "GEAR_MilitaryParka" or "GEAR_MuklukBoots" or
            "GEAR_JacketLeatherFlightA" or "GEAR_InsulatedBoots" or "GEAR_SkiBoots" or
            "GEAR_InsulatedPants" or "GEAR_CombatPants" or "GEAR_ClimbingSocks" or
            "GEAR_LongUnderwearWool" or "GEAR_Gauntlets" or "GEAR_CowichanSweater" or
            "GEAR_FishermanSweater" or "GEAR_WoolShirt" or "GEAR_HeavyWoolSweater" or
            "GEAR_DownParka" or "GEAR_QualityWinterCoat" or "GEAR_PremiumWinterCoat" => 0.01,
            _ => 0.05
        };
    }

    // Stable across processes/platforms. No string.GetHashCode or shared RNG changes.
    public static double Sample(int seed, string key, int slot)
    {
        unchecked
        {
            uint hash = 2166136261u ^ (uint)seed;
            foreach (char ch in key) hash = (hash ^ ch) * 16777619u;
            hash = (hash ^ (uint)slot) * 16777619u;
            hash ^= hash >> 16;
            hash *= 0x7feb352du;
            hash ^= hash >> 15;
            hash *= 0x846ca68bu;
            hash ^= hash >> 16;
            return hash / 4294967296.0;
        }
    }
}
