using System.Collections;
using System.Collections.Generic;
using System.Linq;
using org.flaver.defintion;
using org.flaver.model;
using UnityEngine;

namespace org.flaver.controller
{
    public class TileSpriteController : MonoBehaviour
    {
        // TODO fix me
        public Sprite floorSprite;
        public Sprite emptySprite;

        private Dictionary<Tile, GameObject> mappedTiles;
        private World world { get { return WorldController.Instance.World; } }
        private void Start()
        { 
            // Init map of which tile belongs to which gameobject
            mappedTiles = new Dictionary<Tile, GameObject>();

            // Create the World empty
            for (int x = 0; x < world.Width; x++)
            {
                for (int y = 0; y < world.Height; y++)
                {
                    GameObject tileObject = new GameObject();
                    Tile tile = world.GetTileByPosition(x,y);
                    tileObject.name = $"Tile({x}, {y})";

                    mappedTiles.Add(tile, tileObject); // Register mapped tile

                    tileObject.transform.position = new Vector3(tile.X, tile.Y, 0f);
                    tileObject.transform.SetParent(transform, true); // true = stay in world pos
                    SpriteRenderer renderer = tileObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = emptySprite;
                    renderer.sortingLayerName = "Tiles";

                    OnTileChange(tile);
                }
            }
            
            // Register world callbacks
            world.RegisterTileChanged(OnTileChange);
        }

        private void OnTileChange(Tile tile)
        {
            if (!mappedTiles.ContainsKey(tile))
            {
                Debug.LogError("Given tile is not mapped to any gameobject");
                return;
            }

            GameObject tileGameObject = mappedTiles[tile];
            if (tileGameObject == null)
            {
                Debug.LogError("Given tile is mapped but has a invalid gameobject attached");
                return;
            }

            if (tile.Type == TileType.Floor)
            {
                tileGameObject.GetComponent<SpriteRenderer>().sprite = floorSprite;
            }
            else if (tile.Type == TileType.Empty)
            {
                tileGameObject.GetComponent<SpriteRenderer>().sprite = emptySprite;
            }
            else
            {
                Debug.LogError("OnTileTypeChanged invalid tile type");
            }
        }

        private void DestroyAllTileGameObjects()
        {
            // This will only remove the gameobject and its data
            // it will NOT remove the tile it self
            while (mappedTiles.Count > 0)
            {
                Tile tile = mappedTiles.Keys.First();
                GameObject tileGameObject = mappedTiles[tile];

                mappedTiles.Remove(tile);
                tile.UnregisterTileTypeChangedCallback(OnTileChange);

                Destroy(tileGameObject);
            }
        }
    }
}