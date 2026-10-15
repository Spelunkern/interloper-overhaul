extern alias GameAssembly;

using Game = GameAssembly::Il2Cpp;
using Gameplay = GameAssembly::Il2CppTLD.Gameplay;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterloperOverhaul;

// Compiled only with -p:RuntimeValidation=true. Runs at the main menu with no save loaded.
internal static class RuntimeValidation
{
    private static int checks;
    private static bool ran;
    private static void Check(bool result, string name)
    {
        if (!result) throw new System.InvalidOperationException("FAILED: " + name);
        checks++;
        MelonLogger.Msg("[Validation] PASS: " + name);
    }

    internal static System.Collections.IEnumerator WaitForMenu()
    {
        // Wait for the actual visible menu, without synthesizing keyboard/mouse input.
        for (int frame = 0; frame < 1800; frame++)
        {
            foreach (var menu in Resources.FindObjectsOfTypeAll<Game.Panel_MainMenu>())
            {
                if (menu.gameObject.scene.isLoaded && menu.m_MainWindow != null &&
                    menu.m_MainWindow.activeInHierarchy && menu.HasFadedIn() && menu.GetAlpha() >= 0.99f)
                {
                    Run(menu);
                    yield break;
                }
            }
            yield return null;
        }
        MelonLogger.Error("[Validation] FAILURE: the visible main menu was not reached automatically.");
        System.Environment.Exit(1);
    }

