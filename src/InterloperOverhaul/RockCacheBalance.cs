extern alias GameAssembly;
using System.Reflection;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
using UnityEngine;
namespace InterloperOverhaul;
internal static class RockCacheBalance
{
    private sealed class PrefabState { internal Game.RockCache Original,Balanced; }
    private static readonly Dictionary<int,PrefabState> prefabs=new();
    internal static void Configure(Game.RockCache cache)
    {
        if (!ItemBalance.Active || cache==null) return;
        cache.m_NumRocksToBuild=25;
        cache.m_NumRocksFromDismantle=Math.Min(cache.m_NumRocksFromDismantle,25);
    }
    internal static void Manager(Game.RockCacheManager manager)
    {
        if (manager?.m_RockCachePrefab==null) return;
        int id=manager.GetInstanceID();
        if (!prefabs.TryGetValue(id,out var state)) prefabs[id]=state=new PrefabState { Original=manager.m_RockCachePrefab };
        if (!ItemBalance.Active) { manager.m_RockCachePrefab=state.Original;return; }
        if (state.Balanced==null)
        {
            var root=new GameObject("IO_RockCachePrefab");root.SetActive(false);root.hideFlags=HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(root);
            state.Balanced=UnityEngine.Object.Instantiate(state.Original.gameObject,root.transform).GetComponent<Game.RockCache>();
            state.Balanced.name=state.Original.name;
            Configure(state.Balanced);
        }
        manager.m_RockCachePrefab=state.Balanced;
    }
}
[HarmonyPatch]
internal static class CheaperRockCacheBlueprint
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.RockCacheManager),"Start");
        yield return AccessTools.Method(typeof(Game.RockCacheManager),"GetMissingMaterialsString");
        yield return AccessTools.Method(typeof(Game.RockCacheManager),"CanAttemptToPlaceRockCache");
        yield return AccessTools.Method(typeof(Game.RockCacheManager),"CanPlaceRockCache");
    }
    private static void Prefix(Game.RockCacheManager __instance)=>RockCacheBalance.Manager(__instance);
}
[HarmonyPatch]
internal static class CheaperRockCacheInstances
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Game.RockCache),"Awake");
        yield return AccessTools.Method(typeof(Game.RockCache),"Deserialize");
        yield return AccessTools.Method(typeof(Game.RockCache),"OnBuild");
        yield return AccessTools.Method(typeof(Game.RockCache),"BuildFinished");
        yield return AccessTools.Method(typeof(Game.RockCache),"DismantleFinished");
    }
    private static void Prefix(Game.RockCache __instance)=>RockCacheBalance.Configure(__instance);
}
