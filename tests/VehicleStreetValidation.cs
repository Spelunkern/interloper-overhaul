extern alias GameAssembly;
using Game=GameAssembly::Il2Cpp;
using Units=Il2CppTLD.IntBackedUnit;
using Gear=GameAssembly::Il2CppTLD.Gear;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
namespace InterloperOverhaul;
internal static class VehicleStreetValidation
{
    internal static void Run(Action<bool,string> check)
    {
        var mode=Game.ExperienceModeManager.s_CurrentGameMode;
        var oldInventory=Game.GameManager.m_Inventory;
        var root=new GameObject("IO_VehicleStreetValidation");root.SetActive(false);
        var items=new List<Game.GearItem>();
        string phase="vehicle warmth";
        try
        {
            var playerVehicle=root.AddComponent<Game.PlayerInVehicle>();playerVehicle.m_TempIncrease=5;playerVehicle.m_InVehicle=true;
            VehicleBalance.Temperature(playerVehicle);
            check(playerVehicle.GetTempIncrease()==15,"native vehicle temperature getter reports fifteen degrees");
            var bedObject=new GameObject("VehicleBed");bedObject.transform.SetParent(root.transform,false);
            var bed=bedObject.AddComponent<Game.Bed>();bed.m_WarmthBonusCelsius=3;VehicleBalance.Bed(bed);
            check(bed.GetWarmthBonusCelsius()==5,"native built-in vehicle bed reports five degrees");
            phase="mailbox assets and spawning";
            foreach(string name in StreetLootPolicy.CommonBooks.Concat(StreetLootPolicy.SkillBooks).Concat(new[] { "GEAR_Newsprint","GEAR_NewsprintRoll","GEAR_LampFuel" }))
                check(Game.GearItem.LoadGearItemPrefab(name)!=null,"street loot prefab exists: "+name);
            var mailbox=new GameObject("INTERACTIVE_MailBoxA");mailbox.transform.SetParent(root.transform,false);
            var spawnObject=new GameObject("SpawnPrefab_mailbox");spawnObject.transform.SetParent(mailbox.transform,false);
            var spawner=spawnObject.AddComponent<Game.PrefabSpawn>();
            spawner.m_LodHeights=new Il2CppStructArray<float>(0);spawner.m_TakePrefabScale=true;
            Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(spawnObject,VehicleBalance.GuidFor("IO_Mailbox"));
            spawner.Awake();
            int seed=Game.GameManager.GetRandomSeed(0);
            string expected="";
            for(int position=0;position<10000;position++)
            {
                spawnObject.transform.localPosition=new Vector3(position,0,0);
                expected=StreetLootPolicy.Mail(seed,LootBalance.Key(spawnObject));
                if(StreetLootPolicy.SkillBooks.Contains(expected)) break;
            }
            check(StreetLootPolicy.SkillBooks.Contains(expected),"native mailbox fixture selects a rare research book");
            spawner.Update();
            var book=spawner.GetComponentInChildren<Game.GearItem>(true);if(book!=null) items.Add(book);
            check(book!=null && LootBalancePolicy.ItemName(book.m_GearItemData.name)==expected && spawner.m_SpawnComplete && book.m_RolledSpawnChance && !MailboxLoot.Spawning,
                "native mailbox generator creates its selected physical research book once and ends its rarity scope");
            var selection=spawner.m_PrefabList;
            MailboxLoot.Configure(spawner);
            check(spawner.m_PrefabList.Pointer==selection.Pointer,"completed native mailbox spawners keep their original selection");
            phase="fuel tank construction";
            var vehicle=new GameObject("ENCAMPMENT_CarSedan");vehicle.transform.SetParent(root.transform,false);
            var gloveObject=new GameObject("CarSedanGloveBox_Prefab");gloveObject.transform.SetParent(vehicle.transform,false);
            var glove=gloveObject.AddComponent<Game.Container>();gloveObject.AddComponent<Game.ContainerInteraction>();
            glove.m_Items=new Il2CppSystem.Collections.Generic.List<Gear.GearItemObject>();
            glove.m_DecorationItems=new Il2CppSystem.Collections.Generic.List<Game.DecorationItem>();
            glove.m_ScriptedItemPrefabs=new Il2CppReferenceArray<GameObject>(0);glove.m_ScriptedItemFilter=new Il2CppStructArray<Game.GameModeFilter>(0);
            string sourceGuid=VehicleBalance.GuidFor("IO_GloveBox");Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(gloveObject,sourceGuid);
            var coverObject=new GameObject("CarSedanFuelCover_Prefab");coverObject.transform.SetParent(vehicle.transform,false);
            Game.ObjectGuid.MaybeAttachObjectGuidAddOrReplace(coverObject,VehicleBalance.GuidFor("IO_FuelCover"));
            var cover=coverObject.AddComponent<Game.OpenClose>();
            var tank=VehicleBalance.Tank(cover);
            check(tank!=null && tank.GetComponent<Game.ContainerInteraction>()!=null && tank.m_Items.Count==0 && tank.m_NotPopulated && !tank.m_DisableSerialization,
                "fuel cover gains a native searchable persistent container instead of only an opening animation");
            string tankGuid=Game.ObjectGuid.MaybeGetGuidFromGameObject(tank.gameObject);
            check(!string.IsNullOrEmpty(tankGuid) && tankGuid!=sourceGuid && Game.ObjectGuid.MaybeGetGuidFromGameObject(gloveObject)==sourceGuid &&
                VehicleBalance.Tank(cover).Pointer==tank.Pointer,"fuel tank construction is idempotent and does not borrow the glove box's persistent ID");
            string key="";double litres=0;
            for(int position=0;position<1000;position++)
            {
                vehicle.transform.localPosition=new Vector3(position,0,0);
                key=LootBalance.Key(tank.gameObject);litres=StreetLootPolicy.FuelLitres(seed,key);
                if(litres>0) break;
            }
            check(litres>=0.05 && litres<0.15,"native fuel tank fixture selects a small rare kerosene find");
            tank.Start();
            check(tank.m_Items.Count==1 && !tank.m_NotPopulated,"native tank population produces exactly one rare fuel bottle");
            var fuel=tank.m_Items[0].m_GearItem;items.Add(fuel);
            float ratio=(float)(fuel.m_LiquidItem.m_Liquid/fuel.m_LiquidItem.m_LiquidCapacity);
            check(Math.Abs(ratio*0.5-litres)<0.000001,"native kerosene item contains the requested fifty-to-one-hundred-fifty millilitres");
            tank.PopulateContents();
            check(tank.m_Items.Count==1,"reopening a populated tank cannot generate extra kerosene");
            fuel.Deserialize(fuel.Serialize(),false);
            check(Math.Abs((float)(fuel.m_LiquidItem.m_Liquid/fuel.m_LiquidItem.m_LiquidCapacity)*0.5-litres)<0.000001,"small kerosene quantities survive native item saves");
            tank.RemoveGear(fuel,false);tank.m_Inspected=true;
            var empty=tank.Serialize();tank.Deserialize(empty,new Il2CppSystem.Collections.Generic.List<Game.GearItem>());tank.PopulateContents();
            check(tank.m_Items.Count==0,"a looted tank remains empty after native container save, reload, and reopening");
            phase="rock cache materials";
            var cacheObject=new GameObject("RockCachePrefab");cacheObject.transform.SetParent(root.transform,false);
            var cache=cacheObject.AddComponent<Game.RockCache>();
            cache.m_NumRocksToBuild=50;cache.m_NumRocksFromDismantle=50;cache.m_NumSticksToBuild=0;
            cache.m_RocksPrefab=Game.GearItem.LoadGearItemPrefab("GEAR_Stone");
            cache.m_SticksPrefab=Game.GearItem.LoadGearItemPrefab("GEAR_Stick");
            var manager=root.AddComponent<Game.RockCacheManager>();manager.m_RockCachePrefab=cache;
            var inventory=root.AddComponent<Game.Inventory>();Game.GameManager.m_Inventory=inventory;
            var stones=Game.GearItem.InstantiateGearItem("GEAR_Stone");items.Add(stones);
            stones.SkipSpawnChanceRollInitialDecayAndAutoEvolve();stones.m_HasBeenOwnedByPlayer=true;stones.m_InPlayerInventory=true;stones.m_StackableItem.m_Units=25;
            inventory.m_Items.Add(new Gear.GearItemObject(stones));
            string missing=manager.GetMissingMaterialsString();
            check(manager.m_RockCachePrefab.m_NumRocksToBuild==25 && manager.m_RockCachePrefab.m_NumRocksFromDismantle<=25 && cache.m_NumRocksToBuild==50,
                "rock-cache blueprint costs twenty-five stones without mutating its authored prefab or allowing excess refunds");
            check(string.IsNullOrEmpty(missing),"native rock-cache requirement check accepts twenty-five stones");
            stones.m_StackableItem.m_Units=24;
            check(!string.IsNullOrEmpty(manager.GetMissingMaterialsString()),"native rock-cache requirement check rejects twenty-four stones");
            phase="difficulty isolation";
            Game.ExperienceModeManager.s_CurrentGameMode=Game.ExperienceModeManager.GetGameModeFromName("Stalker");
            VehicleBalance.Temperature(playerVehicle);RockCacheBalance.Manager(manager);
            check(playerVehicle.GetTempIncrease()==5 && manager.m_RockCachePrefab.Pointer==cache.Pointer && cache.m_NumRocksToBuild==50,
                "Stalker restores native vehicle warmth and fifty-stone rock-cache requirements");
        }
        catch(Exception ex) { throw new InvalidOperationException("Vehicle/street validation phase: "+phase,ex); }
        finally
        {
            Game.ExperienceModeManager.s_CurrentGameMode=mode;Game.GameManager.m_Inventory=oldInventory;
            foreach(var item in items) if(item!=null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
