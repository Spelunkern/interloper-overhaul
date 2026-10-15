extern alias GameAssembly;

using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace InterloperOverhaul;

internal static class LootBalanceValidation
{
    internal static void Run(System.Action<bool, string> check)
    {
        var previous = Game.ExperienceModeManager.s_CurrentGameMode;
        var root = new GameObject("IO_LootBalance");
        root.SetActive(false);
        GameObject Child(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            return go;
        }
        try
        {
            foreach (var fixture in new[] { ("GEAR_Rifle", 0.01), ("GEAR_Revolver", 0.01), ("GEAR_Knife", 0.01),
                ("GEAR_HeavyParka", 0.01), ("GEAR_PremiumWinterCoat", 0.01), ("GEAR_CowichanSweater", 0.01),
                ("GEAR_Rifle_Curators", 0.0025), ("GEAR_SurvivalKnife", 0.0025), ("GEAR_PeanutButter", 0.05),
                ("GEAR_RifleAmmoSingle", 0.05), ("GEAR_RifleAmmoCasing", 1.0), ("GEAR_TacticalJacket", 1.0) })
            {
                var prefab = Game.GearItem.LoadGearItemPrefab(fixture.Item1);
                check(prefab != null && LootBalance.Retention(prefab.gameObject) == fixture.Item2,
                    "authored restored prefab receives correct retention: " + fixture.Item1);
            }
            var hammerGo = Child("GEAR_Hammer");
            var hammer = hammerGo.AddComponent<Game.GearItem>();
            check(LootBalance.Retention(hammerGo) == 1, "native placed hammer is not confused with excluded container hammer");
            var placed = Child("GEAR_Rifle");
            var gear = placed.AddComponent<Game.GearItem>();
            var disabler = placed.AddComponent<Game.DisableObjectForXPMode>();
            disabler.m_XPModesToDisable = new Il2CppSystem.Collections.Generic.List<Game.ExperienceModeType>();
            disabler.m_XPModesToDisable.Add(Game.ExperienceModeType.Interloper);
            check(!disabler.ShouldDisableForCurrentMode() && LootBalance.Retention(placed) == 0.01,
                "restored scene gear receives premium rarity through native eligibility hook");
            UnityEngine.Object.DestroyImmediate(disabler);
            check(LootBalance.Retention(placed) == 0.01, "restored source remembered after native disabler removal");
            gear.m_BeenInPlayerInventory = true;
            check(LootBalance.Retention(placed) == 1, "acquired player gear excluded from rarity filtering");
            gear.m_BeenInPlayerInventory = false;
            gear.m_BeenInContainer = true;
            check(LootBalance.Retention(placed) == 1, "saved container gear excluded from rarity filtering");
            gear.m_BeenInContainer = false;
            gear.m_RolledSpawnChance = true;
            gear.RollSpawnChance();
            check(placed != null && gear.m_RolledSpawnChance, "already rolled world gear is not removed on a later call");
            gear.m_RolledSpawnChance = false;
            gear.m_SpawnChance = 100;
            for (int n = 0; LootBalance.Keep(placed, LootBalance.Key(placed), 0); n++) placed.name = "GEAR_Rifle (" + n + ")";
            check(placed.activeSelf, "fresh world test starts with an enabled authored item");
            gear.RollSpawnChance();
            check(gear.m_RolledSpawnChance && !placed.activeSelf && gear.m_SpawnChance == 100,
                "native world spawn hook rejects even guaranteed restored placements without mutating authored chance");

            var radial = Child("IO_Tideline").AddComponent<Game.RadialObjectSpawner>();
            radial.IsTidelineSpawner = true;
            RareRadialGear.Prefix(radial, out int firstSpawn);
            var washedUp = Child("GEAR_Rifle");
            var washedGear = washedUp.AddComponent<Game.GearItem>();
            LootBalance.MarkRestored(washedUp);
            radial.m_Spawns.Add(washedUp);
            radial.m_NumToSpawn = 0;
            radial.m_HasSpawned = true;
            radial.m_GameHourTimeToRespawn = 123;
            for (int n = 0; LootBalance.Keep(washedUp, LootBalance.RadialKey(radial), 0); n++)
                radial.gameObject.name = "IO_Tideline" + n;
            RareRadialGear.Postfix(radial, firstSpawn);
            check(radial.m_Spawns.Count == 0 && radial.m_NumToSpawn == 0 && radial.m_HasSpawned &&
                radial.m_GameHourTimeToRespawn == 123, "fresh radial rejection preserves consumed count and respawn timer");
            string firstCycle = LootBalance.RadialKey(radial);
            radial.m_GameHourTimeToRespawn = 456;
            check(firstCycle != LootBalance.RadialKey(radial), "renewable beach cycles receive independent rarity opportunities");
            radial.m_PendingSpawns.Add(new Game.PendingSpawnData());
            RareRadialGear.Prefix(radial, out int savedSpawn);
            check(savedSpawn == -1, "saved radial finds bypass rarity during restoration");
            radial.m_PendingSpawns.Clear();
            radial.m_Spawns.Add(hammerGo);
            RareRadialGear.Postfix(radial, 0);
            check(radial.m_Spawns.Count == 1, "native radial items retain their original availability");

            var container = Child("CONTAINER_Test").AddComponent<Game.Container>();
            var rifle = Game.GearItem.LoadGearItemPrefab("GEAR_Rifle").gameObject;
            container.m_GearToInstantiate.Add(rifle); // An earlier generation stage: preserve it.
            int expected = 1;
            string key = LootBalance.Key(container.gameObject);
            for (int slot = 1; slot <= 1000; slot++)
            {
                container.m_GearToInstantiate.Add(rifle);
                if (LootBalance.Keep(rifle, key, slot)) expected++;
            }
            LootBalance.FilterPending(container, 1);
            check(container.m_GearToInstantiate.Count == expected && expected > 1 && expected < 30,
                "all-premium container loses rejected slots without replacement and preserves earlier stages");
            check(container.m_Items.Count == 0, "rarity filter never adds or removes stored inventory items");
            container.m_GearToInstantiate.Clear();
            container.m_ScriptedItemPrefabs = new Il2CppReferenceArray<GameObject>(new[] { rifle });
            container.m_ScriptedItemFilter = new Il2CppStructArray<Game.GameModeFilter>(new[] { Game.GameModeFilter.All });
            bool keep = LootBalance.Keep(rifle, key, 0);
            container.PopulateWithScriptedGear();
            check(container.m_GearToInstantiate.Count == (keep ? 1 : 0), "native scripted generation invokes the rarity patch");

            RewardScope.Enter();
            try { check(LootBalance.Retention(rifle) == 0.01, "restored cache reward scopes retain Interloper rarity"); }
            finally { RewardScope.Leave(); }
            foreach (string difficulty in new[] { "Stalker", "Misery" })
            {
                Game.ExperienceModeManager.s_CurrentGameMode = Game.ExperienceModeManager.GetGameModeFromName(difficulty);
                check(LootBalance.Retention(rifle) == 1, difficulty + " is excluded from loot rarity changes");
            }
        }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode = previous;
            UnityEngine.Object.Destroy(root);
        }
    }
}
