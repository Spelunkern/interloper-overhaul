extern alias GameAssembly;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
using UnityEngine;
using MelonLoader;

namespace InterloperOverhaul;

// Opt-in, session-only tools. Inventory edits and teleports use native save behavior.
internal static class DebugTools
{
    internal static bool Open,Enabled,God,Needs,NoWeight,FreezeClock;
    [ThreadStatic] internal static bool Spawning;
    internal static bool Rendered;
    private static bool ownsFlight,oldGod,oldClock,oldMoveBlock,oldActionBlock;
    private static Game.PlayerManager godOwner;
    private static Game.TimeOfDay clockOwner;
    private static CursorLockMode oldCursorLock;
    private static bool oldCursorVisible;
    private static float nextNeeds;
    private static float inputReleaseAt;
    internal static bool BlocksInput=>Open || Time.unscaledTime<inputReleaseAt;
    private static Rect window=new(50,50,740,620);
    private static readonly GUI.WindowFunction drawWindow=new Action<int>(DrawWindow);
    private static int tab,page;
    private static string search="",itemName="GEAR_HeavyParka",count="1",condition="100",hour="12";
    private static string status="F8: panel. F9: start flight / return. Tools reset when loading another world.";
    private static Vector3 bookmark;
    private static Quaternion bookmarkRotation;
    private static string bookmarkScene="";
    private static string sessionScene="";
    private static int sessionPlayer;
    private static readonly string[] catalog=BuildCatalog();

    internal static bool Ready=>!Game.GameManager.s_ActiveIsMainMenu && Game.GameManager.m_PlayerManager!=null &&
        Game.GameManager.GetPlayerObject()!=null && !Game.SaveGameSystem.IsRestoreInProgress() && !Game.SaveGameSystem.IsSceneRestoreInProgress() &&
        Game.GameManager.GetPlayerManagerComponent().GetControlMode()!=Game.PlayerControlMode.Dead;
    internal static bool Active=>Enabled && Ready;
    internal static bool Flying=>ownsFlight && Game.CameraDebugMode.s_Mode==Game.CameraDebugMode.Mode.Fly;
    private static string SceneName=>Game.GameManager.m_ActiveScene ?? "";

    internal static void SetOpen(bool value)
    {
        if(Open==value) return;
        if(value)
        {
            oldCursorLock=Cursor.lockState;oldCursorVisible=Cursor.visible;
            oldMoveBlock=Game.GameManager.m_BlockMoveInputUntilReleased;
            oldActionBlock=Game.GameManager.m_BlockNonMovementInput;
            Game.GameManager.m_BlockMoveInputUntilReleased=true;
            Game.GameManager.m_BlockNonMovementInput=true;
        }
        else
        {
            Game.GameManager.m_BlockMoveInputUntilReleased=oldMoveBlock;
            Game.GameManager.m_BlockNonMovementInput=oldActionBlock;
            Cursor.lockState=oldCursorLock;Cursor.visible=oldCursorVisible;
            GUIUtility.keyboardControl=0;
            inputReleaseAt=Time.unscaledTime+0.15f;
        }
        Open=value;
    }

    internal static void Update()
    {
        if(Input.GetKeyDown(KeyCode.F8)) SetOpen(!Open);
        if(Open && Input.GetKeyDown(KeyCode.Escape)) SetOpen(false);
        if(!Ready)
        {
            if(Enabled) Reset();
            return;
        }
        if(Open) { Game.GameManager.m_BlockMoveInputUntilReleased=true;Game.GameManager.m_BlockNonMovementInput=true; }
        if(!Enabled) return;
        if(sessionScene.Length>0 && (SceneName!=sessionScene || Game.GameManager.GetPlayerManagerComponent().GetInstanceID()!=sessionPlayer))
        { Reset();return; }
        if(Input.GetKeyDown(KeyCode.F9)) Run(ToggleFlight);
        if(godOwner!=null) godOwner.m_God=God || Flying || oldGod;
        if(FreezeClock && clockOwner!=null) clockOwner.SetTODLocked(true);
        if(Needs && Time.unscaledTime>=nextNeeds)
        {
            nextNeeds=Time.unscaledTime+0.25f;
            RestoreNeeds();
        }
        if(ownsFlight && !Flying) { ownsFlight=false;SetGod(God); } // Native cancel/accept may end it.
    }

