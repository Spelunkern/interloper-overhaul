extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
using State=Il2CppTLD.SaveState;
using UnityEngine;

namespace InterloperOverhaul;

internal static class FeatNotificationValidation
{
    internal static Game.Feat_NightWalker Fixture;
    internal static void Run(Action<bool,string> check)
    {
        var profile=State.ProfileState.Instance;
        var badges=profile.m_UnlockedBadges;
        var previous=Game.FeatsManager.m_Feat_NightWalker;
        var selected=Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.ToArray();
        var manager=Resources.FindObjectsOfTypeAll<Game.FeatsManager>().First(value=>value.gameObject.scene.IsValid());
        string progress=manager.Serialize();
        var root=new GameObject("IO_FeatNotificationValidation");root.SetActive(false);
        try
        {
            Fixture=root.AddComponent<Game.Feat_NightWalker>();
            Fixture.m_FeatType=Game.FeatType.NightWalker;
            Fixture.m_FatigueScaleDayMultiplier=1.1f;Fixture.m_FatigueScaleNightMultiplier=0.85f;
            Game.FeatsManager.m_Feat_NightWalker=Fixture;
            profile.m_UnlockedBadges=new Il2CppSystem.Collections.Generic.List<State.BadgeInfoType>();
            profile.m_UnlockedBadges.Add(Game.Feat_NightWalker.BADGE);
            Fixture.m_Unlocked=false;Fixture.m_KickerPending=false;
            AccessTools.Method(typeof(Game.Feat_NightWalker),"Update").Invoke(Fixture,null);
            check(Fixture.IsUnlocked() && Fixture.m_Unlocked && Fixture.m_KickerPending,
                "native Night Walker reproduces the late-profile duplicate unlock kicker");
            Fixture.m_Unlocked=false;
            manager.Deserialize(progress);
            check(Fixture.IsUnlocked() && Fixture.m_Unlocked && !Fixture.m_KickerPending,
                "native feat profile restoration synchronizes an existing Night Walker unlock without a kicker");
            Fixture.m_Unlocked=false;Fixture.m_KickerPending=true;
            manager.Deserialize(progress);
            check(Fixture.m_Unlocked && !Fixture.m_KickerPending,
                "repeated native profile reload cannot requeue the existing feat announcement");
            profile.m_UnlockedBadges.Clear();Fixture.m_Unlocked=false;Fixture.m_KickerPending=false;
            manager.Deserialize(progress);
            check(!Fixture.IsUnlocked() && !Fixture.m_Unlocked && !Fixture.m_KickerPending && profile.m_UnlockedBadges.Count==0,
                "load synchronization never awards Night Walker or its prerequisite badge");
            profile.m_UnlockedBadges.Add(Game.Feat_NightWalker.BADGE);
            AccessTools.Method(typeof(Game.Feat_NightWalker),"Update").Invoke(Fixture,null);
            check(Fixture.m_Unlocked && Fixture.m_KickerPending,
                "a genuinely new badge gained outside restoration retains the native feat announcement");
            check(Fixture.m_FatigueScaleDayMultiplier==1.1f && Fixture.m_FatigueScaleNightMultiplier==0.85f &&
                Game.FeatEnabledTracker.m_FeatsEnabledThisSandbox.ToArray().SequenceEqual(selected) && manager.Serialize()==progress,
                "notification correction preserves feat benefits, selected feats, and serialized progression");
        }
        finally
        {
            profile.m_UnlockedBadges=badges;
            Game.FeatsManager.m_Feat_NightWalker=previous;
            Fixture=null;
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}

// Exercise native Update without opening UI or writing a user's profile.
[HarmonyPatch]
internal static class ObserveNativeFeatKicker
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.Feat),"TryToDisplayKicker");
    private static bool Prefix(Game.Feat __instance,ref bool __result)
    {
        if(FeatNotificationValidation.Fixture==null || __instance.Pointer!=FeatNotificationValidation.Fixture.Pointer) return true;
        __result=false;return false;
    }
}
