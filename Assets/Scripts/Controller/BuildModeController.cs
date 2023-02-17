using System.Collections;
using System.Collections.Generic;
using org.flaver.defintion;
using org.flaver.model;
using org.flaver.pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

namespace org.flaver.controller
{
    public class BuildModeController : MonoBehaviour
    {
        private TileType buildModeTile;
        public BuildMode buildMode = BuildMode.Floor;
        public string buildModeObjectType;
        private FurnitureSpriteController furnitureSpriteController;
        private MouseController mouseController;

        private void Start()
        {
            furnitureSpriteController = GameObject.FindObjectOfType<FurnitureSpriteController>();
            mouseController = GameObject.FindObjectOfType<MouseController>();
        }

        public void Build(Tile tile)
        {
            if (buildMode == BuildMode.Furniture)
            {
                // Create installed object and set it to the tile
                // WorldController.Instance.World.PlaceFurniture(buildModeObjectType, tile);
                string furnitureType = buildModeObjectType;

                // Validate if we can build
                if(WorldController.Instance.World.IsFurniturePlacementValid(furnitureType, tile) && tile.PendingFurnitueJob == null)
                {
                    Job job;
                    
                    if (WorldController.Instance.World.FurnitureJobPrototypes.ContainsKey(furnitureType))
                    {
                        job = WorldController.Instance.World.FurnitureJobPrototypes[furnitureType].Clone();
                        job.Tile = tile;
                    }
                    else
                    {
                        Debug.LogError($"No prototype job found for {furnitureType}. Using dummy job");
                        job = new Job(tile, furnitureType, FurnitureActions.JobCompleteFurnitureBuilding, 0.1f, null);
                    }

                    job.furniturePrototype = WorldController.Instance.World.FurniturePrototypes[furnitureType];

                    // TODO fix me
                    tile.PendingFurnitueJob = job;
                    job.RegisterJobStoppedCallback((job) => { job.Tile.PendingFurnitueJob = null; });
                    WorldController.Instance.World.JobQueue.Enqueue(job);
                    Debug.Log($"Job queue size: {WorldController.Instance.World.JobQueue.Count}");
                }
            }
            else if (buildMode == BuildMode.Floor)
            {
                // Build a tile
                tile.Type = buildModeTile;
            }
            else if(buildMode == BuildMode.Deconstruct)
            {
                if (tile.Furniture != null)
                {
                    tile.Furniture.Deconstruct();
                }
            }
            else
            {
                Debug.LogError("Unkown build mode");
            }
        }

        public void SetModeBuildFloor()
        {
            buildMode = BuildMode.Floor;
            buildModeTile = TileType.Floor;
            mouseController.StartBuildMode();
        }

        public void SetModeBulldoze()
        {
            buildMode = BuildMode.Floor;
            buildModeTile = TileType.Empty;
            mouseController.StartBuildMode();
        }

        public void SetModeDeconstruct()
        {
            buildMode = BuildMode.Deconstruct;
            mouseController.StartBuildMode();
        }

        public void SetModeBuildFurniture(string objectType)
        {
            buildMode = BuildMode.Furniture;
            buildModeObjectType = objectType;
            mouseController.StartBuildMode();
        }

        // Debug only
        public void PathfindingTest()
        {
            WorldController.Instance.World.SetupPathfindingExample();
        }

        public bool IsObjectDraggable()
        {
            if (buildMode == BuildMode.Floor || buildMode == BuildMode.Deconstruct)
            {
                // floors are draggable
                return true;
            }

            Furniture proto = WorldController.Instance.World.FurniturePrototypes[buildModeObjectType];
            return proto.Width == 1 && proto.Height == 1;
        }

    }
}