    internal static void LateUpdate()
    {
        if(!Open) return;
        Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
    }

    internal static void Reset()
    {
        SetOpen(false);
        Restore(()=> { if(Flying) Game.FlyMode.Exit(); });
        Restore(()=> { if(godOwner!=null) godOwner.m_God=oldGod; });
        Restore(()=> { if(clockOwner!=null) clockOwner.SetTODLocked(oldClock); });
        Enabled=God=Needs=NoWeight=FreezeClock=ownsFlight=false;
        godOwner=null;clockOwner=null;bookmarkScene="";sessionScene="";sessionPlayer=0;
        status="Debug tools disabled. Temporary session flags restored.";
    }
    internal static void SceneUnloaded(string name)
    {
        // Region streaming may unload additive scenery while flying. Only the
        // active world transition (or leaving the menu) ends the debug session.
        if((Enabled && name==sessionScene) || name.StartsWith("MainMenu",StringComparison.Ordinal)) Reset();
    }
    private static void Restore(Action action)
    { try { action(); } catch(Exception ex) { MelonLogger.Warning("[Debug reset] "+ex.Message); } }

    internal static void SetGod(bool value)
    {
        if(value && godOwner==null) { godOwner=Game.GameManager.GetPlayerManagerComponent();oldGod=godOwner.m_God; }
        God=value;
        if(godOwner!=null) godOwner.m_God=value || Flying || oldGod;
        if(!value && !Flying) godOwner=null;
    }
    internal static void SetClock(bool value)
    {
        if(value && clockOwner==null) { clockOwner=Game.GameManager.GetTimeOfDayComponent();oldClock=clockOwner.IsTODLocked(); }
        FreezeClock=value;
        if(clockOwner!=null) clockOwner.SetTODLocked(value || oldClock);
        if(!value) clockOwner=null;
    }
    internal static string SavedGod(Game.PlayerManager player,string json)=>godOwner!=null && player.Pointer==godOwner.Pointer ?
        MaskSavedFlag(json,"m_God",oldGod) : json;
    internal static string SavedClock(Game.TimeOfDay clock,string json)=>FreezeClock && clockOwner!=null && clock.Pointer==clockOwner.Pointer ?
        MaskSavedFlag(json,"m_LockedTOD",oldClock) : json;
    internal static string MaskSavedFlag(string json,string key,bool original)
    {
        if(string.IsNullOrEmpty(json)) return json;
        var record=JsonNode.Parse(json);record[key]=original;return record.ToJsonString();
    }
    internal static void RestoreNeeds()
    {
        var hunger=Game.GameManager.GetHungerComponent();if(hunger!=null) hunger.m_CurrentReserveCalories=hunger.GetAdjustedMaxReserveCalories();
        var thirst=Game.GameManager.GetThirstComponent();if(thirst!=null) thirst.m_CurrentThirst=0;
        var fatigue=Game.GameManager.GetFatigueComponent();if(fatigue!=null) fatigue.m_CurrentFatigue=0;
        var freezing=Game.GameManager.GetFreezingComponent();if(freezing!=null) freezing.m_CurrentFreezing=0;
    }
    internal static void Heal()
    {
        RestoreNeeds();
        var health=Game.GameManager.GetConditionComponent();
        if(health!=null) health.m_CurrentHP=health.GetAdjustedMaxHP();
        status="Condition and needs restored. Active afflictions are unchanged.";
    }
    private static void TreatOverhaulAfflictions()
    {
        var frost=Game.GameManager.GetFrostbiteComponent();
        if(frost!=null)
        {
            frost.m_LocationsWithActiveFrostbite.Clear();frost.m_LocationsWithFrostbiteRisk.Clear();
            for(int index=0;index<frost.m_LocationsCurrentFrostbiteDamage.Count;index++) frost.m_LocationsCurrentFrostbiteDamage[index]=0;
        }
        var parasites=Game.GameManager.GetIntestinalParasitesComponent();
        if(parasites!=null && parasites.m_HasParasites) parasites.IntestinalParasitesEnd();
        status="Frostbite and parasites cleared. Other afflictions are unchanged.";
    }
    internal static void ToggleFlight()
    {
        if(Flying)
        {
            Game.FlyMode.Exit();ownsFlight=false;SetGod(God);status="Flight ended; returned to the starting point.";
        }
        else
        {
            if(Game.CameraDebugMode.s_Mode!=Game.CameraDebugMode.Mode.Off || Game.GameManager.GetPlayerManagerComponent().GetControlMode()!=Game.PlayerControlMode.Normal)
                throw new InvalidOperationException("Finish the current interaction before starting flight.");
            SetOpen(false);
            if(godOwner==null) { godOwner=Game.GameManager.GetPlayerManagerComponent();oldGod=godOwner.m_God; }
            godOwner.m_God=true;
            try { Game.FlyMode.Enter(); }
            finally { ownsFlight=Game.CameraDebugMode.s_Mode==Game.CameraDebugMode.Mode.Fly;if(!ownsFlight) SetGod(God); }
            if(!ownsFlight) { SetGod(God);throw new InvalidOperationException("Native flight mode did not activate."); }
            status="Flight active. WASD / mouse; E up, Ctrl or Q down. F8 for landing.";
        }
    }
    private static void Land()
    {
        if(!Flying) return;
        SetOpen(false);Game.FlyMode.TeleportPlayerAndExit();ownsFlight=false;SetGod(God);
        status="Native flight landing requested.";
    }
    internal static Game.GearItem Spawn(string name,float health)
    {
        name=LootBalancePolicy.ItemName(name);
        if(Game.GearItem.LoadGearItemPrefab(name)==null) throw new ArgumentException("Unknown item ID: "+name);
        bool previous=Spawning;Spawning=true;
        Game.GearItem gear=null;
        try
        {
            gear=Game.GearItem.InstantiateGearItem(name);
            gear.SkipSpawnChanceRollInitialDecayAndAutoEvolve();gear.m_HasBeenOwnedByPlayer=true;
            gear.ManualStart();gear.SetNormalizedHP(Math.Clamp(health,0,1),false);
            if(health<=0) gear.m_WornOut=true;
            FiniteFirearms.Empty(gear);
            Game.GameManager.GetInventoryComponent().AddGear(gear);
            return gear;
        }
        catch { if(gear!=null && !gear.m_InPlayerInventory) UnityEngine.Object.Destroy(gear.gameObject);throw; }
        finally { Spawning=previous; }
    }
    private static void SpawnSelection()
    {
        if(!int.TryParse(count,out int quantity) || quantity<1 || quantity>20) throw new ArgumentException("Count must be 1 to 20.");
        if(!float.TryParse(condition,NumberStyles.Float,CultureInfo.InvariantCulture,out float hp) || !float.IsFinite(hp) || hp<0 || hp>100)
            throw new ArgumentException("Condition must be 0 to 100.");
        for(int index=0;index<quantity;index++) Spawn(itemName,hp/100);
        status=$"Added {quantity} x {itemName}. World allocations were not consumed.";
    }
    private static void LearnSchematic()
    {
        if(Workshop.Failed) throw new InvalidOperationException("Workshop progress could not be loaded; schematic edits are disabled.");
        var variant=VariantPolicy.Find(itemName);
        if(variant==null) throw new ArgumentException("Select a weapon variant to learn its schematic.");
        Workshop.LearnVariant(variant);status="Learned: "+variant.Title;
    }
    private static void Run(Action action)
    {
        if(!Active) return;
        try { action(); }
        catch(Exception ex) { status=ex.Message;MelonLogger.Warning("[Debug tools] "+ex.Message); }
    }

