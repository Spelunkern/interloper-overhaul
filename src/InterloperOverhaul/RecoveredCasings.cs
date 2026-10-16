extern alias GameAssembly;

using System.Reflection;
using HarmonyLib;
using Game = GameAssembly::Il2Cpp;

namespace InterloperOverhaul;

// Native shell discharge creates collectible casings at the player's feet.
// Preserve that recovery path even if the new gear's normal start is deferred.
[HarmonyPatch]
internal static class RecoveredCasings
{
    [ThreadStatic] private static int depth;
    internal static bool Active => depth > 0;
    private static MethodBase TargetMethod() => AccessTools.Method(typeof(Game.GunItem), nameof(Game.GunItem.HandleShellDischarge));

    internal static void Prefix(out bool __state)
    {
        __state = LootBalance.Active;
        if (__state) depth++;
    }

    internal static void Finalizer(bool __state)
    {
        if (__state) depth--;
    }

    internal static void Preserve(Game.GearItem gear)
    {
        if (!Active || !WorldPolicy.IsSpentCasing(LootBalancePolicy.ItemName(gear.name))) return;
        gear.SkipSpawnChanceRoll();
        gear.m_HasBeenOwnedByPlayer = true;
    }
}
