extern alias GameAssembly;
using Game=GameAssembly::Il2Cpp;
using UnityEngine;
using System.Text.Json.Nodes;
using System.Reflection;
using HarmonyLib;
namespace InterloperOverhaul;
internal static class DebugToolsValidation
{
    internal static bool SpawnFixture;
    internal static void Run(Action<bool,string> check)
    {
        check(DebugTools.Rendered,"debug window rendered through the real Unity OnGUI callback at the menu");
        check(!DebugTools.Enabled && !DebugTools.God && !DebugTools.NoWeight,"debug tools remain opt-in after viewing the panel");
        var oldPlayer=Game.GameManager.m_PlayerManager;var oldObject=Game.GameManager.m_PlayerObject;
        var oldInventory=Game.GameManager.m_Inventory;var oldClock=Game.GameManager.m_TimeOfDay;
        var oldHunger=Game.GameManager.m_Hunger;var oldThirst=Game.GameManager.m_Thirst;
        var oldFatigue=Game.GameManager.m_Fatigue;var oldFreezing=Game.GameManager.m_Freezing;
        var root=new GameObject("IO_DebugToolsValidation");root.SetActive(false);
        var items=new List<Game.GearItem>();
        try
        {
            var player=root.AddComponent<Game.PlayerManager>();Game.GameManager.m_PlayerManager=player;Game.GameManager.m_PlayerObject=root;
            var inventory=root.AddComponent<Game.Inventory>();Game.GameManager.m_Inventory=inventory;
            var clock=root.AddComponent<Game.TimeOfDay>();Game.GameManager.m_TimeOfDay=clock;
            Game.GameManager.m_Hunger=root.AddComponent<Game.Hunger>();Game.GameManager.m_Hunger.m_MaxReserveCalories=2500;
            Game.GameManager.m_Thirst=root.AddComponent<Game.Thirst>();Game.GameManager.m_Thirst.m_CurrentThirst=80;
            Game.GameManager.m_Fatigue=root.AddComponent<Game.Fatigue>();Game.GameManager.m_Fatigue.m_CurrentFatigue=80;
            Game.GameManager.m_Freezing=root.AddComponent<Game.Freezing>();Game.GameManager.m_Freezing.m_CurrentFreezing=80;
            DebugTools.Enabled=true;
            check(DebugTools.Active,"debug actions recognize a loaded player without requiring Interloper balance changes");
            player.m_God=false;DebugTools.SetGod(true);
            var save=Game.Utils.SerializeObject(new Game.PlayerManagerSaveDataProxy { m_God=true });
            check(player.m_God && JsonNode.Parse(DebugTools.SavedGod(player,save))["m_God"].GetValue<bool>()==false,
                "native god mode is active but its debug change is removed from the native save payload");
            var health=root.AddComponent<Game.Condition>();health.m_CurrentHP=77;health.m_MaxHP=100;
            health.AddHealth(-50,Game.DamageSource.FrostBite);
            check(health.m_CurrentHP==77,"debug god mode blocks the real native condition-damage entry point");
            DebugTools.SetGod(false);
            check(!player.m_God,"disabling debug god mode restores the player's previous native flag");
            player.m_God=true;DebugTools.SetGod(true);DebugTools.SetGod(false);
            check(player.m_God,"debug god mode preserves a pre-existing native or external god flag");player.m_God=false;
            clock.SetTODLocked(false);DebugTools.SetClock(true);
            var clockSave=Game.Utils.SerializeObject(new Game.TimeOfDaySaveDataProxy { m_LockedTOD=true });
            check(clock.IsTODLocked() && !JsonNode.Parse(DebugTools.SavedClock(clock,clockSave))["m_LockedTOD"].GetValue<bool>(),
                "debug time lock operates natively without persisting its temporary lock");
            DebugTools.RestoreNeeds();
            check(Game.GameManager.m_Thirst.m_CurrentThirst==0 && Game.GameManager.m_Fatigue.m_CurrentFatigue==0 && Game.GameManager.m_Freezing.m_CurrentFreezing==0 &&
                Game.GameManager.m_Hunger.m_CurrentReserveCalories>0,"native hunger, thirst, fatigue, and freezing values can be restored for tests");
            var allocation=FiniteFirearms.State?.Serialize();var learned=Workshop.State.Learned.Count;
            SpawnFixture=true;
            var rifle=DebugTools.Spawn("GEAR_Rifle",0.37f);items.Add(rifle);
            check(rifle.m_InPlayerInventory && Math.Abs(rifle.GetNormalizedCondition()-0.37f)<0.001 && rifle.m_GunItem.NumRoundsInClip()==0,
                "debug spawning adds an empty rifle at the requested condition through the native inventory");
            var variant=DebugTools.Spawn("GEAR_SurvivalKnife",0.81f);items.Add(variant);
            check(variant.m_InPlayerInventory && inventory.m_Items.ToArray().Any(entry=>entry.m_GearItem?.Pointer==variant.Pointer) && LootBalancePolicy.ItemName(variant.name)=="GEAR_SurvivalKnife" &&
                Workshop.State.Learned.Count==learned,"explicit debug variants remain physical items rather than being silently converted into schematics");
            var ruined=DebugTools.Spawn("GEAR_CottonScarf",0);items.Add(ruined);
            check(ruined.m_InPlayerInventory && ruined.CurrentHP==0 && ruined.IsWornOut(),"debug spawning supports ruined clothing for Mending tests");
            check(FiniteFirearms.State?.Serialize()==allocation && !DebugTools.Spawning,"debug spawns preserve world allocations and close their temporary scope");
            SpawnFixture=false;
            inventory.m_TotalWeight=Il2CppTLD.IntBackedUnit.ItemWeight.Zero;
            foreach(var item in items) inventory.m_TotalWeight+=item.GetSingleItemWeightKG();
            var weight=inventory.GetTotalWeightKG();DebugTools.NoWeight=true;
            check(weight>Il2CppTLD.IntBackedUnit.ItemWeight.Zero && inventory.GetTotalWeightKG()==Il2CppTLD.IntBackedUnit.ItemWeight.Zero,
                "debug carry override affects actual native inventory weight without changing item masses");
            DebugTools.NoWeight=false;
            check(inventory.GetTotalWeightKG()==weight,"turning off debug carry override restores native inventory weight");
            DebugTools.SetOpen(true);
            check(Game.InputManager.GetPlayerMovement(player)==Vector2.zero && !Game.InputManager.GetFirePressed(player) && !Game.InputManager.GetInteractPressed(player),
                "open debug panels block native walking, firing, and interaction input");
            DebugTools.Reset();
            check(!DebugTools.Enabled && !DebugTools.Open && !DebugTools.NoWeight && !clock.IsTODLocked(),"debug reset closes the panel and restores temporary world flags");
        }
        finally
        {
            SpawnFixture=false;
            DebugTools.Reset();
            Game.GameManager.m_PlayerManager=oldPlayer;Game.GameManager.m_PlayerObject=oldObject;
            Game.GameManager.m_Inventory=oldInventory;Game.GameManager.m_TimeOfDay=oldClock;
            Game.GameManager.m_Hunger=oldHunger;Game.GameManager.m_Thirst=oldThirst;
            Game.GameManager.m_Fatigue=oldFatigue;Game.GameManager.m_Freezing=oldFreezing;
            foreach(var item in items) if(item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}

// Menu probes have no running survival clock. Match the other item fixtures by
// skipping only clock initialization, while retaining real native ManualStart.
[HarmonyPatch]
internal static class DebugSpawnClockFixture
{
    private static IEnumerable<MethodBase> TargetMethods()=>AccessTools.GetDeclaredMethods(typeof(Game.GearItem)).Where(method=>method.Name=="InstantiateGearItem");
    private static void Postfix(Game.GearItem __result)
    { if(DebugToolsValidation.SpawnFixture && DebugTools.Spawning && __result!=null) __result.m_LastUpdatedTODHoursInitialized=true; }
}