    internal static void Draw()
    {
        if(!Open) return;
        var color=GUI.color;bool guiEnabled=GUI.enabled;int depth=GUI.depth;var matrix=GUI.matrix;
        try
        {
            GUI.color=Color.white;GUI.enabled=true;GUI.depth=-100;
            float scale=Mathf.Min(1,Mathf.Min(Screen.width/780f,Screen.height/660f));
            GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            window.x=Mathf.Clamp(window.x,0,Mathf.Max(0,Screen.width/scale-window.width));
            window.y=Mathf.Clamp(window.y,0,Mathf.Max(0,Screen.height/scale-window.height));
            window=GUI.Window(210021,window,drawWindow,"Interloper Overhaul | Debug tools");
            Rendered=true;
        }
        finally { GUI.color=color;GUI.enabled=guiEnabled;GUI.depth=depth;GUI.matrix=matrix; }
    }
    private static bool Button(float x,float y,float width,string text)=>GUI.Button(new Rect(x,y,width,30),text);
    private static void Label(float x,float y,float width,string text)=>GUI.Label(new Rect(x,y,width,42),text);
    private static void DrawWindow(int id)
    {
        if(Button(650,25,75,"Close")) SetOpen(false);
        GUI.enabled=Ready;
        bool enabled=GUI.Toggle(new Rect(15,32,600,25),Enabled,"Enable test tools for this session");
        if(enabled!=Enabled)
        {
            if(!enabled) { Reset();SetOpen(true); }
            else { Enabled=true;sessionScene=SceneName;sessionPlayer=Game.GameManager.GetPlayerManagerComponent().GetInstanceID(); }
        }
        GUI.enabled=true;
        Label(15,64,710,Ready ? $"{SceneName} | Testing changes to items and position can be saved by the game." : "Load a game to use test tools. Nothing is enabled automatically.");
        if(Button(15,110,225,"Player / flight")) tab=0;
        if(Button(255,110,225,"Items / schematics")) tab=1;
        if(Button(495,110,225,"World / diagnostics")) tab=2;
        GUI.enabled=Active;
        if(tab==0) DrawPlayer();else if(tab==1) DrawItems();else DrawWorld();
        GUI.enabled=true;
        Label(15,550,710,status);
        Label(15,590,710,"F8 panel  |  F9 flight / return  |  No debug options are stored in saves.");
        GUI.DragWindow(new Rect(0,0,740,23));
    }
    private static void DrawPlayer()
    {
        bool god=GUI.Toggle(new Rect(20,158,330,26),God,"God mode (native invulnerability)");
        if(god!=God) Run(()=>SetGod(god));
        Needs=GUI.Toggle(new Rect(380,158,330,26),Needs,"Keep hunger, thirst, warmth and rest full");
        NoWeight=GUI.Toggle(new Rect(20,195,330,26),NoWeight,"Ignore carried weight for testing");
        if(Button(20,240,330,"Restore condition and needs")) Run(Heal);
        if(Button(380,240,330,"Treat frostbite and parasites")) Run(TreatOverhaulAfflictions);
        if(Button(20,290,330,Flying ? "End flight / return to start" : "Start native flight")) Run(ToggleFlight);
        GUI.enabled=Active && Flying;
        if(Button(380,290,330,"Land at flight camera")) Run(Land);
        GUI.enabled=Active;
        Label(20,342,690,"Flight: WASD + mouse. E ascends, Ctrl/Q descend.\nF9 returns to the start. Use the landing button to move the survivor.");
        Label(20,405,690,"Flight temporarily protects the survivor. Landing uses native placement checks.\nYour survivor stays at the starting point until you choose to land.");
    }
    private static void DrawItems()
    {
        Label(20,151,80,"Search");
        string query=GUI.TextField(new Rect(95,155,615,28),search,100);
        if(query!=search) { search=query;page=0; }
        var matches=catalog.Where(name=>name.Contains(search,StringComparison.OrdinalIgnoreCase)).ToArray();
        page=Math.Clamp(page,0,Math.Max(0,(matches.Length-1)/6));
        for(int row=0;row<6 && page*6+row<matches.Length;row++)
        {
            string name=matches[page*6+row];
            if(Button(20,193+row*33,690,(name==itemName ? "> " : "")+name)) itemName=name;
        }
        if(Button(20,397,95,"Previous")) page=Math.Max(0,page-1);
        Label(130,399,400,$"Page {page+1} | {matches.Length} matching item IDs");
        if(Button(615,397,95,"Next")) page++;
        itemName=GUI.TextField(new Rect(20,439,430,28),itemName,120);
        Label(460,439,45,"Qty");count=GUI.TextField(new Rect(505,439,55,28),count,3);
        Label(575,439,45,"HP%");condition=GUI.TextField(new Rect(625,439,85,28),condition,6);
        if(Button(20,485,330,"Add item(s) to inventory")) Run(SpawnSelection);
        if(Button(380,485,330,"Learn selected variant schematic")) Run(LearnSchematic);
    }
    private static void DrawWorld()
    {
        bool freeze=GUI.Toggle(new Rect(20,157,650,27),FreezeClock,"Freeze time of day");
        if(freeze!=FreezeClock) Run(()=>SetClock(freeze));
        Label(20,200,150,"Hour (0-24)");hour=GUI.TextField(new Rect(175,200,100,28),hour,8);
        if(Button(300,200,190,"Set time")) Run(()=>
        {
            if(!float.TryParse(hour,NumberStyles.Float,CultureInfo.InvariantCulture,out float value) || !float.IsFinite(value) || value<0 || value>24)
                throw new ArgumentException("Hour must be 0 to 24.");
            Game.GameManager.GetTimeOfDayComponent().SetNormalizedTime(value/24,true);status="Time of day changed.";
        });
        if(Button(20,253,330,"Remember current position")) Run(()=>
        {
            bookmark=Game.GameManager.GetPlayerTransform().position;bookmarkRotation=Game.GameManager.GetPlayerTransform().rotation;
            bookmarkScene=SceneName;status="Position remembered for this scene.";
        });
        if(Button(380,253,330,"Return to remembered position")) Run(()=>
        {
            if(bookmarkScene!=SceneName) throw new InvalidOperationException("Remember a position in this scene first.");
            if(Flying || Game.GameManager.GetPlayerManagerComponent().GetControlMode()!=Game.PlayerControlMode.Normal)
                throw new InvalidOperationException("Return from flight and finish your current interaction first.");
            Game.GameManager.GetPlayerManagerComponent().TeleportPlayer(bookmark,bookmarkRotation);status="Returned to remembered position.";
        });
        if(Ready)
        {
            var p=Game.GameManager.GetPlayerTransform().position;
            Label(20,310,690,$"Player: {p.x:0.00}, {p.y:0.00}, {p.z:0.00} | Scene: {SceneName}");
            var state=FiniteFirearms.State;
            Label(20,360,690,state==null ? "No finite world allocation loaded." : $"World seed: {state.Seed}\nAllocated equipment creation flags:");
            if(state!=null) for(int index=0;index<FirearmSpawnPolicy.Items.Length;index++)
            {
                string item=FirearmSpawnPolicy.Items[index];
                Label(20,415+index*28,690,$"{item}: {(state.Created(item) ? "created" : "not created")} | {state.Site(item).Scene}");
            }
        }
    }
    private static string[] BuildCatalog()=>new[]
    {
        "GEAR_Rifle","GEAR_Revolver","GEAR_Knife","GEAR_Hatchet","GEAR_KnifeImprovised","GEAR_HatchetImprovised",
        "GEAR_Hammer","GEAR_Hacksaw","GEAR_Prybar","GEAR_HighQualityTools","GEAR_MagnifyingLens","GEAR_Firestriker",
        "GEAR_KeroseneLampB","GEAR_LampFuel","GEAR_Flashlight","GEAR_Flashlight_LongLasting","GEAR_Crampons",
        "GEAR_RifleAmmoSingle","GEAR_RevolverAmmoSingle","GEAR_RifleAmmoCasing","GEAR_RevolverAmmoCasing",
        "GEAR_GunpowderCan","GEAR_StumpRemover","GEAR_DustingSulfur","GEAR_ScrapMetal","GEAR_Leather","GEAR_Cloth",
        "GEAR_NoiseMaker","GEAR_FlareGun","GEAR_FlareGunAmmoSingle","GEAR_FlareA","GEAR_BlueFlare","GEAR_Bow","GEAR_Arrow",
        "GEAR_HeavyParka","GEAR_WoolShirt","GEAR_BallisticVest","GEAR_MooseHideBag","GEAR_ToolBelt","GEAR_RifleScabbardA",
        "GEAR_TacticalJacket","GEAR_TacticalGloves","GEAR_WoodMatches","GEAR_Coal","GEAR_Stick","GEAR_Stone",
        "GEAR_RecycledCan","GEAR_CanOpener","GEAR_CattailStalk","GEAR_CoffeeTin","GEAR_TomatoSoupCan","GEAR_InsulatedFlask"
    }.Concat(MementoRewardPolicy.Supplies).Concat(StreetLootPolicy.SkillBooks).Concat(VariantPolicy.Variants.Select(variant=>variant.Item))
        .Distinct().OrderBy(name=>name,StringComparer.Ordinal).ToArray();
}

