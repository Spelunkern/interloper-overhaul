namespace InterloperOverhaul;
public static class StreetLootPolicy
{
    public static readonly string[] CommonBooks={ "GEAR_BookA","GEAR_BookB","GEAR_BookC","GEAR_BookD","GEAR_BookE","GEAR_BookF","GEAR_BookG","GEAR_BookH","GEAR_BookI" };
    public static readonly string[] SkillBooks={ "GEAR_BookArchery","GEAR_BookCooking","GEAR_BookMending","GEAR_BookFireStarting","GEAR_BookCarcassHarvesting","GEAR_BookGunsmithing","GEAR_BookRifleFirearm","GEAR_BookRevolverFirearm","GEAR_BookIceFishing" };
    public static bool IsFuelCover(string name)
    {
        name=LootBalancePolicy.ItemName(name);
        return name.StartsWith("Car",StringComparison.Ordinal) && name.Contains("FuelCover",StringComparison.Ordinal);
    }
    public static bool IsMailboxSpawner(string name)=>LootBalancePolicy.ItemName(name)=="SpawnPrefab_mailbox";
    public static string Mail(int seed,string key,LootAccess access=LootAccess.Ordinary)
    {
        double roll=LootBalancePolicy.Sample(seed,"mailbox/"+key,0);
        if(roll<0.75) return LootBalancePolicy.Sample(seed,"mailbox/"+key,1)<0.5 ? "GEAR_Newsprint" : "GEAR_NewsprintRoll";
        var books=roll<0.83 ? CommonBooks : SkillBooks;
        double research=access==LootAccess.Deep ? 0.03 : access==LootAccess.Remote ? 0.02 : 0.01;
        if(roll>=0.83+research) return "";
        return books[(int)(LootBalancePolicy.Sample(seed,"mailbox/"+key,2)*books.Length)];
    }
    public static double FuelLitres(int seed,string key)=>LootBalancePolicy.Sample(seed,"vehicle-fuel/"+key,0)<0.20 ?
        0.10+0.15*LootBalancePolicy.Sample(seed,"vehicle-fuel/"+key,1) : 0;
    public static bool IsMailSupply(string name)=>ItemBalancePolicy.IsBook(name) || LootBalancePolicy.ItemName(name) is "GEAR_Newsprint" or "GEAR_NewsprintRoll";
}
