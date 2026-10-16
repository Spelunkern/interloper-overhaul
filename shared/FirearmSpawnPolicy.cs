namespace InterloperOverhaul;

// Authored base-game outdoor firearm positions, verified against the local 2.55 assets.
public sealed class FirearmSite
{
    public string Scene { get; }
    public string Path { get; }
    public string Item { get; }
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public string Id => Scene + "/" + Path;

    public FirearmSite(string scene, string path, string item, float x, float y, float z)
    {
        Scene = scene; Path = path; Item = item; X = x; Y = y; Z = z;
    }
}

public static class FirearmSpawnPolicy
{
    public static bool IsStandardFirearm(string name) => LootBalancePolicy.ItemName(name) is "GEAR_Rifle" or "GEAR_Revolver";

    public static readonly FirearmSite[] Sites =
    {
        new("CoastalRegion", "Design/Gear/Weapons/RareRifle/GEAR_Rifle", "GEAR_Rifle", 354.083008f, 203.848999f, 1158.810059f),
        new("CoastalRegion", "Design/Gear/Weapons/Revolver/Rev_lookoutCave/GEAR_Revolver", "GEAR_Revolver", 392.013000f, 148.832001f, 1121.305054f),
        new("CoastalRegion", "Design/Gear/Weapons/Revolver/Rev_islandCorpse/GEAR_Revolver", "GEAR_Revolver", 206.902695f, 26.985100f, 278.861115f),
        new("CoastalRegion", "Design/Gear/Weapons/Revolver/Rev_fishingHut/GEAR_Revolver", "GEAR_Revolver", 82.085999f, 24.096001f, 309.808014f),
        new("CoastalRegion", "Design/Gear/Weapons/Revolver/Rev_car/GEAR_Revolver", "GEAR_Revolver", 419.863586f, 25.336201f, 836.698914f),
        new("CoastalRegion", "Design/Gear/Weapons/Revolver/Rev_campTable/GEAR_Revolver", "GEAR_Revolver", -1201.615967f, 106.024002f, 863.752014f),
        new("LakeRegion_SANDBOX", "Design/Gear/Revolver/Rev_snowshelter/GEAR_Revolver", "GEAR_Revolver", 360.855011f, 103.350998f, 1606.762913f),
        new("LakeRegion_SANDBOX", "Design/Gear/Revolver/Rev_trainCar/GEAR_Revolver", "GEAR_Revolver", 970.670944f, 38.965233f, 856.820103f),
        new("LakeRegion_SANDBOX", "Design/Gear/Revolver/Rev_lakeCorpse/GEAR_Revolver", "GEAR_Revolver", 1587.747849f, 19.731102f, 523.324894f),
        new("LakeRegion_SANDBOX", "Design/Gear/Revolver/Rev_daveCorpse/GEAR_Revolver", "GEAR_Revolver", 947.452988f, 51.507000f, 22.854000f),
        new("LakeRegion_SANDBOX", "Design/Gear/Revolver/Rev_oldLookout/GEAR_Revolver", "GEAR_Revolver", 453.506668f, 158.993622f, 1000.606113f),
        new("MarshRegion_SANDBOX", "Design/Loot/Revolvers_Ammo/RevolverGroup5/GEAR_Revolver", "GEAR_Revolver", 1189.876221f, -133.093033f, 681.777954f),
        new("MarshRegion_SANDBOX", "Design/Loot/Revolvers_Ammo/RevolverGroup3/GEAR_Revolver", "GEAR_Revolver", 444.716003f, -131.673996f, 966.046997f),
        new("MarshRegion_SANDBOX", "Design/Loot/Revolvers_Ammo/RevolverGroup1/GEAR_Revolver", "GEAR_Revolver", 659.895630f, -132.811905f, 27.361738f),
        new("MarshRegion_SANDBOX", "Design/Loot/Revolvers_Ammo/RevolverGroup4/GEAR_Revolver", "GEAR_Revolver", 68.448204f, -106.331001f, 678.429993f),
        new("MarshRegion_SANDBOX", "Design/Loot/Revolvers_Ammo/RevolverGroup2/GEAR_Revolver", "GEAR_Revolver", 576.780945f, -133.244995f, 133.337006f),
        new("MountainTownRegion_SANDBOX", "Design/ToFM/Corpse/BasinCorpseGroup4/GEAR_Rifle", "GEAR_Rifle", 821.689257f, 34.113864f, 988.713303f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/Revolver/revolver1/GEAR_Revolver", "GEAR_Revolver", 1483.122803f, 363.831024f, 2010.735840f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/Revolver/revolver2/GEAR_Revolver", "GEAR_Revolver", 1480.047729f, 323.625000f, 1532.272705f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/Revolver/revolver4/GEAR_Revolver", "GEAR_Revolver", 1155.261169f, 264.437958f, 1722.942993f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/Revolver/revolver5/GEAR_Revolver", "GEAR_Revolver", 1794.737061f, 347.890991f, 2067.270996f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/Revolver/revolver3/GEAR_Revolver", "GEAR_Revolver", 966.737976f, 304.122009f, 1237.822998f),
        new("RuralRegion_SANDBOX", "Design/Gear/Rifle/Rifle1/GEAR_Rifle", "GEAR_Rifle", 744.496399f, 117.074722f, 1564.796021f),
        new("RuralRegion_SANDBOX", "Design/Gear/Rifle/Rifle5/GEAR_Rifle", "GEAR_Rifle", 1235.831055f, 73.414001f, 873.440002f),
        new("RuralRegion_SANDBOX", "Design/Gear/Rifle/Rifle4/GEAR_Rifle", "GEAR_Rifle", 475.070007f, 60.432999f, 587.000977f),
        new("RuralRegion_SANDBOX", "Design/Gear/Rifle/Rifle3/GEAR_Rifle", "GEAR_Rifle", 2365.236084f, 169.742844f, 253.167984f),
        new("RuralRegion_SANDBOX", "Design/Gear/Rifle/Rifle2/GEAR_Rifle", "GEAR_Rifle", 1945.954712f, 45.519508f, 1249.669067f),
        new("RuralRegion_SANDBOX", "Design/Gear/Revolver/GEAR_Revolver", "GEAR_Revolver", 2513.371094f, 185.882004f, 564.611023f),
    };

    public static FirearmSite SelectSite(int seed, string item)
    {
        var sites = Sites.Where(site => site.Item == item).ToArray();
        return sites[(int)(LootBalancePolicy.Sample(seed, "finite-firearm-site/" + item, 0) * sites.Length)];
    }

    public static int Condition(int seed, string item) =>
        1 + (int)(LootBalancePolicy.Sample(seed, "finite-firearm-condition/" + item, 0) * 100);
}
