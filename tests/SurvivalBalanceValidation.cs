extern alias GameAssembly;
using Game=GameAssembly::Il2Cpp;
using Gear=GameAssembly::Il2CppTLD.Gear;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
namespace InterloperOverhaul;
internal static class SurvivalBalanceValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var mode=Game.ExperienceModeManager.s_CurrentGameMode;
        var root=new GameObject("IO_SurvivalValidation");root.SetActive(false);
        var items=new List<Game.GearItem>();
        try
        {
            foreach (var name in MementoRewardPolicy.Supplies)
                check(Game.GearItem.LoadGearItemPrefab(name)!=null,"memento supply exists in native assets: "+name);
            var maker=Game.GearItem.InstantiateGearItem("GEAR_NoiseMaker");items.Add(maker);
            var noise=maker.GetComponent<Game.NoiseMakerItem>();
            check(noise.m_AIDamageInflictionInRadius==250 && noise.m_PlayerDamageInflictionInRadius==250 && noise.m_PlayerDamageRadius==1 && noise.m_AIDamageRadius==5,
                "native noisemaker has 250 fixed damage and preserves original player and wildlife blast radii");
            maker.Deserialize(maker.Serialize(),false);
            check(noise.m_AIDamageInflictionInRadius==250 && noise.m_PlayerDamageInflictionInRadius==250,"noisemaker damage survives native deserialization without multiplication stacking");
            var shelter=root.AddComponent<Game.SnowShelter>();
            shelter.m_DailyDecayHP=10;shelter.m_BlizzardHourlyDecayHP=2;
            shelter.m_Bed=root.AddComponent<Game.Bed>();shelter.m_Bed.m_WarmthBonusCelsius=3;
            SurvivalBalance.Shelter(shelter);SurvivalBalance.Shelter(shelter);
            check(shelter.m_DailyDecayHP==5 && shelter.m_BlizzardHourlyDecayHP==1 && shelter.m_Bed.m_WarmthBonusCelsius==5,
                "shelter daily and blizzard decay are halved once and native bed warmth is five degrees");
            var shelterManager=root.AddComponent<Game.SnowShelterManager>();
            shelterManager.m_TemperatureIncreaseCelsius=15;
            SurvivalBalance.Manager(shelterManager);
            check(shelterManager.GetTemperatureIncreaseCelsius()==30,"native snow shelter temperature getter reports thirty degrees");
            var packManager=root.AddComponent<Game.PackManager>();
            MoraleValidationObserver.Active=true;
            NoisemakerMoraleEvent.Active=true;
            try
            {
                packManager.ModifyGroupMorale(null,null,-5,default);
                check(MoraleValidationObserver.Amount==-100,"native morale entry receives a full hundred-percent penalty during a noisemaker event");
                NoisemakerMoraleEvent.Active=false;
                packManager.ModifyGroupMorale(null,null,-5,default);
                check(MoraleValidationObserver.Amount==-5,"other native morale events retain their original penalty");
            }
            finally { MoraleValidationObserver.Active=false;NoisemakerMoraleEvent.Active=false; }
            var manager=Gear.BlueprintManager.Instance;
            SurvivalBalance.Recipes(manager);
            var noisemaker=manager.m_AllBlueprints.ToArray().First(bp=>bp.name=="BLUEPRINT_GEAR_NoiseMaker_B");
            check(noisemaker.CanCraftAtLocation(Game.CraftingLocation.Workbench) && !noisemaker.IsLocked(false),"native survival noisemaker recipe is unlocked and accepts an ordinary workbench");
            var mementos=new GameObject("Mementos");mementos.transform.SetParent(root.transform);
            var cache=new GameObject("Cache");cache.transform.SetParent(mementos.transform);
            var box=cache.AddComponent<Game.Container>();
            box.m_GearToInstantiate=new Il2CppSystem.Collections.Generic.List<GameObject>();
            box.m_GearToInstantiate.Add(Game.GearItem.LoadGearItemPrefab("GEAR_Revolver").gameObject);
            box.m_GearToInstantiate.Add(Game.GearItem.LoadGearItemPrefab("GEAR_HeavyParka").gameObject);
            LootBalance.FilterPending(box,0);
            var choices=box.m_GearToInstantiate.ToArray().Select(x=>LootBalancePolicy.ItemName(x.name)).ToArray();
            check(choices.Length==2 && choices.All(x=>MementoRewardPolicy.Supplies.Contains(x)),"memento firearm and premium clothing guarantees become useful random supplies");
            LootBalance.FilterPending(box,0);
            check(choices.SequenceEqual(box.m_GearToInstantiate.ToArray().Select(x=>LootBalancePolicy.ItemName(x.name))),"memento selection remains stable without rerolling or modifying saved inventory");
            Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName("Stalker");
            SurvivalBalance.Manager(shelterManager);
            check(shelterManager.GetTemperatureIncreaseCelsius()==15,"native shelter manager restores authored temperature outside Interloper");
            SurvivalBalance.Recipes(manager);
            check(noisemaker.m_RequiredCraftingLocation==Game.CraftingLocation.AmmoWorkbench && !noisemaker.m_IgnoreLockInSurvival,"native survival noisemaker recipe restores authored location and learning rules outside Interloper");
            var ordinary=Game.GearItem.InstantiateGearItem("GEAR_NoiseMaker");items.Add(ordinary);
            check(ordinary.GetComponent<Game.NoiseMakerItem>().m_AIDamageInflictionInRadius==50 && ordinary.GetComponent<Game.NoiseMakerItem>().m_PlayerDamageInflictionInRadius==0,
                "Stalker retains native noisemaker damage");
        }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            foreach(var item in items) if(item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}

// Observe the patched native entry without running combat in a menu fixture.
[HarmonyPatch]
internal static class MoraleValidationObserver
{
    internal static bool Active;
    internal static float Amount;
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.PackManager),"ModifyGroupMorale");
    [HarmonyPriority(Priority.Last)]
    private static bool Prefix(float percent)
    {
        if (!Active) return true;
        Amount=percent;
        return false;
    }
}
