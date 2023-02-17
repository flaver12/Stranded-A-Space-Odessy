using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace org.flaver.model {
    public class ItemManager
    {
        public Dictionary<string, List<Item>> Items { get { return items; } }
        
        private Dictionary<string, List<Item>> items;

        public ItemManager()
        {
            items = new Dictionary<string, List<Item>>();
        }

        public bool InstallItem(Tile tile, Item item)
        {
            bool tileWasEmpty = tile.Item == null;
            if(!tile.InstallItem(item))
            {
                // We could not install the item
                return false;
            }
            
            CleanUpItem(item);

            if (tileWasEmpty)
            {
                if (!items.ContainsKey(tile.Item.objectType))
                {
                    items[tile.Item.objectType] = new List<Item>();
                }

                items[tile.Item.objectType].Add(tile.Item);

                World.Instance.OnItemCreated(tile.Item);
            }

            return true;
        }

        public bool InstallItem(Job job, Item item)
        {
            if (!job.itemRequirements.ContainsKey(item.objectType)) {
                Debug.LogError("Try to add item to a job that it dosent want");
                return false;
            }
            
            job.itemRequirements[item.objectType].StackSize += item.StackSize;
            
            if(job.itemRequirements[item.objectType].maxStackSize < job.itemRequirements[item.objectType].StackSize)
            {
                item.StackSize = job.itemRequirements[item.objectType].StackSize - job.itemRequirements[item.objectType].maxStackSize;
                job.itemRequirements[item.objectType].StackSize = job.itemRequirements[item.objectType].maxStackSize;
            }
            else
            {
                item.StackSize = 0;
            }
            
            CleanUpItem(item);

            return true;
        }

        public bool InstallItem(Character character, Item sourceItem, int amount = -1)
        {
            if (amount < 0)
            {
                amount = sourceItem.StackSize;
            }
            else
            {
                amount = Mathf.Min(amount, sourceItem.StackSize);
            }

            if (character.Item == null)
            {
                character.Item = sourceItem.Clone();
                character.Item.StackSize = 0;
                items[sourceItem.objectType].Add(character.Item);
            }
            else if (character.Item.objectType != sourceItem.objectType)
            {
                Debug.LogError("Character is trying to pick up a mismatch item object type");
                return false;
            }

            character.Item.StackSize += amount;
            
            if(character.Item.maxStackSize < character.Item.StackSize)
            {
                sourceItem.StackSize = character.Item.StackSize - character.Item.maxStackSize;
                character.Item.StackSize = character.Item.maxStackSize;
            }
            else
            {
                sourceItem.StackSize -= amount;
            }
            
            CleanUpItem(sourceItem);

            return true;
        }

        public Item GetClosestItemOfType(string objectType, Tile tile, int desiredAmount, bool canTakFromStockpile)
        {
            // FIXME: This method is lying!!!!
            if (!items.ContainsKey(objectType)) {
                Debug.LogError("GetClosestItemOfType: no items of desired type");
                return null;
            }
            
            foreach (Item item in items[objectType])
            {
                if (item.tile != null && (canTakFromStockpile || item.tile.Furniture == null || !item.tile.Furniture.IsStockpile()))
                {
                    return item;
                }
            }

            return null;
        }

        private void CleanUpItem(Item item)
        {
            if (item.StackSize == 0)
            {
                if (items.ContainsKey(item.objectType))
                {
                    items[item.objectType].Remove(item);
                }
                if (item.tile != null)
                {
                    item.tile.Item = null;
                    item.tile = null;
                }
                if (item.character != null)
                {
                    item.character.Item = null;
                    item.character = null;
                }
            }
        }
    }
}