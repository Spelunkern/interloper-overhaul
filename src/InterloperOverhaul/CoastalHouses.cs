extern alias GameAssembly;

using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Game = GameAssembly::Il2Cpp;
using UnityEngine;
using MelonLoader;

namespace InterloperOverhaul;

internal static class CoastalHouses
{
    internal static bool Prepare(Game.RandomSpawnObject spawner)
    {
        if (!WorldObjects.IsInterloper || !WorldObjects.IsHouseSpawner(spawner)) return false;
        var originals = spawner.m_ObjectList;
        if (spawner.name == WorldPolicy.NightmareSpawner)
        {
            // This legacy pointer aliases Waterfront_oldsave, which another group restores.
            // Do not let the nightmare group switch that shared intact layout off.
            spawner.m_OldSaveObject = null;
            if (originals != null)
                foreach (var go in originals)
                    if (go != null) go.SetActive(false);
            spawner.m_ObjectList = new Il2CppReferenceArray<GameObject>(0);
            spawner.m_Weights = new Il2CppStructArray<int>(0);
            return true;
        }

        var intact = spawner.m_OldSaveObject;
        if (intact == null)
        {
            MelonLogger.Warning($"No authored intact layout for {spawner.name}; leaving the original layout.");
            return false;
        }
        if (originals != null && originals.Length == 1 && originals[0] == intact) return true;
        if (originals != null)
            foreach (var go in originals)
                if (go != null && go != intact) go.SetActive(false);
        spawner.m_ObjectList = new Il2CppReferenceArray<GameObject>(new[] { intact });
        spawner.m_Weights = new Il2CppStructArray<int>(new[] { 1 });
        MelonLogger.Msg($"[World] Restored intact Coastal Highway layout: {spawner.name}");
        return true;
    }
}

[HarmonyPatch(typeof(Game.RandomSpawnObject), nameof(Game.RandomSpawnObject.Initialize))]
internal static class PrepareCoastalHouses
{
    private static void Prefix(Game.RandomSpawnObject __instance) => CoastalHouses.Prepare(__instance);
}

[HarmonyPatch(typeof(Game.RandomSpawnObject), nameof(Game.RandomSpawnObject.LoadObject))]
internal static class LoadIntactCoastalHouses
{
    private static bool Prefix(Game.RandomSpawnObject __instance, ref string objName, ref Vector3 location)
    {
        if (!CoastalHouses.Prepare(__instance)) return true;
        if (__instance.name == WorldPolicy.NightmareSpawner) return false;
        // Resolve old burned-layout selections to the same authored intact group on reload.
        var intact = __instance.m_OldSaveObject;
        objName = intact.name;
        location = intact.transform.position;
        return true;
    }
}
