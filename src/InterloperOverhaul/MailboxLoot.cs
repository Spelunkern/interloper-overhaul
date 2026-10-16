extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
namespace InterloperOverhaul;
internal static class MailboxLoot
{
    [ThreadStatic] internal static bool Spawning;
    internal static void Configure(Game.PrefabSpawn spawner)
    {
        if (!ItemBalance.Active || spawner==null || spawner.m_SpawnComplete || !StreetLootPolicy.IsMailboxSpawner(spawner.name)) return;
        string name=StreetLootPolicy.Mail(Game.GameManager.GetRandomSeed(0),LootBalance.Key(spawner.gameObject),LootBalance.Access(spawner.gameObject));
        spawner.m_NumToSpawnMin=spawner.m_NumToSpawnMax=1;
        spawner.m_ChanceOfNoSpawn=string.IsNullOrEmpty(name) ? 100 : 0;
        if (string.IsNullOrEmpty(name)) return;
        // Native object spawning and gear persistence still own the physical find.
        var pose=spawner.m_PrefabList!=null && spawner.m_PrefabList.Length>0 ? spawner.m_PrefabList[0] : null;
        spawner.m_PrefabList=new Il2CppReferenceArray<Game.PrefabSpawn.Element>(new[] { new Game.PrefabSpawn.Element
        {
            m_Prefab=Game.GearItem.LoadGearItemPrefab(name).gameObject,m_SpawnWeight=1,
            m_Offset=pose?.m_Offset ?? Vector3.zero,m_Rotation=pose?.m_Rotation ?? Vector3.zero,
            m_Scale=Vector3.one,m_SetId=0,m_LodMask=0
        } });
    }
}
[HarmonyPatch]
internal static class StockMailboxes
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.PrefabSpawn),"SpawnObjects");
    private static void Prefix(Game.PrefabSpawn __instance,out bool __state)
    {
        __state=MailboxLoot.Spawning;
        MailboxLoot.Configure(__instance);
        MailboxLoot.Spawning=ItemBalance.Active && StreetLootPolicy.IsMailboxSpawner(__instance.name);
    }
    private static void Finalizer(bool __state)=>MailboxLoot.Spawning=__state;
}
[HarmonyPatch]
internal static class PreserveMailboxFinds
{
    private static MethodBase TargetMethod()=>AccessTools.Method(typeof(Game.GearItem),"Awake");
    private static void Postfix(Game.GearItem __instance)
    {
        if (MailboxLoot.Spawning && StreetLootPolicy.IsMailSupply(__instance.name)) __instance.SkipSpawnChanceRoll();
    }
}
