using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InterloperOverhaul;

// Stored inside the game's save slot, alongside the same snapshot as scene gear.
public sealed class FirearmRunState
{
    public int Version { get; set; } = 1;
    public int Seed { get; set; }
    public string RunKey { get; set; } = "";
    public string RifleSite { get; set; } = "";
    public string RevolverSite { get; set; } = "";
    public bool RifleCreated { get; set; }
    public bool RevolverCreated { get; set; }
    public bool Legacy { get; set; }

    public static FirearmRunState Create(int seed, string runKey) => new()
    {
        Seed = seed, RunKey = runKey,
        RifleSite = FirearmSpawnPolicy.SelectSite(seed, "GEAR_Rifle").Id,
        RevolverSite = FirearmSpawnPolicy.SelectSite(seed, "GEAR_Revolver").Id
    };

    public FirearmSite Site(string item) => FirearmSpawnPolicy.Sites.Single(site =>
        site.Id == (item == "GEAR_Rifle" ? RifleSite : RevolverSite));
    public bool Created(string item) => item == "GEAR_Rifle" ? RifleCreated : RevolverCreated;
    public void SetCreated(string item, bool value)
    {
        if (item == "GEAR_Rifle") RifleCreated = value; else RevolverCreated = value;
    }
    public string GuidFor(string item) => new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(
        "InterloperOverhaul/finite-firearms/v1/" + RunKey + "/" + Seed + "/" + item)).Take(16).ToArray()).ToString();
    public string Serialize() => JsonSerializer.Serialize(this);
    public static FirearmRunState Deserialize(string json)
    {
        var state = JsonSerializer.Deserialize<FirearmRunState>(json) ?? throw new InvalidDataException("Missing firearm run state.");
        if (state.Version != 1) throw new InvalidDataException("Unsupported firearm run state version.");
        if (state.Site("GEAR_Rifle").Item != "GEAR_Rifle" || state.Site("GEAR_Revolver").Item != "GEAR_Revolver")
            throw new InvalidDataException("Invalid firearm placement plan.");
        return state;
    }
}
