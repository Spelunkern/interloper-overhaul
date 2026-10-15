extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
using Trader = GameAssembly::Il2CppTLD.Trader;
using UnityEngine;

namespace InterloperOverhaul;

// Synchronous reward generation only. Never changes the active game mode or its assets.
internal static class RewardScope
{
    [ThreadStatic] private static int depth;
    internal static bool Active => depth > 0;
    internal static void Enter() => depth++;
    internal static void Leave() => depth--;
    internal static Gameplay.ExperienceMode NormalExperience =>
        Game.ExperienceModeManager.GetGameModeFromName("Stalker").Cast<Gameplay.SandboxConfig>().m_XPMode;

    internal static bool IsRewardContainer(Game.Container container) =>
        WorldPolicy.IsTaleBunker(container.gameObject.scene.name) ||
        WorldPolicy.IsRadioCache(WorldObjects.PathOf(container.gameObject));
}

[HarmonyPatch]
internal static class RewardContainerContents
{
    internal sealed class State
    {
        internal Game.LootTableData Normal, Locked, Selected;
        internal readonly List<Game.LootTableData> Clones = new();
        internal Game.LootTableData Clone(Game.LootTableData original)
        {
            if (original == null) return null;
            var clone = UnityEngine.Object.Instantiate(original);
            Clones.Add(clone);
            return clone;
        }
    }

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Container), nameof(Game.Container.PopulateWithRandomGear));
        yield return AccessTools.Method(typeof(Game.Container), nameof(Game.Container.PopulateWithScriptedGear));
    }

    internal static void Prefix(Game.Container __instance, out State __state)
    {
        __state = null;
        if (!WorldObjects.IsInterloper || !RewardScope.IsRewardContainer(__instance)) return;
        __state = new State { Normal = __instance.m_LootTableData,
            Locked = __instance.m_LockedLootTableData, Selected = __instance.m_LootTable };
        RewardScope.Enter();
        // Tables cache their difficulty-filtered entries. Private clones prevent reward
        // generation from changing the shared tables used by ordinary world containers.
        __instance.m_LootTableData = __state.Clone(__state.Normal);
        __instance.m_LockedLootTableData = __state.Clone(__state.Locked);
        __instance.m_LootTable = __state.Clone(__state.Selected);
    }

    internal static void Finalizer(Game.Container __instance, State __state)
    {
        if (__state == null) return;
        try
        {
            __instance.m_LootTableData = __state.Normal;
            __instance.m_LockedLootTableData = __state.Locked;
            __instance.m_LootTable = __state.Selected;
            foreach (var clone in __state.Clones)
            {
                Gameplay.RandomTableDataManager.SetTableDirty(clone);
                UnityEngine.Object.Destroy(clone);
            }
        }
        finally { RewardScope.Leave(); }
    }
}

[HarmonyPatch(typeof(Game.LootTableData), nameof(Game.LootTableData.GetCurrentExperienceMode))]
internal static class RewardTableExperience
{
    private static void Postfix(ref Gameplay.ExperienceMode __result)
    {
        if (RewardScope.Active) __result = RewardScope.NormalExperience;
    }
}

[HarmonyPatch(typeof(Game.ExperienceModeManager), nameof(Game.ExperienceModeManager.GetCurrentExperienceMode))]
internal static class RewardExperience
{
    private static void Postfix(ref Gameplay.ExperienceMode __result)
    {
        if (RewardScope.Active) __result = RewardScope.NormalExperience;
    }
}

[HarmonyPatch(typeof(Game.ExperienceModeManager), nameof(Game.ExperienceModeManager.GetReduceMaxItemsInContainer))]
internal static class RewardQuantity
{
    private static void Postfix(ref int __result)
    {
        if (RewardScope.Active) __result = RewardScope.NormalExperience.m_ReduceMaxItemsInContainer;
    }
}

[HarmonyPatch(typeof(Game.ExperienceModeManager), nameof(Game.ExperienceModeManager.GetChanceForEmptyContainer))]
internal static class RewardEmptyChance
{
    private static void Postfix(ref int __result)
    {
        if (RewardScope.Active) __result = RewardScope.NormalExperience.m_ChanceForEmptyContainer;
    }
}

[HarmonyPatch(typeof(Trader.TradeExchange), nameof(Trader.TradeExchange.FilterExchangeData))]
internal static class SutherlandRewards
{
    internal static void Prefix(Trader.TradeExchange __instance, out bool __state)
    {
        __state = WorldObjects.IsInterloper && WorldPolicy.IsStoryExchange(__instance.name);
        if (__state) RewardScope.Enter();
    }

    internal static void Finalizer(bool __state)
    {
        if (__state) RewardScope.Leave();
    }
}
