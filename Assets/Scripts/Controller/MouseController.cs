using System.Collections;
using System.Collections.Generic;
using org.flaver.defintion;
using org.flaver.model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace org.flaver.controller
{
    public class MouseController : MonoBehaviour
    {
        public GameObject cursorCirclePrefab;

        private Vector3 lastMousePosition;
        private Vector3 transformedMousePosition;
        private Vector3 dragStartPosition;
        private List<GameObject> dragPreviewGameObjects = new List<GameObject>();
        private BuildModeController buildModeController;
        private bool isDragging = false;
        private FurnitureSpriteController furnitureSpriteController;
        private enum MouseMode
        {
            Select,
            Build
        }
        private MouseMode currentMode = MouseMode.Select;

        public Vector3 GetMousePositionInWorldSpace()
        {
            return transformedMousePosition;
        }

        public Tile GetMouseOverTile()
        {
            return WorldController.Instance.GetTileAtWorldCord(transformedMousePosition);
        }

        public void StartBuildMode()
        {
            currentMode = MouseMode.Build;
        }

        private void Start()
        {
            buildModeController = GameObject.FindObjectOfType<BuildModeController>();
            furnitureSpriteController = GameObject.FindObjectOfType<FurnitureSpriteController>();
        }

        private void Update()
        {
            // Convert screen space to world space
            transformedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transformedMousePosition.z = 0;

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (currentMode == MouseMode.Build )
                {
                    currentMode = MouseMode.Select;
                }
                else if(currentMode == MouseMode.Select)
                {
                    Debug.Log("Show game menu?");
                }
            }

            // UpdateCursor();
            UpdateDragging();
            UpdateCameraMovement();

            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lastMousePosition.z = 0;
        }

        private void UpdateDragging()
        {
            // If over ui element go away!
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Clean up old dragPreview
            while(dragPreviewGameObjects.Count > 0)
            {
                GameObject go = dragPreviewGameObjects[0];
                dragPreviewGameObjects.RemoveAt(0);
                SimplePool.Despawn(go);
            }

            if (currentMode != MouseMode.Build)
            {
                return;
            }

            // Handle left mouse
            // Start drag
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragStartPosition = transformedMousePosition;
            }
            else if (!isDragging)
            {
                dragStartPosition = transformedMousePosition;
            }

            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape)) // Right mouse button was released so we are canlce any drag/build
            {
                isDragging = false;
            }

            if (!buildModeController.IsObjectDraggable())
            {
                dragStartPosition = transformedMousePosition;
            }

            int startX = Mathf.FloorToInt(dragStartPosition.x + 0.5f);
            int endX = Mathf.FloorToInt(transformedMousePosition.x + 0.5f);
            int startY = Mathf.FloorToInt(dragStartPosition.y+ 0.5f);
            int endY = Mathf.FloorToInt(transformedMousePosition.y + 0.5f);

            // just a simple flip
            if (endX < startX)
            {
                int tmp = endX;
                endX = startX;
                startX = tmp;
            }

            // just a simple flip
            if (endY < startY)
            {
                int tmp = endY;
                endY = startY;
                startY = tmp;
            }

            //if (isDragging)
            //{
                // Display a preview of the drag
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        Tile tile = WorldController.Instance.World.GetTileByPosition(x,y);
                        if (tile != null)
                        {
                            if (buildModeController.buildMode == BuildMode.Furniture)
                            {
                                ShowFurnitureSpriteAtTile(buildModeController.buildModeObjectType, tile);
                            }
                            else
                            {
                                // Show generic dragging
                                GameObject go = SimplePool.Spawn(cursorCirclePrefab, new Vector3(x,y,0), Quaternion.identity);
                                go.transform.SetParent(transform, true);
                                dragPreviewGameObjects.Add(go);
                            }
                            // Display the building hint on top
                        }
                    }
                }

            //}

            // End drag
            if (isDragging && Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                // If start and end are the same
                // We still want to loop trough
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        Tile tile = WorldController.Instance.World.GetTileByPosition(x,y);
                        
                        if (tile != null)
                        {
                           // Call BuildModeController::Build()
                           buildModeController.Build(tile);
                        }
                    }
                }
            }
        }

        private void ShowFurnitureSpriteAtTile(string furnitureType, Tile tile)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(transform, true);
            dragPreviewGameObjects.Add(go);

            SpriteRenderer spriteRender = go.AddComponent<SpriteRenderer>();
            spriteRender.sprite = furnitureSpriteController.GetSpriteForFurniture(furnitureType);
            spriteRender.sortingLayerID = SortingLayer.NameToID("Furniture");

            if (WorldController.Instance.World.IsFurniturePlacementValid(furnitureType, tile))
            {
                spriteRender.color = new Color(0.5f, 1f, 0.5f, 0.25f);
            }
            else
            {
                spriteRender.color = new Color(1f, 0.5f, 0.5f, 0.25f);
            }
            
            Furniture proto = World.Instance.FurniturePrototypes[furnitureType];
            go.transform.position = new Vector3(tile.X + ((proto.Width - 1) / 2f), tile.Y + ((proto.Height - 1) / 2f), 0f);
        }

        private void UpdateCameraMovement()
        {
            // Screen moving
            if (Input.GetMouseButton(2) || Input.GetMouseButton(1)) // right or middle mouse button down
            {
                Vector3 diff = lastMousePosition - transformedMousePosition;
                Camera.main.transform.Translate(diff);
            }

            Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);
        }

        private Tile GetTileAtWorldPosition(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.y);

            return WorldController.Instance.World.GetTileByPosition(x, y);
        }
    }
}