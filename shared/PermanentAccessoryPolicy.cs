using System.Text.Json;
namespace InterloperOverhaul;

public sealed class PermanentAccessoryState
{
    public int Version { get; set; } = 1;
    public bool ToolBelt { get; set; }
    public bool RifleHolster { get; set; }
    public string Serialize() => JsonSerializer.Serialize(this);
    public static PermanentAccessoryState Deserialize(string json)
    {
        var state = JsonSerializer.Deserialize<PermanentAccessoryState>(json) ?? throw new InvalidDataException("Missing permanent accessory state.");
        if (state.Version != 1) throw new InvalidDataException("Unsupported permanent accessory state.");
        return state;
    }
}

public static class PermanentAccessoryPolicy
{
    public static bool IsCollectible(string name) => LootBalancePolicy.ItemName(name) is "GEAR_ToolBelt" or "GEAR_RifleScabbardA";
    public static bool IsRifle(string name)
    {
        name = LootBalancePolicy.ItemName(name);
        return name == "GEAR_Rifle" || name.StartsWith("GEAR_Rifle_", StringComparison.Ordinal);
    }
    public static bool IsWeapon(string name)
    {
        name = LootBalancePolicy.ItemName(name);
        return IsRifle(name) || name.StartsWith("GEAR_Revolver",StringComparison.Ordinal) || name == "GEAR_Bow" ||
            name.StartsWith("GEAR_Bow_",StringComparison.Ordinal) || name.StartsWith("GEAR_BearSpear",StringComparison.Ordinal) ||
            name is "GEAR_FlareGun" or "GEAR_NoiseMaker" or "GEAR_Stone" or "GEAR_Bullet";
    }
    // Variants and improvised versions share the functional tool family.
    public static string ToolFamily(string name)
    {
        name = LootBalancePolicy.ItemName(name);
        if (IsWeapon(name)) return "";
        return name switch
        {
            "GEAR_Knife" or "GEAR_KnifeImprovised" or "GEAR_SurvivalKnife" or "GEAR_CougarClawKnife" => "Knife",
            "GEAR_Hatchet" or "GEAR_HatchetImprovised" => "Hatchet",
            "GEAR_SimpleTools" or "GEAR_HighQualityTools" => "Tools",
            "GEAR_KeroseneLampB" or "GEAR_KeroseneLamp_Spelunkers" => "Lantern",
            "GEAR_Flashlight" or "GEAR_Flashlight_LongLasting" => "Flashlight",
            "GEAR_SprayPaintCan" or "GEAR_SprayPaintCanGlyphA" => "SprayPaint",
            _ => name
        };
    }
}
