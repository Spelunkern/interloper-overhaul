extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;
using Trader = GameAssembly::Il2CppTLD.Trader;

namespace InterloperOverhaul;

[HarmonyPatch]
internal static class TraderFirearmSupplies
{
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Trader.TraderManager), nameof(Trader.TraderManager.GetAvailableTradeExchanges));

    internal static void Filter(Il2CppSystem.Collections.Generic.List<Trader.TradeExchangeData> exchanges, int seed, int aurora)
    {
        for (int i = exchanges.Count - 1; i >= 0; i--)
        {
            var exchange = exchanges[i];
            if (exchange == null || exchange.m_GainedItem == null) continue;
            string item = LootBalancePolicy.TraderItem(exchange.m_GainedItem.AssetGUID);
            if (LootBalancePolicy.IsWeaponVariant(item) || FirearmSpawnPolicy.IsFiniteItem(item)) { exchanges.RemoveAt(i); continue; }
            if (exchange.m_IsSpecialRequest) continue;
            if (!LootBalancePolicy.KeepTraderSupply(seed, item, aurora))
                exchanges.RemoveAt(i);
        }
    }

    private static void Postfix(Trader.TraderManager __instance, ref Il2CppSystem.Collections.Generic.List<Trader.TradeExchangeData> __result)
    {
        if (!LootBalance.Active || __result == null || __instance.m_CurrentState == null) return;
        // Return a view for this aurora; never remove entries from the cached stock,
        // active trades, or the underlying authored exchange assets.
        var filtered = new Il2CppSystem.Collections.Generic.List<Trader.TradeExchangeData>();
        foreach (var exchange in __result) filtered.Add(exchange);
        Filter(filtered, FiniteFirearms.State?.Seed ?? Game.GameManager.GetRandomSeed(0), __instance.m_CurrentState.m_LastAuroraNum);
        __result = filtered;
    }
}
