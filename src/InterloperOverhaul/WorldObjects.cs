extern alias GameAssembly;

using Game = GameAssembly::Il2Cpp;
using UnityEngine;

namespace InterloperOverhaul;

internal static class WorldObjects
{
    internal static bool IsInterloper =>
        Game.ExperienceModeManager.GetCurrentExperienceModeType() == Game.ExperienceModeType.Interloper;

    internal static string PathOf(GameObject go)
    {
        string path = go.name;
        for (Transform parent = go.transform.parent; parent != null; parent = parent.parent)
            path = parent.name + "/" + path;
        return path;
    }

    internal static bool IsRestorable(GameObject go)
    {
        if (go == null) return false;
        string path = PathOf(go);
        // Keep authored alternatives mutually exclusive and leave wildlife and
        // survivor spawn rules under the original difficulty's control.
        if (WorldPolicy.IsReducedCargo(go.scene.name, path) || WorldPolicy.IsReducedRadioCache(path) ||
            WorldPolicy.IsNonItemSpawnPath(path)) return false;
        if (go.GetComponentInChildren<Game.BaseAi>(true) != null || go.GetComponentInChildren<Game.SpawnRegion>(true) != null ||
            go.GetComponentInChildren<Game.SpawnWildlife_Trigger>(true) != null || go.GetComponentInChildren<Game.PlayerSpawn>(true) != null ||
            go.GetComponentInChildren<Game.PlayerSpawnPoints>(true) != null) return false;
        return true;
    }

    internal static bool IsHouseSpawner(Game.RandomSpawnObject spawner) =>
        WorldPolicy.IsCoastalHouseSpawner(spawner.gameObject.scene.name,
            PathOf(spawner.gameObject), spawner.name);
}
