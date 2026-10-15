extern alias GameAssembly;

using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InterloperOverhaul;

// Validation-only: use the game's actual beachcombing tables and authored prefabs.
internal static class BeachcombingValidation
{
    internal static void Run(System.Action<bool, string> check)
    {
        var originalMode = Game.ExperienceModeManager.s_CurrentGameMode;
        float modifier = originalMode.m_XPMode.m_TidelineSpawnModifier;
        foreach (var fixture in new[] { ("LootTableTideLine", 32), ("LootTableTideLineBlizzard", 37) })
        {
            string path = "Assets/ScriptableObjects/LootTables/" + fixture.Item1 + ".asset";
            var handle = Addressables.LoadAssetAsync<Game.LootTableData>((Il2CppSystem.String)path);
            Game.LootTableData table = null;
            try
            {
                table = UnityEngine.Object.Instantiate(handle.WaitForCompletion());
                check(table != null && table.m_BaseEntries.Count == fixture.Item2, "authored beach table loaded: " + fixture.Item1);
                int weight = 0;
                foreach (var entry in table.m_BaseEntries)
                {
                    weight += entry.m_Weight;
                    var prefab = table.LoadPrefab(entry.m_Item);
                    check(prefab != null && !table.ShouldFilterOutPrefab(prefab),
                        fixture.Item1 + " allows original entry: " + (prefab != null ? prefab.name : "missing"));
                }
                check(table.GetPrefabByName("GEAR_RifleAmmoSingle") != null,
                    fixture.Item1 + " native filtered lookup includes live ammunition");
                if (fixture.Item1 == "LootTableTideLineBlizzard")
                {
                    foreach (string item in new[] { "GEAR_Knife", "GEAR_MRE", "GEAR_ClimbingSocks", "GEAR_Arrow" })
                        check(table.GetPrefabByName(item) != null, "post-blizzard table includes " + item);
                }
                int after = 0;
                foreach (var entry in table.m_BaseEntries) after += entry.m_Weight;
                check(after == weight && table.m_BaseEntries.Count == fixture.Item2,
                    fixture.Item1 + " original entry count and weights preserved");
            }
            finally
            {
                if (table != null) { Gameplay.RandomTableDataManager.SetTableDirty(table); UnityEngine.Object.Destroy(table); }
                Addressables.Release(handle);
            }
        }

        var bigHandle = Addressables.LoadAssetAsync<Gameplay.BeachcombingBigItemTableData>(
            (Il2CppSystem.String)"Assets/ScriptableObjects/LootTables/BigItemTables/BeachcombingBigItemTableData1.asset");
        Gameplay.BeachcombingBigItemTableData big = null;
        var locationGo = new GameObject("IO_BeachLocation");
        locationGo.SetActive(false);
        try
        {
            big = UnityEngine.Object.Instantiate(bigHandle.WaitForCompletion());
            check(big != null && big.m_BaseEntries.Count == 16, "original large beachcombing table loaded");
            var location = locationGo.AddComponent<Gameplay.BeachcombingBigItemLocation>();
            location.m_AllowedObjects = new Il2CppSystem.Collections.Generic.List<Gameplay.AssetReferenceCustomSpawnedItem>();
            foreach (var entry in big.m_BaseEntries) location.m_AllowedObjects.Add(entry.m_Item);
            var locations = new Il2CppSystem.Collections.Generic.List<Gameplay.BeachcombingBigItemLocation>();
            locations.Add(location);
            big.FilterItemsForLocations(locations);
            int restored = 0;
            Game.DisableObjectForXPMode canoeArrow = null;
            foreach (var entry in big.m_BaseEntries)
            {
                var prefab = big.LoadPrefab(entry.m_Item);
                check(prefab != null && !big.ShouldFilterOutItem(entry.m_Item),
                    "native big-item table accepts original beach find: " + (prefab != null ? prefab.name : "missing"));
                foreach (var disabler in prefab.GetComponentsInChildren<Game.DisableObjectForXPMode>(true))
                {
                    if (!disabler.m_XPModesToDisable.Contains(Game.ExperienceModeType.Interloper)) continue;
                    check(!disabler.ShouldDisableForCurrentMode(), "authored beach content restored: " + WorldObjects.PathOf(disabler.gameObject));
                    restored++;
                    if (disabler.gameObject.name == "GEAR_Arrow") canoeArrow = disabler;
                }
            }
            check(restored >= 8 && canoeArrow != null, "boat arrows, tools, fishing tackle, and flare shells covered");
            location.m_AllowedObjects.Clear();
            location.m_AllowedObjects.Add(big.m_BaseEntries[0].m_Item);
            check(big.ShouldFilterOutItem(big.m_BaseEntries[1].m_Item), "large beach find still requires a compatible location");
            Game.ExperienceModeManager.s_CurrentGameMode = Game.ExperienceModeManager.GetGameModeFromName("Misery");
            check(canoeArrow.ShouldDisableForCurrentMode(), "Misery retains its original canoe-arrow exclusion");
        }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode = originalMode;
            if (big != null) { Gameplay.RandomTableDataManager.SetTableDirty(big); UnityEngine.Object.Destroy(big); }
            UnityEngine.Object.Destroy(locationGo);
            Addressables.Release(bigHandle);
        }
        check(originalMode.m_XPMode.m_TidelineSpawnModifier == modifier, "Interloper tideline spawn modifier unchanged");
    }
}
