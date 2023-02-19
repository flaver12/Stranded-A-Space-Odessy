using System.Collections;
using System.Collections.Generic;
using org.flaver.model;
using TMPro;
using UnityEngine;

namespace org.flaver.controller {
    public class ItemSpriteController : MonoBehaviour
    {
        public GameObject itemUiPrefab;

        private Dictionary<Item, GameObject> mappedItems;
        private World world { get { return WorldController.Instance.World; } }

        private void Start()
        {               
            // Init map of which tile belongs to which gameobject
            mappedItems = new Dictionary<Item, GameObject>();

            // Register world callbacks
            world.RegisterItemCreated(OnItemCreated);

            // Go trough all EXISTING item, call on created event manully
            foreach (string objectType in world.itemManager.Items.Keys)
            {
                foreach (Item entry in world.itemManager.Items[objectType])
                {
                    OnItemCreated(entry);
                }
            }
        }

        public void OnItemCreated(Item item)
        {
            Debug.Log("OnItemCreated");
            // Create a visual game object lined to the data
            GameObject itemGameObject = new GameObject();
            itemGameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            itemGameObject.name = item.objectType;

            mappedItems.Add(item, itemGameObject); // Register mapped item

            itemGameObject.transform.position = new Vector3(item.tile.X, item.tile.Y, 0f);
            itemGameObject.transform.SetParent(transform, true); // true = stay in world pos
            itemGameObject.AddComponent<SpriteRenderer>().sprite = SpriteManager.Instance.GetSprite("Items", item.objectType);
            itemGameObject.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Items");

            if (item.maxStackSize > 1)
            {
                // Stack of item add item ui component
                GameObject uiGameObject = Instantiate(itemUiPrefab);
                uiGameObject.transform.SetParent(itemGameObject.transform);
                uiGameObject.transform.localPosition = Vector3.zero; // If we change the sprite pivot we need to 
                uiGameObject.GetComponentInChildren<TMP_Text>().text = item.StackSize.ToString();
            }

            // Object infos changes
            // FIXME add on change callback
            item.RegisterItemChangedCallback(OnItemChanged);
        }

        private void OnItemChanged(Item item)
        {
            if (!mappedItems.ContainsKey(item))
            {
                Debug.LogError("OnItemChanged, can not change visuals, item not mapped");
                return;
            }

            GameObject itemGameObject = mappedItems[item];
            if (item.StackSize > 0)
            {
                TMP_Text text = itemGameObject.GetComponentInChildren<TMP_Text>();
                
                if (text != null)
                {
                    text.text = item.StackSize.ToString();
                }
            }
            else
            {
                // Remove sprite
                Destroy(itemGameObject);
                mappedItems.Remove(item);
                item.UnregisterItemChangedCallback(OnItemChanged);
            }
        }
    }
}