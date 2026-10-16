namespace InterloperOverhaul;

// Retention per newly generated restored item, relative to the restoration baseline.
public static class LootBalancePolicy
{
    public static string ItemName(string name)
    {
        name = name.Replace("(Clone)", "").Split(" (")[0].Trim();
        return name.StartsWith("GearItemData_", System.StringComparison.Ordinal) ?
            "GEAR_" + name.Substring("GearItemData_".Length) : name;
    }

    public static bool IsLiveFirearmAmmo(string name) => ItemName(name) is
        "GEAR_RifleAmmoSingle" or "GEAR_RifleAmmoBox" or "GEAR_RevolverAmmoSingle" or "GEAR_RevolverAmmoBox";

    public static bool IsWeaponVariant(string name) => ItemName(name) is
        "GEAR_Rifle_Barbs" or "GEAR_Rifle_Curators" or "GEAR_Rifle_Vaughns" or "GEAR_Rifle_Trader" or
        "GEAR_RevolverGreen" or "GEAR_RevolverFancy" or "GEAR_RevolverStubNosed" or
        "GEAR_Bow_Woodwrights" or "GEAR_Bow_Manufactured" or "GEAR_SurvivalKnife";

    // Authored 2.55 gear-data GUIDs. Read references in Trader filters without
    // synchronously loading assets while the native exchange catalog is loading.
    public static string TraderItem(string guid) => guid switch
    {
        "0697541dfa8cd2644931091463697794" => "GEAR_Hatchet",
        "5a7d6ed7004a60c49878b62497f2a63f" => "GEAR_Knife",
        "b9474d5e3b25bbe4e8c2f3772da8cdab" => "GEAR_Rifle",
        "406b6c4d772d92e4c8c2458e474f7856" => "GEAR_Revolver",
        "6f4b8ace7b43c3346ab5f8955fd6177d" => "GEAR_RifleAmmoSingle",
        "b077bbe3815612345a39da9a3518c642" => "GEAR_RifleAmmoBox",
        "e5ef5c87193d18f45b64961da0adc56d" => "GEAR_RifleAmmoCasing",
        "ab8fccd8c80c0c442aeeb244d6cf396b" => "GEAR_RevolverAmmoSingle",
        "e1e4e7d1932d79b46aab8b9034f04c17" => "GEAR_RevolverAmmoBox",
        "1096a8e89e17d7646867f1caaa751cb0" => "GEAR_RevolverAmmoCasing",
        "2e03fa64f3ab1d141a584daf81e358f5" => "GEAR_SurvivalKnife",
        "96daa7434362e754fb7bd4fbb24c8cd4" => "GEAR_Bow_Manufactured",
        "4c5b21d92f282da4594dc59dd73a9f4a" => "GEAR_Bow_Woodwrights",
        "9554b92c571828948b07f11703c50dea" => "GEAR_RevolverFancy",
        "fc0ad0fa37b22274cb0a9964f5dcbf4b" => "GEAR_RevolverGreen",
        "1a5913c327c618046bdebe220935e903" => "GEAR_RevolverStubNosed",
        "28e60db238cbfb549a7a1da9498f65f1" => "GEAR_Rifle_Barbs",
        "118e9d9493692c44ba7a0acb6133fc21" => "GEAR_Rifle_Curators",
        "b930bfe9aa379ec4885009a692cfea2e" => "GEAR_Rifle_Trader",
        "32ed5798a3d3189429b3d08ef20eec63" => "GEAR_Rifle_Vaughns",
        _ => ""
    };

    public static bool KeepTraderSupply(int seed, string name, int aurora) =>
        !IsWeaponVariant(name) && Sample(seed, "trader-firearm-supplies/" + ItemName(name), aurora) <
        (IsLiveFirearmAmmo(name) ? 0.0025 : WorldPolicy.IsSpentCasing(name) ? 0.005 : 1);

    public static double Retention(string name, bool restored)
    {
        name = ItemName(name);
        if (FirearmSpawnPolicy.IsFiniteItem(name)) return 0; // Supplied only by the finite world allocation.
        if (IsWeaponVariant(name)) return 0; // Temporarily disabled, regardless of authored difficulty restrictions.
        if (IsLiveFirearmAmmo(name)) return 0.0025;
        if (WorldPolicy.IsSpentCasing(name)) return 0.005;
        if (!restored || WorldPolicy.IsTaleReward(name)) return 1;
        return name switch
        {
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