[HarmonyPatch]
internal static class ResetDebugSession
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.GameManager),"LaunchSandbox");
        yield return AccessTools.Method(typeof(Game.SaveGameSystem),"RestoreGlobalData");
    }
    private static void Prefix()=>DebugTools.Reset();
}
[HarmonyPatch]
internal static class DebugCarryWeight
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.Inventory),"GetTotalWeightKG");
        yield return AccessTools.Method(typeof(Game.Encumber),"GetGearWeightKG");
    }
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(ref Il2CppTLD.IntBackedUnit.ItemWeight __result)
    { if(DebugTools.Active && DebugTools.NoWeight) __result=Il2CppTLD.IntBackedUnit.ItemWeight.Zero; }
}

[HarmonyPatch]
internal static class DebugPanelMovementInput
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.InputManager),"GetPlayerMovement");
        yield return AccessTools.Method(typeof(Game.InputManager),"GetCameraMovementMouse");
        yield return AccessTools.Method(typeof(Game.InputManager),"GetCameraMovementGamepad");
    }
    private static bool Prefix(ref Vector2 __result)
    { if(!DebugTools.BlocksInput) return true;__result=Vector2.zero;return false; }
}
[HarmonyPatch]
internal static class DebugPanelActionInput
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.InputManager),"GetFirePressed");
        yield return AccessTools.Method(typeof(Game.InputManager),"GetAltFirePressed");
        yield return AccessTools.Method(typeof(Game.InputManager),"GetInteractPressed");
    }
    private static bool Prefix(ref bool __result)
    { if(!DebugTools.BlocksInput) return true;__result=false;return false; }
}
[HarmonyPatch]
internal static class DebugPanelStopsFlightCamera
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.FlyMode),"LateUpdate");
    private static bool Prefix()=>!DebugTools.Open;
}
[HarmonyPatch]
internal static class DebugGodDamage
{
    private static IEnumerable<MethodBase> TargetMethods()=>AccessTools.GetDeclaredMethods(typeof(Game.Condition)).Where(method=>method.Name=="AddHealth");
    private static bool Prefix(float hp)=>!(DebugTools.Active && (DebugTools.God || DebugTools.Flying) && hp<0);
}
[HarmonyPatch]
internal static class DebugFlyControls
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.InputManager),"GetFlyModeToggleEnabled");
        yield return AccessTools.Method(typeof(Game.InputManager),"GetFlyModeAcceptPlacementPressed");
        yield return AccessTools.Method(typeof(Game.InputManager),"GetFlyModeCancelPlacementPressed");
    }
    private static bool Prefix(MethodBase __originalMethod,ref bool __result)
    {
        bool ownFlightKey = __originalMethod.Name=="GetFlyModeToggleEnabled" ? DebugTools.Enabled : DebugTools.Flying;
        if(!DebugTools.BlocksInput && !ownFlightKey) return true;
        __result=false;return false;
    }
}
[HarmonyPatch]
internal static class DebugGodSave
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.PlayerManager),"Serialize");
    private static void Postfix(Game.PlayerManager __instance,ref string __result)=>__result=DebugTools.SavedGod(__instance,__result);
}
[HarmonyPatch]
internal static class DebugClockSave
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.TimeOfDay),"Serialize");
    private static void Postfix(Game.TimeOfDay __instance,ref string __result)=>__result=DebugTools.SavedClock(__instance,__result);
}
[HarmonyPatch]
internal static class DebugFlyVerticalInput
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.InputManager),"GetFlyModeMoveUpDown");
        yield return AccessTools.Method(typeof(Game.InputManager),"GetFlyModeMoveDownDown");
    }
    private static void Postfix(MethodBase __originalMethod,ref bool __result)
    {
        if(!DebugTools.Active || !DebugTools.Flying || DebugTools.Open) return;
        __result |= __originalMethod.Name=="GetFlyModeMoveUpDown" ? Input.GetKey(KeyCode.E) : Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q);
    }
}
