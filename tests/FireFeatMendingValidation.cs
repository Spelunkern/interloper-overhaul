extern alias GameAssembly;
using System.Text.Json.Nodes;
using HarmonyLib;
using System.Reflection;
using Gear = GameAssembly::Il2CppTLD.Gear;
using Game = GameAssembly::Il2Cpp;
using AI = GameAssembly::Il2CppTLD.AI;
using Units = Il2CppTLD.IntBackedUnit;
using UnityEngine;

namespace InterloperOverhaul;
internal static class FireFeatMendingValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var mode=Game.ExperienceModeManager.s_CurrentGameMode;
        var oldSkills=Game.GameManager.m_SkillsManager;
        var oldTinder=FireBalance.SelectedTinder;
        var enabled=Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.ToArray();
        var root=new GameObject("IO_FireFeatMendingValidation");
        root.SetActive(false);
        var items=new List<Game.GearItem>();
        string phase="fire materials";
        bool Close(float a,float b) => Math.Abs(a-b)<0.0001;
        Game.GearItem Create(string name)
        {
            var gear=Game.GearItem.InstantiateGearItem(name);
            if (gear==null) throw new InvalidOperationException("Missing prefab: "+name);
            items.Add(gear);
            gear.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
            gear.m_HasBeenOwnedByPlayer=true;
            gear.m_LastUpdatedTODHoursInitialized=true;
            gear.ManualStart();
            return gear;
        }
        try
        {
            foreach (var entry in new Dictionary<string,float> { ["GEAR_Newsprint"]=0.05f,["GEAR_NewsprintRoll"]=0.05f,
                ["GEAR_PaperStack"]=0.05f,["GEAR_CashBundle"]=0.01f,["GEAR_Firelog"]=1,["GEAR_Coal"]=0.2f,
                ["GEAR_ReclaimedWoodB"]=0.2f,["GEAR_Stick"]=0.1f })
            {
                var gear=Create(entry.Key);
                check(Close(gear.GetSingleItemWeightKG()/Units.ItemWeight.FromKilograms(1),entry.Value),"native fire material weight: "+entry.Key);
                gear.Deserialize(gear.Serialize(),false);
                check(Close(gear.GetSingleItemWeightKG()/Units.ItemWeight.FromKilograms(1),entry.Value),"fire material weight survives native saves: "+entry.Key);
            }
            foreach (string name in new[] { "GEAR_Newsprint","GEAR_NewsprintRoll","GEAR_PaperStack","GEAR_Tinder","GEAR_BarkTinder","GEAR_CattailTinder","GEAR_CashBundle" })
            {
                var gear=Create(name);
                check(gear.m_FuelSourceItem.m_IsTinder && gear.m_FuelSourceItem.m_FireStartSkillModifier==FireBalancePolicy.TinderBonus(name),"tinder has the requested success bonus: "+name);
            }
            foreach (string name in new[] { "GEAR_BookA","GEAR_BookBopen","GEAR_BookLabE_Open_01","GEAR_BookFireStarting" })
                check(Close(Create(name).m_FuelSourceItem.GetModifiedBurnDurationHours(1),0.5f),"native book fuel lasts thirty base minutes: "+name);
            var log=items.First(i=>LootBalancePolicy.ItemName(i.name)=="GEAR_Firelog");
            var stick=items.First(i=>LootBalancePolicy.ItemName(i.name)=="GEAR_Stick");
            check(log.m_FuelSourceItem.m_FireStartSkillModifier==50 && log.m_FuelSourceItem.m_HeatIncrease==10 &&
                Close(log.m_FuelSourceItem.GetModifiedBurnDurationHours(1),3),"firelog gives fifty success points, ten degrees, and three base hours");
            check(Close(stick.m_FuelSourceItem.GetModifiedBurnDurationHours(1),10f/60) &&
                items.First(i=>LootBalancePolicy.ItemName(i.name)=="GEAR_ReclaimedWoodB").m_FuelSourceItem.m_FireStartSkillModifier==0,
                "stick duration and neutral reclaimed wood modifier reach native fuel components");
            phase="tinder success";
            var skills=root.AddComponent<Game.SkillsManager>();
            var fire=root.AddComponent<Game.Skill_Firestarting>();
            fire.m_TierPoints=new[] { 0,20,50,100,200 };
            fire.m_BaseSuccessChance=new[] { 40,55,65,75,90 };
            fire.m_LevelWhereTinderNotRequired=3;
            skills.m_Skill_Firestarting=fire;
            var mend=root.AddComponent<Game.Skill_ClothingRepair>();
            mend.m_TierPoints=new[] { 0,25,75,150,350 };
            skills.m_Skill_ClothingRepair=mend;
            Game.GameManager.m_SkillsManager=skills;
            var manager=root.AddComponent<Game.FireManager>();
            var starter=Create("GEAR_WoodMatches");
            foreach (int points in new[] { 0,50,100,200 })
            {
                fire.m_CurrentPoints=points;
                FireBalance.SelectedTinder=null;
                float baseline=manager.CalculateFireStartSuccess(starter.m_FireStarterItem,stick.m_FuelSourceItem,null);
                foreach (string name in new[] { "GEAR_Newsprint","GEAR_CashBundle" })
                {
                    FireBalance.SelectedTinder=items.First(i=>LootBalancePolicy.ItemName(i.name)==name).m_FuelSourceItem;
                    float chance=manager.CalculateFireStartSuccess(starter.m_FireStarterItem,stick.m_FuelSourceItem,null);
                    check(Close(chance,Math.Min(100,baseline+FireBalancePolicy.TinderBonus(name))) && stick.m_FuelSourceItem.m_FireStartSkillModifier==10,
                        "native success calculation adds tinder once and restores fuel tuning at firestarting points "+points+": "+name);
                    check(manager.CalculateFireStartSuccess(starter.m_FireStarterItem,stick.m_FuelSourceItem,null)==chance,"repeated previews cannot accumulate a tinder bonus");
                }
                check(fire.TinderRequired()==(points==0),"native tinder requirement remains optional from level three at points "+points);
            }
            FireBalance.SelectedTinder=null;
            var panel=root.AddComponent<Game.Panel_FireStart>();
            panel.m_StarterList=new Il2CppSystem.Collections.Generic.List<Game.GearItem>();
            panel.m_StarterList.Add(starter);
            panel.m_FuelList=new Il2CppSystem.Collections.Generic.List<Game.GearItem>();
            panel.m_FuelList.Add(stick);
            panel.m_TinderList=new Il2CppSystem.Collections.Generic.List<Game.GearItem>();
            panel.m_TinderList.Add(items.First(i=>LootBalancePolicy.ItemName(i.name)=="GEAR_Newsprint"));
            panel.m_AccelerantList=new Il2CppSystem.Collections.Generic.List<Game.GearItem>();
            panel.m_Label_ChanceSuccess=root.AddComponent<Game.UILabel>();
            panel.m_Label_Tinder=root.AddComponent<Game.UILabel>();
            panel.m_SelectedStarterIndex=panel.m_SelectedFuelIndex=panel.m_SelectedTinderIndex=0;
            panel.m_SelectedAccelerantIndex=-1;
            panel.RefreshChanceOfSuccessLabel();
            check(panel.m_Label_ChanceSuccess.text.Contains("100") && FireBalance.SelectedTinder==null,
                "native fire UI preview includes tinder and restores the calculation context at level five");
            panel.RefreshTinderLabel();
            check(panel.GetSelectedTinder()!=null && panel.m_Label_Tinder.text.Contains("(+8%)") && !fire.TinderRequired(),
                "level-five fire UI retains optional tinder selection and displays its bonus");
            phase="mending";
            mend.m_CurrentPoints=249;
            check(mend.GetCurrentTierNumber()==3 && mend.m_TierPoints[4]==250,"Mending remains level four at 249 points and uses the new cap");
            mend.m_CurrentPoints=250;
            check(mend.GetCurrentTierNumber()==4 && mend.GetMaxPoints()==250,"Mending reaches level five at 250 points");
            var ruined=Create("GEAR_WoolSweater");
            ruined.SetNormalizedHP(0,false);
            ruined.m_WornOut=true;
            check(MendingBalance.CanRestore(ruined) && ruined.CurrentHP==0 && ruined.IsWornOut(),"master Mending recognizes repairable ruined gear without reviving it for free");
            mend.m_CurrentPoints=249;
            check(!MendingBalance.CanRestore(ruined),"ruined repairs stay locked before Mending level five");
            mend.m_CurrentPoints=250;
            var examine=root.AddComponent<Game.Panel_Inventory_Examine>();
            examine.m_GearItem=ruined;
            examine.m_Repairable=ruined.m_Repairable.Cast<Gear.IRepairable>();
            RuinedRepairObserver.Open=false;
            examine.RefreshRepairPanel();
            check(RuinedRepairObserver.Open && ruined.IsWornOut() && ruined.CurrentHP==0,
                "native repair UI gate opens at Mending five without clearing ruined status or granting health outside its scope");
            mend.m_CurrentPoints=249;
            RuinedRepairObserver.Open=false;
            examine.RefreshRepairPanel();
            check(!RuinedRepairObserver.Open && ruined.IsWornOut() && ruined.CurrentHP==0,
                "native repair UI gate remains closed at Mending four");
            mend.m_CurrentPoints=250;
            RuinedRepairObserver.Complete=true;
            examine.RepairSuccessful();
            check(ruined.CurrentHP>0 && !ruined.IsWornOut(),"a controlled successful native repair callback revives the ruined item");
            ruined.Deserialize(ruined.Serialize(),false);
            check(ruined.CurrentHP>0 && !ruined.IsWornOut(),"restored ruined gear remains usable after a native save round trip");
            var food=Create("GEAR_DogFood");
            food.SetNormalizedHP(0,false);
            food.m_WornOut=true;
            check(!MendingBalance.CanRestore(food),"master Mending cannot repair ruined food or invent missing repair actions");
            var savedMending=mend.Serialize();
            mend.Deserialize(savedMending);
            check(mend.GetCurrentTierNumber()==4 && mend.m_TierPoints[4]==250,"native Mending saves retain level five at the new threshold");
            phase="cougar health";
            var cougar=root.AddComponent<AI.AiCougar>();
            cougar.m_MaxHP=300;
            cougar.m_CurrentHP=180;
            CougarBalance.Initialize(cougar);
            CougarBalance.Initialize(cougar);
            check(cougar.m_MaxHP==150 && cougar.m_CurrentHP==90,"cougar health halves once and preserves existing wounds");
            cougar.m_CachedTransform=root.transform;
            var realSave=cougar.Serialize();
            check(Game.BaseAi.GetBaseAiDataProxyFromText(realSave).m_CurrentHP==180 && cougar.m_CurrentHP==90,
                "native cougar serialization writes original-scale HP and leaves live health unchanged");
            for (int reload=0;reload<3;reload++)
            {
                cougar.Deserialize(realSave);
                check(cougar.m_CurrentHP==90 && cougar.m_MaxHP==150,"native cougar deserialization preserves wounded health across repeated loads");
            }
            cougar.m_PlayerPassingAttackDamageMin=20;
            cougar.m_PlayerPassingAttackDamageMax=40;
            cougar.m_AiPassingAttackDamage=1000;
            CougarSwipeObserver.Observed=false;
            cougar.OnAnimationEvent_OnPassingAttackChomp();
            check(CougarSwipeObserver.Observed && cougar.m_PlayerPassingAttackDamageMin==20 && cougar.m_PlayerPassingAttackDamageMax==40 &&
                cougar.m_AiPassingAttackDamage==1000,"native cougar swipe entry receives half damage and restores tuning after the attack");
            var proxy=new Game.BaseAiDataProxy { m_CurrentHP=cougar.m_CurrentHP };
            string saved=SaveCougarHealth.ToNativeScale(Game.Utils.SerializeObject(proxy));
            var native=Game.BaseAi.GetBaseAiDataProxyFromText(saved);
            check(native.m_CurrentHP==180 && cougar.m_CurrentHP==90,"cougar save conversion uses native-compatible HP without changing the animal");
            for (int reload=0;reload<3;reload++)
            {
                float loaded=Game.BaseAi.GetBaseAiDataProxyFromText(saved).m_CurrentHP*CougarBalance.Scale;
                check(loaded==90,"repeated native cougar proxy loads preserve wounded health");
            }
            var freshCougar=root.AddComponent<AI.AiCougar>();
            freshCougar.m_CachedTransform=root.transform;
            freshCougar.m_MaxHP=freshCougar.m_CurrentHP=300;
            string freshSave=freshCougar.Serialize();
            check(freshCougar.m_MaxHP==150 && freshCougar.m_CurrentHP==150 && Game.BaseAi.GetBaseAiDataProxyFromText(freshSave).m_CurrentHP==300,
                "saving an uninitialized cougar applies balance before native serialization exactly once");
            freshCougar.m_CurrentHP=0;
            freshCougar.Deserialize(freshCougar.Serialize());
            check(freshCougar.m_CurrentHP==0 && freshCougar.m_MaxHP==150,"native cougar saves never revive a dead animal");
            phase="feat effects";
            var blizzard=root.AddComponent<Game.Feat_BlizzardWalker>();
            blizzard.m_WalkingSpeedInWindReductionPercent=25;
            var books=root.AddComponent<Game.Feat_BookSmarts>();
            books.m_PercentBenefit=10;
            var heart=root.AddComponent<Game.Feat_StraightToHeart>();
            heart.m_EffectiveLengthIncreasePercent=25;
            blizzard.m_FeatType=Game.FeatType.BlizzardWalker;
            books.m_FeatType=Game.FeatType.BookSmarts;
            heart.m_FeatType=Game.FeatType.StraightToHeart;
            books.m_NumHoursRequired=0;
            heart.m_ItemConsumedCountRequired=0;
            blizzard.m_BlizzardHoursOutsideRequired=0;
            Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.Add(Game.FeatType.BlizzardWalker);
            Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.Add(Game.FeatType.BookSmarts);
            Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.Add(Game.FeatType.StraightToHeart);
            var coffee=Create("GEAR_CoffeeCup");
            heart.m_Items=new Il2CppSystem.Collections.Generic.List<Gear.GearItemData>();
            heart.m_Items.Add(coffee.m_GearItemData);
            check(Close(blizzard.GetWalkingSpeedInWindReductionModifier(),0.5f) && Close(books.GetSkillPointsScale(),2) &&
                Close(heart.GetEffectiveLengthModifier(coffee),2) && Close(heart.GetEffectiveLengthModifier(stick),1),
                "enabled native feats halve wind resistance and double research XP and eligible drink duration");
            blizzard.IsUnlockedAndEnabled(); books.IsUnlockedAndEnabled(); heart.IsUnlockedAndEnabled();
            check(blizzard.m_WalkingSpeedInWindReductionPercent==50 && books.m_PercentBenefit==100 && heart.m_EffectiveLengthIncreasePercent==100,
                "native feat eligibility path configures fifty-percent Blizzard Walker and doubled Book Smarts and Straight to the Heart bonuses");
            phase="difficulty isolation";
            foreach (string difficulty in new[] { "Stalker","Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                mend.m_CurrentPoints=250;
                check(mend.GetCurrentTierNumber()==3 && mend.m_TierPoints[4]==350 && !MendingBalance.CanRestore(ruined),difficulty+" preserves Mending thresholds and ruined repair restrictions");
                blizzard.IsUnlockedAndEnabled();books.IsUnlockedAndEnabled();heart.IsUnlockedAndEnabled();
                check(blizzard.m_WalkingSpeedInWindReductionPercent==25 && books.m_PercentBenefit==10 && heart.m_EffectiveLengthIncreasePercent==25,
                    difficulty+" restores original feat tuning on the same persistent feat objects");
                var vanillaLog=Create("GEAR_Firelog");
                check(vanillaLog.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms(2) && vanillaLog.m_FuelSourceItem.m_BurnDurationHours==2 &&
                    vanillaLog.m_FuelSourceItem.m_HeatIncrease==3,difficulty+" keeps native firelog weight, duration, and heat");
                var otherCougar=root.AddComponent<AI.AiCougar>();
                otherCougar.m_MaxHP=otherCougar.m_CurrentHP=300;
                CougarBalance.Initialize(otherCougar);
                check(otherCougar.m_MaxHP==300 && otherCougar.m_CurrentHP==300,difficulty+" retains normal cougar health");
            }
        }
        catch (Exception ex) { throw new InvalidOperationException("Fire/feat/Mending validation phase: "+phase,ex); }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            Game.GameManager.m_SkillsManager=oldSkills;
            Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.Clear();
            foreach (var feat in enabled) Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.Add(feat);
            FireBalance.SelectedTinder=oldTinder;
            foreach (var gear in items) if (gear!=null) UnityEngine.Object.DestroyImmediate(gear.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}

// Observe the real animation-event -> native swipe entry without injuring a
// player or spawning a live animal into the menu-only validation fixture.
[HarmonyPatch]
internal static class CougarSwipeObserver
{
    internal static bool Observed;
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(AI.AiCougar),nameof(AI.AiCougar.DoPassingAttackDamage));
    [HarmonyPriority(Priority.Last)]
    private static bool Prefix(AI.AiCougar __instance)
    {
        if (__instance.gameObject.name!="IO_FireFeatMendingValidation") return true;
        Observed=__instance.m_PlayerPassingAttackDamageMin==10 && __instance.m_PlayerPassingAttackDamageMax==20 && __instance.m_AiPassingAttackDamage==500;
        return false;
    }
}

// Exercise repair scopes without running menus, time acceleration, or inventory
// consumption against a player save. Success models the native condition update.
[HarmonyPatch]
internal static class RuinedRepairObserver
{
    internal static bool Open,Complete;
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Panel_Inventory_Examine),nameof(Game.Panel_Inventory_Examine.RefreshRepairPanel));
        yield return AccessTools.Method(typeof(Game.Panel_Inventory_Examine),nameof(Game.Panel_Inventory_Examine.RepairSuccessful));
    }
    [HarmonyPriority(Priority.Last)]
    private static bool Prefix(Game.Panel_Inventory_Examine __instance,MethodBase __originalMethod)
    {
        if (__instance.gameObject.name!="IO_FireFeatMendingValidation") return true;
        var gear=__instance.m_GearItem;
        Open=gear!=null && !gear.m_WornOut;
        if (__originalMethod.Name==nameof(Game.Panel_Inventory_Examine.RepairSuccessful) && Complete && Open)
            gear.SetNormalizedHP(0.20f,false);
        return false;
    }
}
