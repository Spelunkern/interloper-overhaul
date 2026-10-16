extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;

namespace InterloperOverhaul;
internal static class FeatBalance
{
    private static readonly Dictionary<int,int> original=new();
    internal static void RestoreNightWalker()
    {
        var feat=Game.FeatsManager.m_Feat_NightWalker;
        // Night Walker derives its unlock from the saved event badge, rather than
        // FeatsManagerSaveData. Awake can run before that profile is restored.
        // Synchronize its transient latch at load boundaries, without awarding
        // a badge or suppressing a genuine unlock during play.
        if(feat!=null && feat.IsUnlocked())
        {
            feat.m_Unlocked=true;
            feat.m_KickerPending=false;
        }
    }
    internal static void Configure(Game.Feat feat)
    {
        if (feat==null) return;
        var blizzard=feat.TryCast<Game.Feat_BlizzardWalker>();
        var books=feat.TryCast<Game.Feat_BookSmarts>();
        var heart=feat.TryCast<Game.Feat_StraightToHeart>();
        if (blizzard==null && books==null && heart==null) return;
        int id=feat.GetInstanceID();
        if (!original.TryGetValue(id,out int baseline))
        {
            baseline=blizzard!=null ? blizzard.m_WalkingSpeedInWindReductionPercent : books!=null ? books.m_PercentBenefit : heart.m_EffectiveLengthIncreasePercent;
            original[id]=baseline;
        }
        int value=ItemBalance.Active ? (blizzard!=null ? 50 : 100) : baseline;
        if (blizzard!=null) blizzard.m_WalkingSpeedInWindReductionPercent=value;
        if (books!=null) books.m_PercentBenefit=value;
        if (heart!=null) heart.m_EffectiveLengthIncreasePercent=value;
    }
}
[HarmonyPatch]
internal static class BalanceFeatBenefits
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // Native effect getters call this shared method even when their own
        // bodies are inlined. Keep unlock and selected-feat checks native.
        yield return AccessTools.Method(typeof(Game.Feat),nameof(Game.Feat.IsUnlockedAndEnabled));
        foreach (var type in new[] { typeof(Game.Feat_BlizzardWalker),typeof(Game.Feat_BookSmarts),typeof(Game.Feat_StraightToHeart) })
            foreach (string method in new[] { "GetDescription","GetShortDescription","GetBonusDescription" })
                yield return AccessTools.Method(type,method);
    }
    private static void Prefix(Game.Feat __instance) => FeatBalance.Configure(__instance);
}

[HarmonyPatch]
internal static class RestoreExistingNightWalker
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.FeatsManager),nameof(Game.FeatsManager.Deserialize));
        yield return AccessTools.Method(typeof(Game.SaveGameSystem),nameof(Game.SaveGameSystem.RestoreGlobalData));
        yield return AccessTools.Method(typeof(Game.GameManager),nameof(Game.GameManager.LaunchSandbox));
    }
    private static void Postfix()=>FeatBalance.RestoreNightWalker();
}
