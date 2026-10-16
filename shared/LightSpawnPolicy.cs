namespace InterloperOverhaul;

public sealed class LightAllocation
{
    public string Key { get; set; } = "";
    public string Item { get; set; } = "";
    public string SiteId { get; set; } = "";
    public bool Created { get; set; }
    public FirearmSite Site => LightSpawnPolicy.Sites.Single(site => site.Id == SiteId);
}

public static class LightSpawnPolicy
{
    public static bool IsLight(string name) => LootBalancePolicy.ItemName(name) is "GEAR_Flashlight" or "GEAR_Flashlight_LongLasting";
    // Verified Survival scene placements from the locally installed 2.55 assets.
    public static readonly FirearmSite[] Sites =
    {
        new("AshCanyonRegion_SANDBOX", "Root/Design/Gear/Tools/GEAR_Flashlight", "GEAR_Flashlight", -630.683838f,303.846222f,766.539246f),
        new("MarshRegion_SANDBOX", "Design/PlaneCrash/GEAR_Flashlight", "GEAR_Flashlight",1598.259033f,-46.874001f,302.636993f),
        new("RiverValleyRegion_SANDBOX", "Design/Gear/LakeCave/LakeCave2/GEAR_Flashlight", "GEAR_Flashlight",1492.994019f,203.326004f,1742.298950f),
        new("CrashMountainRegion", "Design/Loot/SummitCave/GEAR_Flashlight", "GEAR_Flashlight",1021.255005f,491.307007f,1401.380005f),
        new("TracksRegion_SANDBOX", "Design/Ravine/Corpse_Group/ravineGroup3/GEAR_Flashlight", "GEAR_Flashlight",606.099365f,91.848999f,265.331116f),
        new("MineConcentratorBuilding_SANDBOX", "Design /Gear/Watchroom/GEAR_Flashlight_LongLasting", "GEAR_Flashlight_LongLasting",-6.722789f,14.698245f,-8.910429f)
    };
    public static List<LightAllocation> Allocate(int seed)
    {
        var pool = Sites.Where(s => s.Item == "GEAR_Flashlight").OrderBy(s => LootBalancePolicy.Sample(seed,"finite-light/" + s.Id,0)).ThenBy(s => s.Id,StringComparer.Ordinal).Take(2).ToArray();
        return new()
        {
            new() { Key="Flashlight1", Item="GEAR_Flashlight", SiteId=pool[0].Id },
            new() { Key="Flashlight2", Item="GEAR_Flashlight", SiteId=pool[1].Id },
            new() { Key="MinerFlashlight", Item="GEAR_Flashlight_LongLasting", SiteId=Sites.Last().Id }
        };
    }
}
