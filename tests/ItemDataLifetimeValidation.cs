extern alias GameAssembly;
using System.Collections;
using HarmonyLib;
using Game=GameAssembly::Il2Cpp;
using Gear=GameAssembly::Il2CppTLD.Gear;
using Units=Il2CppTLD.IntBackedUnit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InterloperOverhaul;

// Menu-only regression: detached data used by weight previews must survive the
// same native asset collection that runs between Survival scenes.
internal static class ItemDataLifetimeValidation
{
    internal static IEnumerator Run(Action<bool,string> check)
    {
        var mode=Game.ExperienceModeManager.s_CurrentGameMode;
        var manager=Game.GameManager.m_ExperienceModeManager;
        var inventoryBefore=Game.GameManager.m_Inventory;
        var playerBefore=Game.GameManager.m_PlayerManager;
        bool mainMenu=Game.GameManager.s_ActiveIsMainMenu;
        GameObject root=null;
        var items=new List<Game.GearItem>();
        var detached=new List<Gear.GearItemData>();
        var sentinels=new List<Gear.GearItemData>();
        var transientScenes=new List<Scene>();
        var names=new[] { "GEAR_BookMending","GEAR_BookFireStarting","GEAR_BookA","GEAR_BookBopen","GEAR_Newsprint","GEAR_NewsprintRoll","GEAR_PaperStack","GEAR_CanOpener" };
        try
        {
            Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName("Interloper");
            Game.GameManager.m_ExperienceModeManager=Game.ExperienceModeManager.Instance;
            Game.GameManager.s_ActiveIsMainMenu=false;
            root=new GameObject("IO_ItemDataLifetimeValidation");root.SetActive(false);
            var inventory=root.AddComponent<Game.Inventory>();
            Game.GameManager.m_Inventory=inventory;
            Game.GameManager.m_PlayerManager=root.AddComponent<Game.PlayerManager>();
            foreach(string name in names)
            {
                var prefab=Game.GearItem.LoadGearItemPrefab(name);
                var original=prefab.m_GearItemData;
                prefab.GetSingleItemWeightKG(); // Preview detaches its temporary balanced data.
                var clone=ItemBalance.BalancedData(original,ItemBalancePolicy.WeightKg(name),false);
                check(clone!=null && prefab.m_GearItemData.Pointer==original.Pointer,"weight preview leaves native prefab data intact: "+name);
                detached.Add(clone);
            }
            // A known unprotected object proves the native collector really ran.
            sentinels.Add(ScriptableObject.CreateInstance<Gear.GearItemData>());
            for(int cycle=0;cycle<2;cycle++)
            {
                var scene=SceneManager.CreateScene("IO_LifetimeTransition_"+cycle);
                transientScenes.Add(scene);
                var anchor=new GameObject("IO_TransientSceneAnchor");
                SceneManager.MoveGameObjectToScene(anchor,scene);
                var unload=SceneManager.UnloadSceneAsync(scene);
                while(!unload.isDone) yield return null;
                var collect=Resources.UnloadUnusedAssets();
                while(!collect.isDone) yield return null;
                check(sentinels[0]==null,"native unused-asset collection releases an unprotected control asset (cycle "+cycle+")");
                for(int index=0;index<names.Length;index++)
                {
                    var data=detached[index];
                    check(data!=null && !string.IsNullOrEmpty(data.name),"balanced data survives scene unload and collection: "+names[index]+" cycle "+cycle);
                    var item=Game.GearItem.InstantiateGearItem(names[index]);items.Add(item);
                    item.SkipSpawnChanceRollInitialDecayAndAutoEvolve();
                    item.m_HasBeenOwnedByPlayer=true;item.m_LastUpdatedTODHoursInitialized=true;item.ManualStart();
                    inventory.AddGear(item);
                    check(item.m_InPlayerInventory && !item.gameObject.activeSelf &&
                        inventory.m_Items.ToArray().Any(entry=>entry.m_GearItem!=null && entry.m_GearItem.Pointer==item.Pointer) &&
                        item.GetSingleItemWeightKG()==Units.ItemWeight.FromKilograms((float)ItemBalancePolicy.WeightKg(names[index])),
                        "native pickup completes and hides inspection gear after collection: "+names[index]+" cycle "+cycle);
                    // Remove native references again before the next collection.
                    inventory.DestroyGear(item);
                }
            }
            var staleItem=Game.GearItem.InstantiateGearItem("GEAR_CattailStalk");items.Add(staleItem);
            var stale=UnityEngine.Object.Instantiate(staleItem.m_GearItemData).Cast<Gear.GearItemData>();
            staleItem.m_GearItemData=stale;
            UnityEngine.Object.DestroyImmediate(stale);
            check(stale==null && !ReferenceEquals(stale,null),"destroyed Unity data retains a managed wrapper, reproducing the reported null semantics");
            AccessTools.Method(typeof(PreserveCraftedBundleIdentity),"Prefix").Invoke(null,new object[] { staleItem });
            check(true,"bundle pickup prefix ignores unloaded native data instead of reading its name");
            var originalBook=Game.GearItem.LoadGearItemPrefab("GEAR_BookMending").m_GearItemData;
            var cached=ItemBalance.BalancedData(originalBook,0.2,false);
            UnityEngine.Object.DestroyImmediate(cached);
            var replacement=ItemBalance.BalancedData(originalBook,0.2,false);
            check(replacement!=null && replacement.name==originalBook.name,"balanced-data cache replaces explicitly destroyed entries");
        }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;
            Game.GameManager.m_ExperienceModeManager=manager;
            Game.GameManager.m_Inventory=inventoryBefore;
            Game.GameManager.m_PlayerManager=playerBefore;
            Game.GameManager.s_ActiveIsMainMenu=mainMenu;
            foreach(var item in items) if(item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            foreach(var sentinel in sentinels) if(sentinel!=null) UnityEngine.Object.DestroyImmediate(sentinel);
            foreach(var scene in transientScenes) if(scene.IsValid() && scene.isLoaded) SceneManager.UnloadSceneAsync(scene);
            if(root!=null) UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
