extern alias GameAssembly;
using System.Reflection;
using System.Text.Json.Nodes;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
using UnityEngine;
namespace InterloperOverhaul;
internal static class AfflictionValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var mode=Game.ExperienceModeManager.s_CurrentGameMode;
        var oldPlayer=Game.GameManager.m_PlayerManager;
        var oldCondition=Game.GameManager.m_Condition;
        var oldFrost=Game.GameManager.m_Frostbite;
        var oldParasites=Game.GameManager.m_IntestinalParasites;
        var oldRest=Game.GameManager.m_Rest;
        var oldPass=Game.GameManager.m_PassTime;
        var root=new GameObject("IO_AfflictionValidation");root.SetActive(false);
        var items=new List<Game.GearItem>();
        string phase="frostbite initialization";
        bool Close(float a,float b)=>Math.Abs(a-b)<0.0001f;
        try
        {
            Game.GameManager.m_PlayerManager=root.AddComponent<Game.PlayerManager>();
            var condition=root.AddComponent<Game.Condition>();condition.m_MaxHP=100;condition.m_CurrentHP=100;
            Game.GameManager.m_Condition=condition;
            var frost=root.AddComponent<Game.Frostbite>();Game.GameManager.m_Frostbite=frost;
            frost.m_StartHasBeenCalled=true;frost.m_MaxHPList=Enumerable.Repeat(100f,12).ToArray();
            frost.m_LocationsCurrentFrostbiteDamage=new Il2CppSystem.Collections.Generic.List<float>();
            for(int i=0;i<12;i++) frost.m_LocationsCurrentFrostbiteDamage.Add(0);
            frost.m_MaxPlayerHPReductionPerInstance=10;
            frost.FrostbiteStart(0,false,true);
            check(frost.NumInstancesFrostbite()==1 && frost.GetTotalHitPointReduction()==0 && frost.m_LocationsCurrentFrostbiteDamage[0]==100,
                "native frostbite activation fills its gauge without permanently reducing maximum condition");
            frost.FrostbiteStart(0,false,true);
            frost.FrostbiteStart(3,false,true);
            check(frost.NumInstancesFrostbite()==2 && frost.NumInstancesFrostbiteAtLocation(0)==1,"frostbite stacks across body areas but cannot duplicate on one area");
            FrostbiteDamageObserver.Active=true;FrostbiteDamageObserver.Damage=0;
            var tick=FrostbiteBalance.Begin(frost,1);
            FrostbiteBalance.End(tick);
            check(Close(FrostbiteDamageObserver.Damage,20),"two frostbitten areas inflict twenty condition points per game hour");
            phase="frostbite recovery and persistence";
            frost.DealFrostbiteDamageToLocation(50,Game.AfflictionBodyArea.Head);
            check(frost.m_LocationsCurrentFrostbiteDamage[0]==100,"continued cold cannot overflow an active frostbite gauge");
            frost.DealFrostbiteDamageToLocation(-40,Game.AfflictionBodyArea.Head);
            check(frost.HasFrostbite() && frost.m_LocationsCurrentFrostbiteDamage[0]==60 && frost.m_LocationsWithFrostbiteRisk.Contains(0),
                "native warming reduces an active area's visible gauge while its affliction remains");
            string save=frost.Serialize();
            frost.Deserialize(save);
            check(frost.NumInstancesFrostbite()==2 && frost.m_LocationsCurrentFrostbiteDamage[0]==60 && frost.GetTotalHitPointReduction()==0,
                "native frostbite round trip preserves separate areas and partial recovery without restoring permanent damage");
            for(int reload=0;reload<3;reload++) frost.Deserialize(frost.Serialize());
            check(frost.m_LocationsCurrentFrostbiteDamage[0]==60,"repeated frostbite reloads do not refill recovering gauges");
            FrostbiteBalance.Current=FrostbiteBalance.Begin(frost,1);
            FrostbiteDamageObserver.Damage=0;
            frost.DealFrostbiteDamageToLocation(-120,Game.AfflictionBodyArea.Head);
            FrostbiteBalance.End(FrostbiteBalance.Current);FrostbiteBalance.Current=null;
            check(frost.NumInstancesFrostbite()==1 && !frost.m_LocationsWithFrostbiteRisk.Contains(0) && Close(FrostbiteDamageObserver.Damage,15),
                "one area's cure clears only its affliction and prorates damage when recovery finishes during a large time step");
            frost.DealFrostbiteDamageToLocation(-1000,Game.AfflictionBodyArea.HandLeft);
            check(!frost.HasFrostbite() && frost.GetTotalHitPointReduction()==0,"warming all affected areas completely cures frostbite");
            var legacy=JsonNode.Parse(save);legacy.AsObject().Remove(FrostbiteBalance.Marker);
            frost.Deserialize(legacy.ToJsonString());
            check(frost.NumInstancesFrostbite()==2 && frost.m_LocationsCurrentFrostbiteDamage[0]==100 && frost.GetTotalHitPointReduction()==0,
                "legacy permanent frostbite migrates to recoverable full gauges without free condition healing");
            check(frost.m_HealPerHourBase>=400 && frost.m_MaxHealPerHour>=400,"warm frostbite meters can recover fully within fifteen game minutes");
            phase="parasite treatment";
            var parasites=root.AddComponent<Game.IntestinalParasites>();Game.GameManager.m_IntestinalParasites=parasites;
            Game.GameManager.m_Rest=root.AddComponent<Game.Rest>();
            Game.GameManager.m_PassTime=root.AddComponent<Game.PassTime>();
            parasites.IntestinalParasitesStart(true);
            check(parasites.HasIntestinalParasites() && Close(parasites.GetTotalHitPointReduction(),20) && Close(parasites.GetTotalFatigueReduction(),20),
                "native parasite infection immediately applies both fixed twenty-percent penalties");
            parasites.UpdateIntestinalParasites();parasites.UpdateIntestinalParasites();
            check(parasites.m_ParasitesElapsedHours==24 && Close(parasites.GetTotalHitPointReduction(),20) && parasites.GetNumDosesRequired()==1,
                "parasite penalties do not escalate and one native treatment dose is required");
            ParasiteBalance.AddRest(parasites,24);
            check(parasites.m_HasParasites && ParasiteBalance.RestHours(parasites)==0,"rest before medication cannot satisfy parasite treatment");
            parasites.TakeAntibiotics();
            check(parasites.GetNumDosesTaken()==1 && parasites.GetNumDosesRemaining()==0 && parasites.m_HasParasites,
                "one native antibiotic or reishi treatment completes medication without curing parasites before rest");
            parasites.TakeAntibiotics();
            check(parasites.GetNumDosesTaken()==1,"parasite treatment cannot accumulate additional doses");
            ParasiteBalance.AddRest(parasites,12);
            string treated=parasites.Serialize();parasites.Deserialize(treated);
            check(parasites.m_HasParasites && ParasiteBalance.RestHours(parasites)==12 && parasites.GetNumDosesTaken()==1 && parasites.m_ParasitesElapsedHours==24,
                "native parasite save preserves medication, twelve hours of recovery, and fixed penalties");
            ParasiteBalance.AddRest(parasites,11.99f);
            check(parasites.m_HasParasites,"parasites remain active until all twenty-four rest hours are completed");
            ParasiteBalance.AddRest(parasites,0.01f);
            check(!parasites.m_HasParasites && parasites.GetTotalHitPointReduction()==0 && parasites.GetTotalFatigueReduction()==0,
                "one treatment plus twenty-four rest hours cures parasites and removes both penalties");
            phase="cabin fever and lacerations";
            var fever=root.AddComponent<Game.CabinFever>();
            fever.m_Active=true;fever.m_RiskActive=true;fever.m_IndoorTimeTracked.Add(24);
            string feverSave=fever.Serialize();fever.Deserialize(feverSave);
            check(!fever.HasCabinFever() && !fever.HasCabinFeverRisk() && fever.m_IndoorTimeTracked.Count==0,"loading an existing cabin-fever affliction or risk clears it");
            fever.CabinFeverStart(false,true);fever.Update();
            check(!fever.HasCabinFever() && fever.DisabledForGameMode(mode),"native cabin-fever activation and subsequent updates cannot generate the affliction");
            var lacerations=root.AddComponent<Game.SevereLacerations>();
            lacerations.Deserialize(new Game.SevereLacerationsSaveData { m_IsActive=true,m_CauseLocID="GAMEPLAY_Cougar" });
            check(!lacerations.HasAffliction(),"existing severe lacerations lose their recurring seventy-two-hour syndrome on load");
            lacerations.m_CurrentState.m_IsActive=true;lacerations.Update();
            check(!lacerations.HasAffliction(),"severe-laceration updates cannot create recurring wounds or infection risks");
            phase="prybar defense";
            var prefab=Game.GearItem.LoadGearItemPrefab("GEAR_Prybar");
            var original=prefab.GetComponent<Game.StruggleBonus>();
            var prybar=Game.GearItem.InstantiateGearItem("GEAR_Prybar");items.Add(prybar);
            var bonus=prybar.GetComponent<Game.StruggleBonus>();
            check(Close(bonus.m_DamageScalePercent,original.m_DamageScalePercent*2) && Close(bonus.m_FleeChanceScale,original.m_FleeChanceScale*2) && !bonus.m_CanPuncture && Close(bonus.m_TapIncrementScale,original.m_TapIncrementScale),
                "prybar doubles native damage and flee multipliers without adding punctures or changing tap speed");
            prybar.Deserialize(prybar.Serialize(),false);PrybarBalance.Configure(prybar);
            check(Close(bonus.m_DamageScalePercent,2.5f) && Close(bonus.m_FleeChanceScale,3),"prybar save reload and repeated initialization do not stack its bonuses");
            var struggle=root.AddComponent<Game.PlayerStruggle>();struggle.m_WeaponUsed=prybar;struggle.m_FleeChanceOnHit=10;
            check(Close(struggle.GetFleeChanceOnHitPercent(),30),"native struggle flee calculation consumes the improved prybar multiplier");
            phase="difficulty isolation";
            foreach(string difficulty in new[] { "Stalker","Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                var vanillaFrost=root.AddComponent<Game.Frostbite>();vanillaFrost.m_MaxPlayerHPReductionPerInstance=10;
                FrostbiteBalance.Configure(vanillaFrost);
                var vanillaParasites=root.AddComponent<Game.IntestinalParasites>();vanillaParasites.m_HasParasites=true;vanillaParasites.m_ParasitesElapsedHours=48;vanillaParasites.m_MaxHitPointReductionPerDay=2;
                ParasiteBalance.Configure(vanillaParasites);
                var vanillaPrybar=Game.GearItem.InstantiateGearItem("GEAR_Prybar");items.Add(vanillaPrybar);
                check(vanillaFrost.m_MaxPlayerHPReductionPerInstance==10 && vanillaParasites.m_ParasitesElapsedHours==48 && vanillaParasites.GetTotalHitPointReduction()==4 && vanillaParasites.GetNumDosesRequired()>1 &&
                    Close(vanillaPrybar.GetComponent<Game.StruggleBonus>().m_DamageScalePercent,1.25f),difficulty+" preserves native frostbite, escalating parasites, and prybar damage");
            }
        }
        catch(Exception ex) { throw new InvalidOperationException("Affliction validation phase: "+phase,ex); }
        finally
        {
            FrostbiteDamageObserver.Active=false;FrostbiteBalance.Current=null;
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            Game.GameManager.m_PlayerManager=oldPlayer;Game.GameManager.m_Condition=oldCondition;Game.GameManager.m_Frostbite=oldFrost;
            Game.GameManager.m_IntestinalParasites=oldParasites;Game.GameManager.m_Rest=oldRest;Game.GameManager.m_PassTime=oldPass;
            foreach(var item in items) if(item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
// Observe actual condition-damage calls without running death, HUD, or camera logic in a menu fixture.
[HarmonyPatch]
internal static class FrostbiteDamageObserver
{
    internal static bool Active;
    internal static float Damage;
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Condition),"AddHealth",new[] { typeof(float),typeof(Game.DamageSource) });
    [HarmonyPriority(Priority.Last)]
    private static bool Prefix(float hp,Game.DamageSource cause)
    {
        if (!Active || cause!=Game.DamageSource.FrostBite) return true;
        Damage-=hp;return false;
    }
}
