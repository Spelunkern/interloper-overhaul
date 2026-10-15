extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
using Trader = GameAssembly::Il2CppTLD.Trader;
using UnityEngine;

namespace InterloperOverhaul;

// Synchronous reward generation only; restores the original mode before returning.
// Native private getters omit unused instance arguments, so Harmony instance hooks
// on those helpers are unsafe. Use a nested scope around the public generation work.
internal static class RewardScope
{
    [ThreadStatic] private static int depth;
    internal static bool Active => depth > 0;
    [ThreadStatic] private static Gameplay.GameModeConfig previousMode;
    internal static void Enter()
    {
        if (depth == 0)
        {
            var normal = Game.ExperienceModeManager.GetGameModeFromName("Stalker");
            previousMode = Game.ExperienceModeManager.s_CurrentGameMode;
            Game.ExperienceModeManager.s_CurrentGameMode = normal;
        }
        depth++;
    }
    internal static void Leave()
    {
        if (--depth != 0) return;
        Game.ExperienceModeManager.s_CurrentGameMode = previousMode;
        previousMode = null;
    }
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
        if ((!WorldObjects.IsInterloper && !RewardScope.Active) || !RewardScope.IsRewardContainer(__instance)) return;
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