    internal static void Run(Game.Panel_MainMenu menu)
    {
        if (ran) return;
        ran = true;
        var previous = Game.ExperienceModeManager.s_CurrentGameMode;
        bool mainMenu = Game.GameManager.s_ActiveIsMainMenu;
        GameObject root = null;
        GameObject houseRoot = null;
        Gameplay.BunkerDistributor distributor = null;
        bool success = false;
        try
        {
            Check(menu.m_MainWindow.activeInHierarchy && menu.HasFadedIn() && menu.GetAlpha() >= 0.99f,
                "visible main menu reached automatically without user input");
            Check(!Game.MoviePlayer.IsPlaying(), "no startup movie playing at the visible main menu");
            foreach (var boot in Resources.FindObjectsOfTypeAll<Game.Panel_Boot>())
            {
                if (!boot.gameObject.scene.isLoaded) continue;
                Check(boot.m_BootFlow == null || !boot.m_BootFlow.activeInHierarchy,
                    "boot logos hidden at the main menu");
                Check(boot.m_MainDisclaimer == null || !boot.m_MainDisclaimer.activeInHierarchy,
                    "main disclaimer hidden without user input");
                Check(!boot.m_HoldLoadingUntilInputGiven, "startup is not waiting for a key press");
            }
            var interloper = Game.ExperienceModeManager.GetGameModeFromName("Interloper").Cast<Gameplay.SandboxConfig>();
            var stalker = Game.ExperienceModeManager.GetGameModeFromName("Stalker").Cast<Gameplay.SandboxConfig>();
            Check(interloper != null && stalker != null, "game mode assets available");
            float lootScale = interloper.m_XPMode.m_GearSpawnChanceScale;
            int emptyChance = interloper.m_XPMode.m_ChanceForEmptyContainer;
            var originalVariants = interloper.m_BunkerSetup.m_BunkerInteriors;
            Game.ExperienceModeManager.s_CurrentGameMode = interloper;
            Check(WorldObjects.IsInterloper, "Interloper scope");
            Game.GameManager.s_ActiveIsMainMenu = false;
            root = new GameObject("IO_Validation");
            root.SetActive(false);
            GameObject Child(string name)
            {
                var go = new GameObject(name);
                go.transform.SetParent(root.transform, false);
                return go;
            }
            var memento = Child("GEAR_VisorNoteCH2");
            var noteEnabler = memento.AddComponent<Game.EnableObjectForXPMode>();
            noteEnabler.m_XPModesToEnable = new Il2CppSystem.Collections.Generic.List<Game.ExperienceModeType>();
            noteEnabler.m_XPModesToEnable.Add(Game.ExperienceModeType.Stalker);
            Check(!noteEnabler.ShouldDisableForCurrentMode(), "memento enabler restored through Harmony");
            var caches = Child("TrackableHiddenCaches");
            var cacheDisabler = caches.AddComponent<Game.DisableObjectForXPMode>();
            cacheDisabler.m_XPModesToDisable = new Il2CppSystem.Collections.Generic.List<Game.ExperienceModeType>();
            cacheDisabler.m_XPModesToDisable.Add(Game.ExperienceModeType.Interloper);
            Check(!cacheDisabler.ShouldDisableForCurrentMode(), "hidden cache group restored through Harmony");
            var gear = Child("GEAR_Rifle");
            gear.AddComponent<Game.GearItem>();
            var gearDisabler = gear.AddComponent<Game.DisableObjectForXPMode>();
            gearDisabler.m_XPModesToDisable = cacheDisabler.m_XPModesToDisable;
            Check(!WorldObjects.IsRestorable(gear), "ordinary gear excluded from restoration");
            var container = Child("CONTAINER_Lockbox");
            container.AddComponent<Game.Container>();
            Check(WorldObjects.IsRestorable(container), "container eligibility");
            var tagDatabase = interloper.m_ActiveTags[0].m_TagDatabase;
            var tagged = container.AddComponent<Gameplay.DisableObjectForGameMode>();
            tagged.m_DisableIfAnyActive = new Il2CppReferenceArray<Il2CppTLD.GameplayTags.GameplayTag>(new[]
            {
                new Il2CppTLD.GameplayTags.GameplayTag("Gameplay.Gear.NoFirearms", tagDatabase)
            });
            Check(!tagged.ShouldDisableForCurrentMode(), "firearm tag does not remove a world container");
            tagged.m_DisableIfAnyActive = new Il2CppReferenceArray<Il2CppTLD.GameplayTags.GameplayTag>(new[]
            {
                new Il2CppTLD.GameplayTags.GameplayTag("Gameplay.Gear.NoFirearms", tagDatabase),
                new Il2CppTLD.GameplayTags.GameplayTag("Gameplay.SafehouseSupported", tagDatabase)
            });
            Check(tagged.ShouldDisableForCurrentMode(), "unrelated active tag remains enforced");
            var group = Child("Containers").AddComponent<Game.RandomSpawnObject>();
            group.m_ObjectList = new Il2CppReferenceArray<GameObject>(new[] { container });
            group.m_NumObjectsToEnableByLevel = new Il2CppStructArray<int>(new[] { 0, 1, 1, 1 });
            Check(group.GetNumObjectsToEnableCurrentXPMode() == 1, "container count restored through Harmony");
            group.m_ObjectList = new Il2CppReferenceArray<GameObject>(new[] { gear });
            Check(group.GetNumObjectsToEnableCurrentXPMode() == 0, "ordinary gear count preserved");
            group.m_ObjectList = new Il2CppReferenceArray<GameObject>(new[] { container, gear });
            Check(group.GetNumObjectsToEnableCurrentXPMode() == 0, "mixed loot group preserved");

            // A synthetic Coastal scene checks the real native random-spawn save loader.
            var coastalScene = SceneManager.CreateScene("CoastalRegion");
            houseRoot = new GameObject("Design");
            houseRoot.SetActive(false);
            SceneManager.MoveGameObjectToScene(houseRoot, coastalScene);
            GameObject HouseChild(string name, Transform parent)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                return go;
            }
            var structures = HouseChild("RandomSpawnStructures", houseRoot.transform);
            var burnedRoot = HouseChild("RandomBurnedHouses", structures.transform);
            var town = HouseChild("TownNorth", burnedRoot.transform);
            var intact = HouseChild("TownNorth_oldsave", town.transform);
            var burned = HouseChild("TownNorth2", town.transform);
            var house = HouseChild("RandomSpawnObjectBase_townNorth", town.transform).AddComponent<Game.RandomSpawnObject>();
            house.m_ObjectList = new Il2CppReferenceArray<GameObject>(new[] { burned });
            house.m_OldSaveObject = intact;
            house.m_Weights = new Il2CppStructArray<int>(new[] { 1 });
            house.m_NumObjectsToEnableByLevel = new Il2CppStructArray<int>(new[] { 0, 1, 1, 1 });
            house.Initialize();
            Check(house.m_ObjectList.Length == 1 && house.m_ObjectList[0] == intact,
                "Coastal Initialize Harmony patch selects authored intact variant");
            Check(house.GetNumObjectsToEnableCurrentXPMode() == 1, "intact house group enabled on Interloper");
            house.LoadObject("TownNorth2", Vector3.zero, 0);
            Check(intact.activeSelf && !burned.activeSelf, "saved burnt selection resolves to intact group without overlap");
            var nightmareGroup = HouseChild("Nightmare_Burned_Buildings", burnedRoot.transform);
            var nightmare = HouseChild(WorldPolicy.NightmareSpawner, nightmareGroup.transform).AddComponent<Game.RandomSpawnObject>();
            var burntNightmare = HouseChild("Nightmare_burnt", nightmareGroup.transform);
            nightmare.m_ObjectList = new Il2CppReferenceArray<GameObject>(new[] { burntNightmare });
            nightmare.m_OldSaveObject = intact; // A real shared-pointer edge case in 2.55.
            nightmare.m_Weights = new Il2CppStructArray<int>(new[] { 1 });
            nightmare.m_NumObjectsToEnableByLevel = new Il2CppStructArray<int>(new[] { 1, 0, 0, 0 });
            nightmare.Initialize();
            nightmare.LoadObject("Nightmare_burnt", Vector3.zero, 0);
            Check(nightmare.GetNumObjectsToEnableCurrentXPMode() == 0 && !burntNightmare.activeSelf,
                "nightmare burnt group stays disabled after loading old selection");
            Check(intact.activeSelf, "nightmare shared legacy pointer does not disable restored house group");

            // The real native distributor operates on a temporary clone, never on a save.
            distributor = UnityEngine.Object.Instantiate(interloper.m_BunkerSetup);
            var cloneVariants = distributor.m_BunkerInteriors;
            distributor.DistributeBunkerInteriors();
            var distribution = distributor.Serialize();
            Check(distribution.Count == 9, "native bunker distributor assigns nine locations");
            int stocked = 0;
            foreach (var entry in distribution)
                if (!entry.m_InteriorId.Contains("Empty") && !entry.m_InteriorId.Contains("Interloper")) stocked++;
            Check(stocked == 3, "normal bunker mix: three furnished, six abandoned");
            Check(distributor.m_BunkerInteriors.Pointer == cloneVariants.Pointer, "bunker specification array restored after generation");
            var roundTrip = UnityEngine.Object.Instantiate(interloper.m_BunkerSetup);
            try
            {
                roundTrip.Deserialize(distribution);
                var loaded = roundTrip.Serialize();
                Check(loaded.Count == distribution.Count, "bunker save round trip count");
                for (int i = 0; i < loaded.Count; i++)
                    Check(loaded[i].m_BunkerId == distribution[i].m_BunkerId &&
                        loaded[i].m_InteriorId == distribution[i].m_InteriorId, "bunker save round trip " + i);
            }
            finally { roundTrip.ClearReplacementData(); UnityEngine.Object.Destroy(roundTrip); }
            var legacy = new Il2CppSystem.Collections.Generic.List<Gameplay.BunkerLocationSaveData>();
            legacy.Add(new Gameplay.BunkerLocationSaveData
            {
                m_BunkerId = distribution[0].m_BunkerId,
                m_InteriorId = originalVariants[0].name
            });
            var oldSave = UnityEngine.Object.Instantiate(interloper.m_BunkerSetup);
            try
            {
                oldSave.Deserialize(legacy);
                var loadedLegacy = oldSave.Serialize();
                Check(loadedLegacy.Count == 1 && loadedLegacy[0].m_InteriorId == originalVariants[0].name,
                    "existing Interloper-only bunker assignment preserved on load");
            }
            finally { oldSave.ClearReplacementData(); UnityEngine.Object.Destroy(oldSave); }
            Check(interloper.m_XPMode.m_GearSpawnChanceScale == lootScale &&
                interloper.m_XPMode.m_ChanceForEmptyContainer == emptyChance, "loot tunables unchanged");
            Game.ExperienceModeManager.s_CurrentGameMode = stalker;
            Check(!WorldObjects.IsInterloper, "Stalker excluded");
            var stalkerDisabler = container.AddComponent<Game.DisableObjectForXPMode>();
            stalkerDisabler.m_XPModesToDisable = new Il2CppSystem.Collections.Generic.List<Game.ExperienceModeType>();
            stalkerDisabler.m_XPModesToDisable.Add(Game.ExperienceModeType.Stalker);
            Check(stalkerDisabler.ShouldDisableForCurrentMode(), "Stalker restriction preserved through Harmony");
            var misery = Game.ExperienceModeManager.GetGameModeFromName("Misery");
            Game.ExperienceModeManager.s_CurrentGameMode = misery;
            Check(!WorldObjects.IsInterloper && noteEnabler.ShouldDisableForCurrentMode(), "Misery memento restriction preserved");
            success = true;
            MelonLogger.Msg($"[Validation] SUCCESS: {checks} runtime checks.");
        }
        catch (System.Exception ex) { MelonLogger.Error("[Validation] FAILURE: " + ex); }
        finally
        {
            if (distributor != null) { distributor.ClearReplacementData(); UnityEngine.Object.Destroy(distributor); }
            if (root != null) UnityEngine.Object.Destroy(root);
            if (houseRoot != null) UnityEngine.Object.Destroy(houseRoot);
            Game.GameManager.s_ActiveIsMainMenu = mainMenu;
            Game.ExperienceModeManager.s_CurrentGameMode = previous;
            // This is an isolated test process at the menu. Exit after restoring state;
            // Unity's shutdown from an injected validation coroutine can otherwise stall.
            System.Environment.Exit(success ? 0 : 1);
        }
    }
}
