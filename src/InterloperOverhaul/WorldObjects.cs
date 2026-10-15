extern alias GameAssembly;

using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
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
        if (WorldPolicy.IsReducedRadioCache(path)) return false;
        if (RewardScope.Active || WorldPolicy.IsTaleReward(go.name) || WorldPolicy.IsTaleBunker(go.scene.name)) return true;
        if (WorldPolicy.IsMemento(path)) return true;
        if (WorldPolicy.IsCoastalStandaloneHouse(go.scene.name, path)) return true;
        // Do not re-enable weapons, loose loot, plants, wildlife or player spawns.
        // A memento can itself be a GearItem, hence the earlier explicit exception.
        var gear = go.GetComponent<Game.GearItem>();
        if (gear != null)
            // Generated rewards must survive leaving their container and reloading.
            // These flags are false on untouched world spawns and source prefabs.
            return gear.m_BeenInContainer || gear.m_BeenInPlayerInventory;
        if (go.GetComponent<Game.Container>() != null) return true;
        if (go.GetComponent<Game.TrackableItem>() != null) return true;
        if (go.GetComponent<Game.NarrativeCollectibleItem>() != null) return true;
        if (go.GetComponent<Game.MapDetail>() != null &&
            go.GetComponent<Game.BodyHarvest>() == null && go.GetComponent<Game.Harvestable>() == null) return true;
        return false;
    }

    internal static bool IsHouseSpawner(Game.RandomSpawnObject spawner) =>
        WorldPolicy.IsCoastalHouseSpawner(spawner.gameObject.scene.name,
            PathOf(spawner.gameObject), spawner.name);
}
