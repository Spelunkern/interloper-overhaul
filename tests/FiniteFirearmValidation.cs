extern alias GameAssembly;

using System.Text.Json.Nodes;
using Game = GameAssembly::Il2Cpp;
using Trader = GameAssembly::Il2CppTLD.Trader;
using Gear = GameAssembly::Il2CppTLD.Gear;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterloperOverhaul;

internal static class FiniteFirearmValidation
{
    internal static void Run(System.Action<bool, string> check)
    {
        var previous = FiniteFirearms.State;
        var spawned = new List<Game.GearItem>();
        try
        {
            var state = FirearmRunState.Create(12345, "Validation/12345");
            state.RifleSite = FirearmSpawnPolicy.Sites.First(site => site.Item == "GEAR_Rifle" && site.Scene == "RuralRegion_SANDBOX").Id;
            state.RevolverSite = FirearmSpawnPolicy.Sites.First(site => site.Scene == "LakeRegion_SANDBOX").Id;
            state.KnifeSite = FirearmSpawnPolicy.Sites.First(site => site.Item == "GEAR_Knife" && site.Scene == "LakeRegion_SANDBOX").Id;
            state.HatchetSite = FirearmSpawnPolicy.Sites.First(site => site.Item == "GEAR_Hatchet" && site.Scene == "LakeRegion_SANDBOX").Id;
            state.LightsLegacy=true; // Flashlights have their own native fixture.
            FiniteFirearms.State = state;
            var slot = new Game.SlotData();
            check(FiniteFirearms.WriteSlot(slot) && slot.HasData(FiniteFirearms.SaveKey),
                "finite firearm plan stored in the native save slot without disk writes");
            check(FirearmRunState.Deserialize(System.Text.Encoding.UTF8.GetString(
                slot.m_Dict[FiniteFirearms.SaveKey].ToArray())).RifleSite == state.RifleSite,
                "opaque native save-slot payload round trip preserves allocation");
            check(Game.Utils.SerializeObject(slot).Contains(FiniteFirearms.SaveKey),
                "native whole-slot serialization retains the allocation payload key");

            var rifleScene = SceneManager.CreateScene("RuralRegion_SANDBOX");
            var revolverScene = SceneManager.CreateScene("LakeRegion_SANDBOX");
            var scenes=new List<Scene> { rifleScene,revolverScene };
            foreach(var allocation in state.Additional)
            {
                var site=state.Site(allocation.Item,1);
                var scene=SceneManager.GetSceneByName(site.Scene);
                if(!scene.IsValid()) scene=SceneManager.CreateScene(site.Scene);
                if(!scenes.Contains(scene)) scenes.Add(scene);
            }
            bool ManagedEquipment(Game.GearItem gear)=>FirearmSpawnPolicy.Items.Contains(LootBalancePolicy.ItemName(gear.name)) &&
                scenes.Contains(gear.gameObject.scene) && FiniteFirearms.IsManaged(gear);
            FiniteFirearms.PopulateLoadedScenes();
            spawned.AddRange(Resources.FindObjectsOfTypeAll<Game.GearItem>().Where(gear =>
                ManagedEquipment(gear)));
            check(spawned.Count==8 && FirearmSpawnPolicy.Items.All(item=>Enumerable.Range(0,2).All(copy=>state.Created(item,copy))) &&
                FirearmSpawnPolicy.Items.All(item=>spawned.Count(gear=>LootBalancePolicy.ItemName(gear.name)==item)==2),
                "loaded selected regions create exactly two rifles, revolvers, hunting knives, and hatchets");
            foreach (var gear in spawned)
            {
                string item = LootBalancePolicy.ItemName(gear.name);
                int copy=Workshop.GuidOf(gear)==state.GuidFor(item,1) ? 1 : 0;
                check(FiniteFirearms.IsManaged(gear) && LootBalance.Retention(gear.gameObject) == 1 && gear.m_RolledSpawnChance,
                    "allocated firearm bypasses ordinary spawn suppression: " + item);
                check(Math.Abs(gear.GetNormalizedCondition() * 100 - FirearmSpawnPolicy.Condition(state.Seed,item,copy)) < 0.1,
                    "allocated firearm receives seed-selected initial condition: " + item);
                check(!FirearmSpawnPolicy.IsStandardFirearm(item) || (gear.m_GunItem != null && gear.m_GunItem.NumRoundsInClip() == 0 &&
                    gear.m_GunItem.NumSpentCasingsInClip() == 0 && gear.m_GunItem.m_RoundsInClip == 0),
                    "allocated firearm has no live rounds, spent casings, or legacy starting ammunition: " + item);
                gear.m_LastUpdatedTODHoursInitialized = true; // Menu fixture has no running survival clock.
                gear.ManualStart();
                check(gear.gameObject.activeSelf && Math.Abs(gear.GetNormalizedCondition() * 100 -
                    FirearmSpawnPolicy.Condition(state.Seed,item,copy)) < 0.1, "native initialization preserves allocated condition and firearm: " + item);
            }
            FiniteFirearms.PopulateLoadedScenes();
            check(Resources.FindObjectsOfTypeAll<Game.GearItem>().Count(gear =>
                ManagedEquipment(gear)) == 8,
                "repeated region population does not create additional firearms");
            bool Registered(Game.GearItem gear)
            {
                foreach (var active in Game.GearManager.m_Gear)
                    if (active.Pointer == gear.Pointer) return true;
                return false;
            }
            check(spawned.All(Registered),
                "allocated guns are registered in the native world gear manager");
            var previousGearList = Game.GearManager.m_Gear;
            Game.GearManager.m_Gear = new Il2CppSystem.Collections.Generic.List<Game.GearItem>();
            foreach (var gear in spawned) Game.GearManager.m_Gear.Add(gear);
            try
            {
                string scenePayload = Game.GearManager.Serialize();
                check(FirearmSpawnPolicy.Items.All(item=>Enumerable.Range(0,2).All(copy=>scenePayload.Contains(state.GuidFor(item,copy)))),
                    "native world gear serialization includes all eight distinct allocated identities");
            }
            finally { Game.GearManager.m_Gear = previousGearList; }

            check(FiniteFirearms.WriteSlot(slot), "issued firearm flags saved in the same native slot snapshot");
            FiniteFirearms.State = null;
            FiniteFirearms.ReadSlot(slot);
            check(FirearmSpawnPolicy.Items.All(item=>Enumerable.Range(0,2).All(copy=>FiniteFirearms.State.Created(item,copy))) && !FiniteFirearms.State.Legacy,
                "loading saved allocation preserves all eight independent issuance flags");
            FiniteFirearms.PopulateLoadedScenes();
            check(Resources.FindObjectsOfTypeAll<Game.GearItem>().Count(gear =>
                ManagedEquipment(gear)) == 8,
                "restored issuance prevents replacement after scene reload");

            foreach(var original in spawned.Where(gear=>FirearmSpawnPolicy.IsStandardFirearm(gear.name)).ToArray())
            {
                var originalGun = original.m_GunItem;
                originalGun.AddRoundsToClip(1, 100);
                original.SetNormalizedHP(0.37f, false);
                var proxy = original.Serialize();
                string originalName = LootBalancePolicy.ItemName(original.name);
                var savedScene=original.gameObject.scene;
                UnityEngine.Object.DestroyImmediate(original.gameObject);
                var restored = Game.GearItem.InstantiateAndDeserializeGearItem(originalName, proxy);
                SceneManager.MoveGameObjectToScene(restored.gameObject,savedScene);
                spawned.Add(restored);
                check(FiniteFirearms.IsManaged(restored) && Math.Abs(restored.GetNormalizedCondition() - 0.37f) < 0.001f && restored.m_GunItem.NumRoundsInClip() == 1,
                    "native gear serialization preserves later damage and player-loaded ammunition");
                restored.m_LastUpdatedTODHoursInitialized = true;
                restored.ManualStart();
                check(restored.m_GunItem.NumRoundsInClip() == 1 && Math.Abs(restored.GetNormalizedCondition() - 0.37f) < 0.001f,
                    "loading an acquired firearm does not empty or reroll it");

            }
            foreach (string item in new[] { "GEAR_Knife", "GEAR_Hatchet" })
            foreach(int copy in Enumerable.Range(0,2))
            {
                var tool = spawned.Single(gear => gear != null && Workshop.GuidOf(gear)==state.GuidFor(item,copy));
                tool.SetNormalizedHP(0.37f, false);
                var toolProxy = tool.Serialize();
                var savedScene=tool.gameObject.scene;
                UnityEngine.Object.DestroyImmediate(tool.gameObject);
                var restoredTool = Game.GearItem.InstantiateAndDeserializeGearItem(item, toolProxy);
                SceneManager.MoveGameObjectToScene(restoredTool.gameObject,savedScene);
                spawned.Add(restoredTool);
                restoredTool.m_LastUpdatedTODHoursInitialized = true;
                restoredTool.ManualStart();
                check(restoredTool.gameObject.activeSelf && FiniteFirearms.IsManaged(restoredTool) &&
                    Math.Abs(restoredTool.GetNormalizedCondition() - 0.37f) < 0.001f,
                    "native tool serialization preserves managed identity and player wear: " + item);
            }

            // A version-three save knows the first copies, but has never issued
            // the second copies. Simulate that world without opening a real save.
            var oldPlan=JsonNode.Parse(state.Serialize());oldPlan["Version"]=3;
            oldPlan.AsObject().Remove("Additional");
            foreach(var gear in spawned.Where(gear=>gear!=null && FirearmSpawnPolicy.Items.Contains(LootBalancePolicy.ItemName(gear.name))).ToArray())
                if(Workshop.GuidOf(gear)==state.GuidFor(LootBalancePolicy.ItemName(gear.name),1) || Workshop.GuidOf(gear)==state.GuidFor("GEAR_Rifle"))
                    UnityEngine.Object.DestroyImmediate(gear.gameObject);
            slot.m_Dict[FiniteFirearms.SaveKey]=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>(System.Text.Encoding.UTF8.GetBytes(oldPlan.ToJsonString()));
            FiniteFirearms.ReadSlot(slot);
            check(FiniteFirearms.State.Version==4 && FirearmSpawnPolicy.Items.All(FiniteFirearms.State.Created) &&
                FiniteFirearms.State.Additional.All(allocation=>!allocation.Created),
                "native old-slot migration retains first-copy issuance and adds only unissued second copies");
            FiniteFirearms.PopulateLoadedScenes();
            spawned.AddRange(Resources.FindObjectsOfTypeAll<Game.GearItem>().Where(ManagedEquipment));
            check(Resources.FindObjectsOfTypeAll<Game.GearItem>().Count(ManagedEquipment)==7 &&
                FirearmSpawnPolicy.Items.All(item=>FiniteFirearms.State.Created(item,1)) &&
                !Resources.FindObjectsOfTypeAll<Game.GearItem>().Any(gear=>gear!=null && Workshop.GuidOf(gear)==state.GuidFor("GEAR_Rifle")),
                "migration issues one new copy per family without replacing a lost first-copy rifle");
            FiniteFirearms.WriteSlot(slot);FiniteFirearms.ReadSlot(slot);FiniteFirearms.PopulateLoadedScenes();
            check(Resources.FindObjectsOfTypeAll<Game.GearItem>().Count(ManagedEquipment)==7,
                "migrated second-copy issuance survives save/reload without duplicates");

            var legacy = new Game.SlotData();
            check(Game.SaveGameSlots.SaveDataToSlot(legacy, Game.SlotData.GLOBAL_KEY, (Il2CppSystem.String)"validation-global"),
                "legacy save fixture created in memory");
            FiniteFirearms.ReadSlot(legacy);
            check(FiniteFirearms.State.Legacy, "pre-allocation saves preserve existing guns without allocating extras");

            foreach (string item in new[] { "GEAR_Knife", "GEAR_Hatchet" })
            {
                var extraTool = Game.GearItem.InstantiateGearItem(item);
                spawned.Add(extraTool);
                extraTool.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
                extraTool.m_LastUpdatedTODHoursInitialized = true;
                extraTool.ManualStart();
                check(!extraTool.gameObject.activeSelf,
                    "even legacy runs cannot obtain newly generated guaranteed tools: " + item);
            }

            Trader.TradeExchangeData Exchange(string guid) => new()
            {
                m_GainedItem = new Gear.AssetReferenceGearItemData(guid), m_GainedItemAmount = 1
            };
            var rifleOffer = Exchange("b9474d5e3b25bbe4e8c2f3772da8cdab");
            var variantOffer = Exchange("b930bfe9aa379ec4885009a692cfea2e");
            var specialVariantOffer = Exchange("96daa7434362e754fb7bd4fbb24c8cd4");
            specialVariantOffer.m_IsSpecialRequest = true;
            var interloperMode = Game.ExperienceModeManager.s_CurrentGameMode;
            Game.ExperienceModeManager.s_CurrentGameMode = Game.ExperienceModeManager.GetGameModeFromName("Stalker");
            bool nativeVariantEligibility = variantOffer.IsValidInCurrentGamemode();
            Game.ExperienceModeManager.s_CurrentGameMode = interloperMode;
            check(variantOffer.IsValidInCurrentGamemode() == nativeVariantEligibility, "schematic trades preserve native Stalker eligibility checks");
            var ammoOffer = Exchange("b077bbe3815612345a39da9a3518c642");
            var casingOffer = Exchange("e5ef5c87193d18f45b64961da0adc56d");
            var knifeOffer = Exchange("5a7d6ed7004a60c49878b62497f2a63f");
            var hatchetOffer = Exchange("0697541dfa8cd2644931091463697794");
            hatchetOffer.m_IsSpecialRequest = true;
            var otherOffer = Exchange("00000000000000000000000000000000");
            check(!knifeOffer.IsValidInCurrentGamemode() && !hatchetOffer.IsValidInCurrentGamemode(),
                "native trader eligibility excludes additional hunting knives and hatchets");
            var storyOffer = Exchange("b077bbe3815612345a39da9a3518c642");
            storyOffer.m_IsSpecialRequest = true;
            check(FirearmSpawnPolicy.IsStandardFirearm(LootBalancePolicy.TraderItem(rifleOffer.m_GainedItem.AssetGUID)) &&
                !rifleOffer.IsValidInCurrentGamemode(), "native trader eligibility excludes additional standard rifles");
            int aurora = Enumerable.Range(0, 10000).First(n => !LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoBox", n) &&
                !LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoCasing", n));
            var stock = new Il2CppSystem.Collections.Generic.List<Trader.TradeExchangeData>();
            foreach (var offer in new[] { rifleOffer, variantOffer, specialVariantOffer, ammoOffer, casingOffer, knifeOffer, hatchetOffer, otherOffer, storyOffer }) stock.Add(offer);
            TraderFirearmSupplies.Filter(stock, 123, aurora);
            check(stock.Count == 4 && stock[0].Pointer == variantOffer.Pointer && stock[1].Pointer == specialVariantOffer.Pointer && stock[2].Pointer == otherOffer.Pointer && stock[3].Pointer == storyOffer.Pointer,
                "trader preserves schematic trades and special requests while filtering extra guns and scarce supplies");
            int rareAurora = Enumerable.Range(0, 100000).First(n => LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoBox", n));
            stock.Clear(); stock.Add(ammoOffer);
            TraderFirearmSupplies.Filter(stock, 123, rareAurora);
            check(stock.Count == 1, "rare trader auroras can still offer ammunition");

            RecoveredCasings.Prefix(out bool discharge);
            Game.GearItem recovered;
            try { recovered = Game.GearItem.InstantiateGearItem("GEAR_RifleAmmoCasing"); }
            finally { RecoveredCasings.Finalizer(discharge); }
            spawned.Add(recovered);
            check(recovered.m_HasBeenOwnedByPlayer && recovered.m_RolledSpawnChance && LootBalance.Retention(recovered.gameObject) == 1,
                "casings created during native shell discharge survive deferred gear initialization");
            check(!RecoveredCasings.Active, "shell discharge scope ends without affecting found-casing rarity");
        }
        finally
        {
            foreach (var gear in spawned) if (gear != null) UnityEngine.Object.Destroy(gear.gameObject);
            FiniteFirearms.Reset();
            FiniteFirearms.State = previous;
        }
    }
}
