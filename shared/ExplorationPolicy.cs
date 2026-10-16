#nullable disable
namespace InterloperOverhaul;

public enum LootAccess { Ordinary, Remote, Deep, Beach }

// Audited destinations, not whole outdoor regions. Native scene names are exact.
public static class ExplorationPolicy
{
    public static LootAccess Access(string scene,string path,float x=0,float y=0,float z=0,bool beach=false)
    {
        if (beach || WorldPolicy.IsBeachcombingObject(path)) return LootAccess.Beach;
        if (WorldPolicy.IsTaleBunker(scene) || scene=="AshMine") return LootAccess.Deep;
        if (scene=="CrashMountainRegion" && (Under(path,"Design/Loot/PlaneTailStuff") || Under(path,"Design/Loot/SummitCave") ||
            Near(x,y,z,1021.255f,491.307f,1401.380f,35) || Near(x,y,z,934,470,1168,65))) return LootAccess.Deep;
        if (scene=="RiverValleyRegion_SANDBOX" && (
            Under(path,"Design/Mementos/HRV1/CONTAINER_LockBoxB") ||
            Under(path,"Design/Mementos/HRV2/GEAR_RockCache_Prefab") ||
            Near(x,y,z,957.507f,194.697f,1279.637f,45) || Near(x,y,z,1175.754f,194.900f,787.663f,45) ||
            Near(x,y,z,1492.994f,203.326f,1742.299f,35))) return LootAccess.Deep;
        if (scene=="MineConcentratorBuilding_SANDBOX" && Under(path,"Design/Gear/Watchroom")) return LootAccess.Deep;
        if (WorldPolicy.IsRadioCache(path) || MementoRewardPolicy.IsCache(path)) return LootAccess.Remote;
        if (scene=="CoastalRegion" && Near(x,y,z,354.083f,203.849f,1158.810f,45)) return LootAccess.Remote;
        if (scene=="LakeRegion_SANDBOX" && Near(x,y,z,389.028f,109.898f,932.501f,45)) return LootAccess.Remote;
        return LootAccess.Ordinary;
    }

    public static bool Under(string path,string root)
    {
        path=string.Join("/",path.Split('/').Select(part=>part.Trim()));
        if(path.StartsWith("Root/",StringComparison.Ordinal)) path=path.Substring(5);
        return path==root || path.StartsWith(root+"/",StringComparison.Ordinal);
    }
    private static bool Near(float x,float y,float z,float a,float b,float c,float radius)=>
        (x-a)*(x-a)+(y-b)*(y-b)+(z-c)*(z-c)<=radius*radius;

    public static bool IsProgressionReward(string item,string scene,string path)=>
        WorldPolicy.IsTaleReward(item) && WorldPolicy.IsTaleBunker(scene) && Under(path,"Design");

    public static bool IsPremium(string item)=>LootBalancePolicy.ItemName(item) is
        "GEAR_Balaclava" or "GEAR_HeavyParka" or "GEAR_MilitaryParka" or "GEAR_MuklukBoots" or
        "GEAR_JacketLeatherFlightA" or "GEAR_InsulatedBoots" or "GEAR_SkiBoots" or "GEAR_InsulatedPants" or
        "GEAR_CombatPants" or "GEAR_ClimbingSocks" or "GEAR_LongUnderwearWool" or "GEAR_Gauntlets" or
        "GEAR_CowichanSweater" or "GEAR_FishermanSweater" or "GEAR_WoolShirt" or "GEAR_HeavyWoolSweater" or
        "GEAR_DownParka" or "GEAR_QualityWinterCoat" or "GEAR_PremiumWinterCoat";

    public static bool IsUtility(string item)=>LootBalancePolicy.ItemName(item) is
        "GEAR_Hammer" or "GEAR_Hacksaw" or "GEAR_MagnifyingLens" or "GEAR_Firestriker" or
        "GEAR_HighQualityTools" or "GEAR_SimpleTools" or "GEAR_FlareGun" or "GEAR_Crampons" or
        "GEAR_KeroseneLampB" or "GEAR_KeroseneLamp_Spelunkers" or "GEAR_Prybar" or "GEAR_NoiseMaker";

    public static double EquipmentRetention(string item,LootAccess access,bool equipment)
    {
        if (IsPremium(item)) return access==LootAccess.Deep ? 0.01 : 0;
        if (equipment || IsUtility(item)) return access==LootAccess.Deep ? 0.05 : access==LootAccess.Remote ? 0.01 : 0;
        return 0.05;
    }
}
