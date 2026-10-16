extern alias GameAssembly;

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
            FiniteFirearms.PopulateLoadedScenes();
            spawned.AddRange(Resources.FindObjectsOfTypeAll<Game.GearItem>().Where(gear =>
                (gear.gameObject.scene == rifleScene || gear.gameObject.scene == revolverScene) && FiniteFirearms.IsManaged(gear)));
            check(spawned.Count == 2 && state.RifleCreated && state.RevolverCreated,
                "loaded selected regions create exactly one allocated rifle and one revolver");
            foreach (var gear in spawned)
            {
                string item = LootBalancePolicy.ItemName(gear.name);
                check(FiniteFirearms.IsManaged(gear) && LootBalance.Retention(gear.gameObject) == 1 && gear.m_RolledSpawnChance,
                    "allocated firearm bypasses ordinary spawn suppression: " + item);
                check(Math.Abs(gear.GetNormalizedCondition() * 100 - FirearmSpawnPolicy.Condition(state.Seed, item)) < 0.1,
                    "allocated firearm receives seed-selected initial condition: " + item);
                check(gear.m_GunItem != null && gear.m_GunItem.NumRoundsInClip() == 0 &&
                    gear.m_GunItem.NumSpentCasingsInClip() == 0 && gear.m_GunItem.m_RoundsInClip == 0,
                    "allocated firearm has no live rounds, spent casings, or legacy starting ammunition: " + item);
                gear.m_LastUpdatedTODHoursInitialized = true; // Menu fixture has no running survival clock.
                gear.ManualStart();
                check(gear.gameObject.activeSelf && Math.Abs(gear.GetNormalizedCondition() * 100 -
                    FirearmSpawnPolicy.Condition(state.Seed, item)) < 0.1, "native initialization preserves allocated condition and firearm: " + item);
            }
            FiniteFirearms.PopulateLoadedScenes();
            check(Resources.FindObjectsOfTypeAll<Game.GearItem>().Count(gear =>
                (gear.gameObject.scene == rifleScene || gear.gameObject.scene == revolverScene) && FiniteFirearms.IsManaged(gear)) == 2,
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
                check(scenePayload.Contains(state.GuidFor("GEAR_Rifle")) && scenePayload.Contains(state.GuidFor("GEAR_Revolver")),
                    "native world gear serialization includes both allocated firearm identities");
            }
            finally { Game.GearManager.m_Gear = previousGearList; }

            check(FiniteFirearms.WriteSlot(slot), "issued firearm flags saved in the same native slot snapshot");
            FiniteFirearms.State = null;
            FiniteFirearms.ReadSlot(slot);
            check(FiniteFirearms.State.RifleCreated && FiniteFirearms.State.RevolverCreated && !FiniteFirearms.State.Legacy,
                "loading saved allocation preserves both issuance flags");
            FiniteFirearms.PopulateLoadedScenes();
            check(Resources.FindObjectsOfTypeAll<Game.GearItem>().Count(gear =>
                (gear.gameObject.scene == rifleScene || gear.gameObject.scene == revolverScene) && FiniteFirearms.IsManaged(gear)) == 2,
                "restored issuance prevents replacement after scene reload");

            var original = spawned[0];
            var originalGun = original.m_GunItem;
            originalGun.AddRoundsToClip(1, 100);
            original.SetNormalizedHP(0.37f, false);
            var proxy = original.Serialize();
            string originalName = LootBalancePolicy.ItemName(original.name);
            UnityEngine.Object.DestroyImmediate(original.gameObject);
            var restored = Game.GearItem.InstantiateAndDeserializeGearItem(originalName, proxy);
            spawned.Add(restored);
            check(Math.Abs(restored.GetNormalizedCondition() - 0.37f) < 0.001f && restored.m_GunItem.NumRoundsInClip() == 1,
                "native gear serialization preserves later damage and player-loaded ammunition");
            restored.m_LastUpdatedTODHoursInitialized = true;
            restored.ManualStart();
            check(restored.m_GunItem.NumRoundsInClip() == 1 && Math.Abs(restored.GetNormalizedCondition() - 0.37f) < 0.001f,
                "loading an acquired firearm does not empty or reroll it");

            var legacy = new Game.SlotData();
            check(Game.SaveGameSlots.SaveDataToSlot(legacy, Game.SlotData.GLOBAL_KEY, (Il2CppSystem.String)"validation-global"),
                "legacy save fixture created in memory");
            FiniteFirearms.ReadSlot(legacy);
            check(FiniteFirearms.State.Legacy, "pre-allocation saves preserve existing guns without allocating extras");

            Trader.TradeExchangeData Exchange(string guid) => new()
            {
                m_GainedItem = new Gear.AssetReferenceGearItemData(guid), m_GainedItemAmount = 1
            };
            var rifleOffer = Exchange("b9474d5e3b25bbe4e8c2f3772da8cdab");
            var ammoOffer = Exchange("b077bbe3815612345a39da9a3518c642");
            var casingOffer = Exchange("e5ef5c87193d18f45b64961da0adc56d");
            var knifeOffer = Exchange("5a7d6ed7004a60c49878b62497f2a63f");
            var storyOffer = Exchange("b077bbe3815612345a39da9a3518c642");
            storyOffer.m_IsSpecialRequest = true;
            check(FirearmSpawnPolicy.IsStandardFirearm(LootBalancePolicy.TraderItem(rifleOffer.m_GainedItem.AssetGUID)) &&
                !rifleOffer.IsValidInCurrentGamemode(), "native trader eligibility excludes additional standard rifles");
            int aurora = Enumerable.Range(0, 10000).First(n => !LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoBox", n) &&
                !LootBalancePolicy.KeepTraderSupply(123, "GEAR_RifleAmmoCasing", n));
            var stock = new Il2CppSystem.Collections.Generic.List<Trader.TradeExchangeData>();
            foreach (var offer in new[] { rifleOffer, ammoOffer, casingOffer, knifeOffer, storyOffer }) stock.Add(offer);
            TraderFirearmSupplies.Filter(stock, 123, aurora);
            check(stock.Count == 2 && stock[0].Pointer == knifeOffer.Pointer && stock[1].Pointer == storyOffer.Pointer,
                "trader scarcity filters normal guns and firearm supplies while preserving other stock and story rewards");
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
