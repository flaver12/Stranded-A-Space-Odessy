using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace org.flaver.model
{
    public class Item // like stokepile on the floor
    {
        public string objectType = "Steel Plate";
        public int maxStackSize = 50;
        public int StackSize {
            get
            {
                return stackSize;
            }
            set
            {
                if (stackSize != value)
                {
                    stackSize = value;
                    if (tile != null && itemChangedCallback != null)
                    {
                        itemChangedCallback(this);
                    }
                }
            }
        }
        public Tile tile;
        public Character character;

        private int stackSize = 1;
        private Action<Item> itemChangedCallback;


        public Item()
        {

        }

        public Item(string objectType, int maxStackSize, int stackSize)
        {
            this.objectType = objectType;
            this.maxStackSize = maxStackSize;
            this.StackSize = stackSize;
        }

        protected Item(Item toClone) // Copy constructor
        {
            objectType = toClone.objectType;
            maxStackSize = toClone.maxStackSize;
            StackSize = toClone.StackSize;
        }

        public virtual Item Clone()
        {
            return new Item(this);
        }

        public void RegisterItemChangedCallback(Action<Item> callback)
        {
            itemChangedCallback += callback;
        }

        public void UnregisterItemChangedCallback(Action<Item> callback)
        {
            itemChangedCallback -= callback;
        }

    }
}