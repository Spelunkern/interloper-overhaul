using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InterloperOverhaul;

// Stored inside the game's save slot, alongside the same snapshot as scene gear.
public sealed class FirearmRunState
{
    public int Version { get; set; } = 3;
    public List<LightAllocation> Lights { get; set; } = new();
    public bool LightsLegacy { get; set; }
    public int Seed { get; set; }
    public string RunKey { get; set; } = "";
    public string RifleSite { get; set; } = "";
    public string RevolverSite { get; set; } = "";
    public bool RifleCreated { get; set; }
    public bool RevolverCreated { get; set; }
    public bool Legacy { get; set; }
    public string KnifeSite { get; set; } = "";
    public string HatchetSite { get; set; } = "";
    public bool KnifeCreated { get; set; }
    public bool HatchetCreated { get; set; }
    public bool ToolsLegacy { get; set; }

    public static FirearmRunState Create(int seed, string runKey) => new()
    {
        Seed = seed, RunKey = runKey, Lights = LightSpawnPolicy.Allocate(seed),
        RifleSite = FirearmSpawnPolicy.SelectSite(seed, "GEAR_Rifle").Id,
        RevolverSite = FirearmSpawnPolicy.SelectSite(seed, "GEAR_Revolver").Id,
        KnifeSite = FirearmSpawnPolicy.SelectSite(seed, "GEAR_Knife").Id,
        HatchetSite = FirearmSpawnPolicy.SelectSite(seed, "GEAR_Hatchet").Id
    };

    public FirearmSite Site(string item) => FirearmSpawnPolicy.Sites.Single(site => site.Id == (item switch
    {
        "GEAR_Rifle" => RifleSite, "GEAR_Revolver" => RevolverSite,
        "GEAR_Knife" => KnifeSite, "GEAR_Hatchet" => HatchetSite,
        _ => throw new ArgumentException("Unknown finite item: " + item)
    }));
    public bool Created(string item) => item switch
    {
        "GEAR_Rifle" => RifleCreated, "GEAR_Revolver" => RevolverCreated,
        "GEAR_Knife" => KnifeCreated, "GEAR_Hatchet" => HatchetCreated,
        _ => throw new ArgumentException("Unknown finite item: " + item)
    };
    public void SetCreated(string item, bool value)
    {
        switch (item)
        {
            case "GEAR_Rifle": RifleCreated = value; break;
            case "GEAR_Revolver": RevolverCreated = value; break;
            case "GEAR_Knife": KnifeCreated = value; break;
            case "GEAR_Hatchet": HatchetCreated = value; break;
            default: throw new ArgumentException("Unknown finite item: " + item);
        }
    }
    public bool IsLegacy(string item) => Legacy || (FirearmSpawnPolicy.IsLimitedTool(item) && ToolsLegacy);
    public string GuidFor(string item) => new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(
        "InterloperOverhaul/finite-firearms/v1/" + RunKey + "/" + Seed + "/" + item)).Take(16).ToArray()).ToString();
    public string Serialize() => JsonSerializer.Serialize(this);
    public static FirearmRunState Deserialize(string json)
    {
        var state = JsonSerializer.Deserialize<FirearmRunState>(json) ?? throw new InvalidDataException("Missing firearm run state.");
        if (state.Version == 1)
        {
            // Earlier snapshots can already contain tools in unloaded regions.
            // Preserve the firearm allocation without issuing extra tools.
            state.ToolsLegacy = true;
            state.KnifeSite = FirearmSpawnPolicy.SelectSite(state.Seed, "GEAR_Knife").Id;
            state.HatchetSite = FirearmSpawnPolicy.SelectSite(state.Seed, "GEAR_Hatchet").Id;
            state.Version = 2;
        }
        if (state.Version == 2)
        {
            state.LightsLegacy = true;
            state.Lights = LightSpawnPolicy.Allocate(state.Seed);
            state.Version = 3;
        }
        if (state.Version != 3) throw new InvalidDataException("Unsupported finite item run state version.");
        foreach (string item in FirearmSpawnPolicy.Items)
            if (state.Site(item).Item != item) throw new InvalidDataException("Invalid finite item placement plan.");
        if (state.Lights == null || state.Lights.Count != 3 || state.Lights.Select(light => light.Key).Distinct().Count() != 3 ||
            state.Lights.Select(light => light.SiteId).Distinct().Count() != 3 ||
            state.Lights.Count(light => light.Item == "GEAR_Flashlight") != 2 ||
            state.Lights.Count(light => light.Item == "GEAR_Flashlight_LongLasting") != 1)
            throw new InvalidDataException("Invalid flashlight allocation.");
        foreach (var light in state.Lights)
            if (light.Site.Item != light.Item) throw new InvalidDataException("Invalid flashlight site.");
        return state;
    }
}
