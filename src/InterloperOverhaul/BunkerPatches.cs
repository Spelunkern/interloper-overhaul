extern alias GameAssembly;

using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
using MelonLoader;

namespace InterloperOverhaul;

[HarmonyPatch]
internal static class BunkerPatches
{
    private static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Gameplay.BunkerDistributor), nameof(Gameplay.BunkerDistributor.DistributeBunkerInteriors));
        yield return AccessTools.Method(typeof(Gameplay.BunkerDistributor), nameof(Gameplay.BunkerDistributor.Deserialize));
    }

    private static void Prefix(Gameplay.BunkerDistributor __instance, System.Reflection.MethodBase __originalMethod,
        out Il2CppReferenceArray<Gameplay.BunkerInteriorSpecification> __state)
    {
        __state = null;
        if (!WorldObjects.IsInterloper) return;
        var stalker = Game.ExperienceModeManager.GetGameModeFromName("Stalker")?.TryCast<Gameplay.SandboxConfig>();
        var normal = stalker?.m_BunkerSetup?.m_BunkerInteriors;
        if (normal == null || normal.Length == 0)
        {
            MelonLogger.Error("Normal bunker specifications unavailable; keeping the existing distribution.");
            return;
        }
        __state = __instance.m_BunkerInteriors;
        // Let the game's seeded distributor choose the locations and persist them normally.
        // Only the interior variants change; container loot generation is untouched.
        if (__originalMethod.Name == nameof(Gameplay.BunkerDistributor.Deserialize))
        {
            // Existing saves may still name Interloper-only interiors. Keep those resolvable
            // alongside the normal variants; never discard a previously saved assignment.
            var known = new System.Collections.Generic.List<Gameplay.BunkerInteriorSpecification>();
            foreach (var interior in normal) known.Add(interior);
            foreach (var interior in __state) known.Add(interior);
            __instance.m_BunkerInteriors = new Il2CppReferenceArray<Gameplay.BunkerInteriorSpecification>(known.ToArray());
        }
        else __instance.m_BunkerInteriors = normal;
        MelonLogger.Msg("[World] Using normal bunker variants for Interloper generation / loading.");
    }

    private static void Finalizer(Gameplay.BunkerDistributor __instance,
        Il2CppReferenceArray<Gameplay.BunkerInteriorSpecification> __state)
    {
        if (__state != null) __instance.m_BunkerInteriors = __state;
    }
}
