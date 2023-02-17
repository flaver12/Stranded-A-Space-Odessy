using System.Collections;
using System.Collections.Generic;
using org.flaver.defintion;
using org.flaver.model;
using UnityEngine;

namespace org.flaver.controller
{
    public class FurnitureSpriteController : MonoBehaviour
    {
        private Dictionary<Furniture, GameObject> mappedFurnitures;
        private Dictionary<string, Sprite> mappedFurnituresSprites;
        private World world { get { return WorldController.Instance.World; } }

        private void Start()
        {
            LoadSprites();
                        
            // Init map of which tile belongs to which gameobject
            mappedFurnitures = new Dictionary<Furniture, GameObject>();

            // Register world callbacks
            world.RegisterFurnitureCreated(OnFurnitureCreated);

            // Go trough all EXISTING furnuture, call on created event manully
            foreach (Furniture entry in world.Furnitures)
            {
                OnFurnitureCreated(entry);
            }
        }

        public void OnFurnitureCreated(Furniture furniture)
        {
            Debug.Log("OnFurnitureCreated");
            // Create a visual game object lined to the data
            GameObject furnitureGameObject = new GameObject();
            furnitureGameObject.name = $"{furniture.ObjectType}({furniture.Tile.X}, {furniture.Tile.Y})";

            mappedFurnitures.Add(furniture, furnitureGameObject); // Register mapped tile

            furnitureGameObject.transform.position = new Vector3(furniture.Tile.X + ((furniture.Width - 1) / 2f), furniture.Tile.Y + ((furniture.Height - 1) / 2f), 0f);
            furnitureGameObject.transform.SetParent(transform, true); // true = stay in world pos



            // FIXME maybe later make it rotate better
            if (furniture.ObjectType == "Door")
            {
                // By default the door graphic is for east west
                // Check for a wall north south
                // Rotate then rotate 90degres
                Tile nortTile = world.GetTileByPosition(furniture.Tile.X, furniture.Tile.Y + 1);
                Tile southTile = world.GetTileByPosition(furniture.Tile.X, furniture.Tile.Y - 1);
                
                if (nortTile != null && southTile != null && nortTile.Furniture != null && southTile.Furniture != null && nortTile.Furniture.ObjectType == "Wall" && southTile.Furniture.ObjectType == "Wall")
                {
                    furnitureGameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }

            furnitureGameObject.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furniture);
            furnitureGameObject.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Furniture");
            furnitureGameObject.GetComponent<SpriteRenderer>().color = furniture.tint;

            // Object infos changes
            furniture.RegisterOnChange(OnFurnitureChanged);
            furniture.RegisterOnRemoved(OnFurnitureRemoved);
        }

        public Sprite GetSpriteForFurniture(string objectType)
        {
            if (mappedFurnituresSprites.ContainsKey(objectType))
            {
                return mappedFurnituresSprites[objectType];
            }

            if (mappedFurnituresSprites.ContainsKey(objectType + "_"))
            {
                return mappedFurnituresSprites[objectType + "_"];
            }

            Debug.LogError($"Could not find the object type or its base key ${objectType}");
            return null;
        }

        public Sprite GetSpriteForFurniture(Furniture furniture)
        {
            string spriteName = furniture.ObjectType ;
            if(!furniture.LinksToNeighbour)
            {
                // FIXME needs to be general
                if (furniture.ObjectType == "Door")
                {
                    if (furniture.GetParameteter("openness") < 0.1f)
                    {
                        spriteName = "Door";
                    }
                    else if (furniture.GetParameteter("openness") < 0.5f)
                    {
                        spriteName = "Door_openness_1";
                    }
                    else if (furniture.GetParameteter("openness") < 0.9f)
                    {
                        spriteName = "Door_openness_2";
                    }
                    else
                    {
                        spriteName = "Door_openness_3";
                    }
                    
                }
                return mappedFurnituresSprites[spriteName];
            }

            // Load the correct sprite
            spriteName = furniture.ObjectType + "_";
            
            // Check for your lovley neighbours North, East, South, West (Clockwise)
            Tile tile;
            int x = furniture.Tile.X;
            int y = furniture.Tile.Y;

            tile = world.GetTileByPosition(x, y + 1); // North
            if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
            {
                // We have a neighbour to the north
                spriteName = spriteName + "N";
            }
            tile = world.GetTileByPosition(x + 1, y); // East
            if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
            {
                // We have a neighbour to the north
                spriteName = spriteName + "E";
            }
            tile = world.GetTileByPosition(x, y - 1); // South
            if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
            {
                // We have a neighbour to the north
                spriteName = spriteName + "S";
            }
            tile = world.GetTileByPosition(x - 1, y); // West
            if (tile != null && tile.Furniture != null && tile.Furniture.ObjectType == furniture.ObjectType)
            {
                // We have a neighbour to the north
                spriteName = spriteName + "W";
            }
            
            if (!mappedFurnituresSprites.ContainsKey(spriteName))
            {
                Debug.LogError($"Sprite:\"{spriteName}\" does not exist in spritemap");
                return null;
            }

            
          

            return mappedFurnituresSprites[spriteName];
        }

        private void OnFurnitureChanged(Furniture furniture)
        {
            //Debug.Log("OnFurnitureChanged");
            if (!mappedFurnitures.ContainsKey(furniture))
            {
                Debug.LogError($"Could not find a furniture with {furniture}");
                return;       
            }

            GameObject furnitureGameObject = mappedFurnitures[furniture];
            furnitureGameObject.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furniture);
            furnitureGameObject.GetComponent<SpriteRenderer>().color = furniture.tint;
        }

        private void OnFurnitureRemoved(Furniture furniture)
        {
            if (!mappedFurnitures.ContainsKey(furniture))
            {
                Debug.LogError($"Could not find a furniture with {furniture}");
                return;       
            }

            GameObject furnitureGameObject = mappedFurnitures[furniture];
            Destroy(furnitureGameObject);
            mappedFurnitures.Remove(furniture);
        }

        private void LoadSprites()
        {
            mappedFurnituresSprites = new Dictionary<string, Sprite>();
            Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furnitures");

            Debug.Log("FurnitureSpriteController: Resources loaded");
            foreach (Sprite item in sprites)
            {
                Debug.Log($"FurnitureSpriteController: {item.name}");
                mappedFurnituresSprites[item.name] = item;
            }
        }

    }
}