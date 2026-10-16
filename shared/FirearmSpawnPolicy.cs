namespace InterloperOverhaul;

// Authored base-game outdoor firearm and tool positions, verified against the local 2.55 assets.
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

    public static bool IsLimitedTool(string name) => LootBalancePolicy.ItemName(name) is "GEAR_Knife" or "GEAR_Hatchet";
    public static bool IsFiniteItem(string name) => IsStandardFirearm(name) || IsLimitedTool(name);
    public static readonly string[] Items = { "GEAR_Rifle", "GEAR_Revolver", "GEAR_Knife", "GEAR_Hatchet" };

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
        new("CoastalRegion", "Design/Gear/Hatchet/Hatchet_tunnelCorpse/GEAR_Hatchet", "GEAR_Hatchet", -722.093018f, 204.731003f, 1311.561035f),
        new("CoastalRegion", "Design/Gear/Hatchet/Hatchet_waterfrontCorpse/GEAR_Hatchet", "GEAR_Hatchet", 899.536987f, 37.367001f, 120.349998f),
        new("CoastalRegion", "Design/Gear/Knife/Knife_littleIslandCorpse/GEAR_Knife", "GEAR_Knife", -495.656342f, 23.388000f, 406.991180f),
        new("CoastalRegion", "Design/Gear/Hatchet/Hatchet_misanthrops/GEAR_Hatchet", "GEAR_Hatchet", 370.199005f, 57.308998f, 217.710999f),
        new("CoastalRegion", "Design/Gear/Knife/Knife_house/GEAR_Knife", "GEAR_Knife", 945.826660f, 48.359001f, 505.229645f),
        new("CoastalRegion", "Design/Gear/Hatchet/Hatchet_bearcamp/GEAR_Hatchet", "GEAR_Hatchet", -1272.009033f, 128.587997f, 858.948975f),
        new("CoastalRegion", "Design/Gear/Knife/Knife_outlook/GEAR_Knife", "GEAR_Knife", 360.582214f, 204.483902f, 1154.930420f),
        new("CoastalRegion", "Design/Gear/Knife/Knife_fishCabin/GEAR_Knife", "GEAR_Knife", -701.793701f, 28.804000f, 673.102358f),
        new("CoastalRegion", "Design/Gear/Hatchet/Hatchet_fishBoat/GEAR_Hatchet", "GEAR_Hatchet", -654.710999f, 23.517000f, 639.023010f),
        new("CoastalRegion", "Design/Gear/Hatchet/Hatchet_cabin/GEAR_Hatchet", "GEAR_Hatchet", -925.900024f, 71.073997f, 660.419983f),
        new("CoastalRegion", "Design/Gear/Knife/Knife_car/GEAR_Knife", "GEAR_Knife", 686.957520f, 35.212299f, -423.285400f),
        new("CoastalRegion", "Design/Gear/Knife/Knife_fishhut1/GEAR_Knife", "GEAR_Knife", 370.131500f, 24.101999f, 696.442871f),
        new("CoastalRegion", "Design/Gear/Knife/Knife_deadEndRidgeCorpse/GEAR_Knife", "GEAR_Knife", -1108.635376f, 116.119003f, 311.080505f),
        new("CoastalRegion", "Design/Gear/Hatchet/Hatchet_logSort/GEAR_Hatchet", "GEAR_Hatchet", -57.973000f, 25.298000f, 883.270020f),
        new("LakeRegion_SANDBOX", "Design/Gear/Hatchet/Hatchet_outlook/GEAR_Hatchet", "GEAR_Hatchet", 786.579296f, 213.193743f, 969.487391f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_forestCorpse/GEAR_Knife", "GEAR_Knife", 491.165314f, 0.085000f, 316.261597f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_trainCorpse/GEAR_Knife", "GEAR_Knife", 995.274231f, 41.514641f, 642.563721f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_crummie/GEAR_Knife", "GEAR_Knife", 1244.873291f, 39.132507f, 1183.981079f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_outlook/GEAR_Knife", "GEAR_Knife", 787.701541f, 213.450640f, 967.020365f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_riverCorpse/GEAR_Knife", "GEAR_Knife", 1639.031006f, 16.393988f, 1376.362427f),
        new("LakeRegion_SANDBOX", "Design/Gear/Hatchet/Hatchet_bridge/GEAR_Hatchet", "GEAR_Hatchet", 1561.510986f, 16.480000f, 1277.109009f),
        new("LakeRegion_SANDBOX", "Design/Gear/Hatchet/Hatchet_damageCabin/GEAR_Hatchet", "GEAR_Hatchet", 564.994812f, -1.554000f, 264.926910f),
        new("LakeRegion_SANDBOX", "Design/Gear/Hatchet/Hatchet_barn1/GEAR_Hatchet", "GEAR_Hatchet", 146.145126f, 3.616420f, 52.630100f),
        new("LakeRegion_SANDBOX", "Design/Gear/Hatchet/Hatchet_logcamp1/GEAR_Hatchet", "GEAR_Hatchet", 889.775024f, 21.591000f, 1269.051025f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_fishhut/GEAR_Knife", "GEAR_Knife", 1529.151001f, 16.701000f, 224.223999f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_hunter1/GEAR_Knife", "GEAR_Knife", 130.673004f, 12.639000f, 661.716980f),
        new("LakeRegion_SANDBOX", "Design/Gear/Knife/Knife_oldLookout/GEAR_Knife", "GEAR_Knife", 389.028046f, 109.898003f, 932.500610f),
        new("LakeRegion_SANDBOX", "Design/Gear/Hatchet/Hatchet_lakeCabins/GEAR_Hatchet", "GEAR_Hatchet", 1464.300049f, 19.455999f, -63.200001f),
        new("LakeRegion_SANDBOX", "Design/Gear/Hatchet/Hatchet_fishhut/GEAR_Hatchet", "GEAR_Hatchet", 1307.645020f, 16.639000f, 289.959015f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/Hatchet/hatchetGroup2/GEAR_Hatchet", "GEAR_Hatchet", 1479.654053f, 323.621002f, 1531.489990f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/knife/knifeGroup2/GEAR_Knife", "GEAR_Knife", 1321.467041f, 350.710999f, 1588.735962f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/knife/knifeGroup1/GEAR_Knife", "GEAR_Knife", 951.620972f, 311.829010f, 1151.031006f),
        new("MountainTownRegion_SANDBOX", "Design/Gear/Hatchet/hatchetGroup1/GEAR_Hatchet", "GEAR_Hatchet", 1365.673950f, 263.625000f, 1201.352051f),
    };

    public static FirearmSite SelectSite(int seed, string item)
    {
        var sites = Sites.Where(site => site.Item == item).ToArray();
        return sites[(int)(LootBalancePolicy.Sample(seed, "finite-firearm-site/" + item, 0) * sites.Length)];
    }

    public static int Condition(int seed, string item) =>
        1 + (int)(LootBalancePolicy.Sample(seed, "finite-firearm-condition/" + item, 0) * 100);
}
