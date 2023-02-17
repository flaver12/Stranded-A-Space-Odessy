using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using org.flaver.defintion;
using UnityEngine;
using MoonSharp.Interpreter;

namespace org.flaver.model
{
    public class FurnitureActions
    {    
        public static FurnitureActions Instance { get; private set; }

        private Script luaScript;
        public FurnitureActions(string rawLua)
        {
            // Tell Moonsharp load all user data
            UserData.RegisterAssembly();

            Instance = this;
            luaScript = new Script();
            luaScript.DoString(rawLua);
        }
        

        public static void CallFunctionsWithFurnitures(string[] functioNames, Furniture furniture, float deltaTime)
        {
            foreach (string functionName in functioNames)
            {
                DynValue func = Instance.luaScript.Globals.Get(functionName);
                if (func.Type != DataType.Function)
                {
                    Debug.LogError($"CallFunctionsWithFurnitures: {functionName} is not a function!");
                    return; // TODO do we want a hard return?
                }
                DynValue result = Instance.luaScript.Call(func, furniture, deltaTime);
                
                if (result.Type == DataType.String)
                {
                    Debug.Log(result.String);
                }
            }
        }

        public static DynValue CallFunction(string functionName, params object[] args)
        {
            DynValue func = Instance.luaScript.Globals.Get(functionName);
            if (func.Type != DataType.Function)
            {
                Debug.LogError($"CallbackFunction: {functionName} is not a function!");
                return DynValue.Void; // TODO do we want a hard return?
            }

            return Instance.luaScript.Call(func, args);
        }



        public static void JobCompleteFurnitureBuilding(Job job)
        {
            WorldController.Instance.World.PlaceFurniture(job.JobObjectType, job.Tile);
            job.Tile.PendingFurnitueJob = null;
        }

        /*
        public static void DoorTickAction(Furniture furniture, float deltaTime)
        {
            // Debug.Log("DoorTickAction");
            if (furniture.GetParameteter("isOpening") >= 1f)
            {
                furniture.SetParameteter("openness", furniture.GetParameteter("openness") + deltaTime * 4);

                if (furniture.GetParameteter("openness") >= 1)
                {
                    furniture.SetParameteter("isOpening", 0f);
                }
            }
            else
            {
               furniture.SetParameteter("openness", furniture.GetParameteter("openness") - deltaTime * 4);
            }

            furniture.SetParameteter("openness", Mathf.Clamp01(furniture.GetParameteter("openness")));

            if (furniture.onChanged != null)
            {
                furniture.onChanged(furniture);
            }
        }

        public static Item[] StockPileGetItemsFromFiler()
        {
            return new Item[1] { new Item("Steel Plate", 50, 0) };
        }

        public static void StockpileTickAction(Furniture furniture, float deltaTime)
        {
            // We need to ensure that we have a job on the queue
            // Asking for: I am empty?: That all loos item come to us
            // Asking for: We have something: Then if below max stack size, bring me more

            // TODO this function does not need to run each update
            if (furniture.Tile.Item != null && furniture.Tile.Item.StackSize >= furniture.Tile.Item.maxStackSize)
            {
                // We are full
                furniture.CancelJobs();
                return;
            }

            if(furniture.Tile.Item != null && furniture.Tile.Item.StackSize == 0)
            {
                // Something went wrong here!
                Debug.LogError("Stockpile has a 0 sized stack!");
                furniture.CancelJobs();
                return;
            }

            if (furniture.JobCount() > 0)
            {
                // All done
                return;
            }

            Item[] desiredItems;
            if (furniture.Tile.Item == null)
            {
                // Empty ask anything to get here
                desiredItems = StockPileGetItemsFromFiler();

            }
            else
            {
                Item desiredItem = furniture.Tile.Item.Clone();
                desiredItem.maxStackSize -= desiredItem.StackSize;
                desiredItem.StackSize = 0;

                desiredItems = new Item[] { desiredItem };
            }

            Job job = new Job(furniture.Tile, null, null, 0, desiredItems);
            job.canTakeFromStockpile = false;

            job.RegisterJobWorkedCallback(StockpileJobWorked);
            furniture.AddJob(job);
        }

        public static Enterability IsDoorEnterable(Furniture furniture)
        {
            furniture.SetParameteter("isOpening", 1f);

            if(furniture.GetParameteter("openness") >= 1f)
            {
                return Enterability.Yes;                
            }

            return Enterability.Soon;
        }

       

        
        private static void StockpileJobWorked(Job job)
        {
            job.CancelJob();

            // TODO change me
            foreach (Item item in job.itemRequirements.Values)
            {
                if (item.StackSize > 0)
                {
                    World.Instance.itemManager.InstallItem(job.Tile, item);
                    return;
                }
            }
        }

        public static void OxygenGeneratorTickAction(Furniture furniture, float deltaTime)
        {
            if (furniture.Tile.room.GetGasAmount("O2") < 0.20f)
            {
                // TODO change the gas contribution based on the volume of the room
                furniture.Tile.room.ChangeGas("O2", 0.01f * deltaTime);
            }
        }

        public static void MiningDroneStationTickAction(Furniture furniture, float deltaTime)
        {
            Tile tileSpawnSpot = furniture.GetSpawnSpotTile();

            if (furniture.JobCount() > 0)
            {
                // Check to see if the Steel plate dest tile is full
                if(tileSpawnSpot.Item != null && tileSpawnSpot.Item.StackSize >= tileSpawnSpot.Item.maxStackSize)
                {
                    // We can stop now
                    furniture.CancelJobs();
                }
                return;
            }

            // If we get here, the there is no job
            // Check if we are full, then we dont need a new one!
            if (tileSpawnSpot.Item != null && tileSpawnSpot.Item.StackSize >= tileSpawnSpot.Item.maxStackSize)
            {
                return;
            }

            Tile jobSpot = furniture.GetJobSpotTile();

            if (jobSpot.Item != null && (jobSpot.Item.StackSize >= jobSpot.Item.maxStackSize))
            {
                // Our drop stop is all ready full, dont do anything;
                return;
            }

            Job job = new Job(
                furniture.GetJobSpotTile(),
                null,
                MiningDroneStationJobCompleted,
                1f,
                null,
                true // repeats until the tile is full
            );
            furniture.AddJob(job);
        }

        public static void MiningDroneStationJobCompleted(Job job)
        {
            Item item = new Item("Steel Plate", 50, 10);
            World.Instance.itemManager.InstallItem(job.furniture.GetSpawnSpotTile(), item);
        }
        */
    }
}

