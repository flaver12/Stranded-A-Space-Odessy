using org.flaver.defintion;
using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using MoonSharp.Interpreter;

namespace org.flaver.model
{
    [MoonSharpUserData]
    public class Tile : IXmlSerializable
    {
        public TileType Type { 
            get {
                return type;
            }
            set {
                TileType oldType = type;
                type = value;

                // call callback onTileTypeChanged
                // only when its a new tile type
                if (tileTypeChangedCallback != null && oldType != type)
                {
                    tileTypeChangedCallback(this);
                }
            }
        }
        public int X { private set; get; }
        public int Y { private set; get; }
        public float MovementCost {
            get {
                if (Type == TileType.Empty)
                {
                    return 0; // can not walk
                }
                if (furniture == null )
                {
                    return 1;
                }

                return 1 * furniture.MovementCost;
            }
        }

        public Furniture Furniture
        {
            get {
                return furniture;
            }
            set {
                furniture = value;
            }
        }

        public Item Item
        {
            get {
                return item;
            }

            set {
                item = value;
            }
        }
        public Job PendingFurnitueJob;
        public Room room;

        private Item item;
        private World world;
        private Action<Tile> tileTypeChangedCallback;
        private TileType type;
        private Furniture furniture;

        public Tile(int x, int y)
        {
            world = World.Instance;
            X = x;
            Y = y;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter writer)
        {
            // save info
            writer.WriteAttributeString("Type", ((int)Type).ToString());
            writer.WriteAttributeString("X", X.ToString());
            writer.WriteAttributeString("Y", Y.ToString());
            writer.WriteAttributeString("RoomId", room == null ? "-1" : room.Id.ToString());
        }

        public void ReadXml(XmlReader reader)
        {
            // X,Y have all ready been processed
            // load info
            Type = (TileType)int.Parse(reader.GetAttribute("Type"));
            room = World.Instance.GetRoomFromId(int.Parse(reader.GetAttribute("RoomId")));
        }
        
        public Enterability IsEnterable()
        {
            // Can we walk trough it?
            if (MovementCost == 0)
            {
                return Enterability.Never;
            }

            // Check the furniture what his status
            if (furniture != null)
            {
                return furniture.IsEnterable();
            }

    
            return Enterability.Yes;
        }

        public Tile North()
        {
            return world.GetTileByPosition(X, Y + 1);
        }

        public Tile South()
        {
            return world.GetTileByPosition(X, Y - 1);
        }

        public Tile East()
        {
            return world.GetTileByPosition(X + 1, Y);
        }

        public Tile West()
        {
            return world.GetTileByPosition(X - 1, Y);
        }

        public void RegisterTileTypeChangedCallback(Action<Tile> callback)
        {
            tileTypeChangedCallback += callback;
        }

        public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
        {
            tileTypeChangedCallback -= callback;
        }
        
        public bool UnplaceFurniture()
        {
            if (furniture == null)
            {
                return false;
            }

            Furniture cachedFurniture = furniture;

            for (int xOff = X; xOff < (X + cachedFurniture.Width); xOff++)
            {
                for (int yOff = Y; yOff < (Y + cachedFurniture.Height); yOff++)
                {
                    Tile tile = world.GetTileByPosition(xOff, yOff);
                    tile.Furniture = null;
                }       
            }

            return true;
        }

        public bool InstallFurniture(Furniture furnitureToInstall)
        {
            if (furnitureToInstall == null)
            {
                return UnplaceFurniture();
            }

            if (!furnitureToInstall.IsValidPosition(this))
            {
                Debug.LogError("InstallFurniture: Position is not valid");
                return false;
            }

            for (int xOff = X; xOff < (X + furnitureToInstall.Width); xOff++)
            {
                for (int yOff = Y; yOff < (Y + furnitureToInstall.Height); yOff++)
                {
                    Tile tile = world.GetTileByPosition(xOff, yOff);
                    tile.Furniture = furnitureToInstall;
                }       
            }
            return true;
        }

        public bool InstallItem(Item itemToInstall)
        {
             if (itemToInstall == null)
            {
                // Uninstall item
                item = null;
                return true;
            }

            // We have a obj all ready
            if (item != null)
            {
                // There are some items still here, is it possibly to create a stack?
                if (item.objectType != itemToInstall.objectType)
                {
                    Debug.LogError("Try to assign a diffrent object type to tile, that not maches current");
                    return false;
                }

                // Merging
                int numToMove = itemToInstall.StackSize;
                if (item.StackSize + numToMove > item.maxStackSize)
                {
                    numToMove = item.maxStackSize - item.StackSize;
                }

                item.StackSize += numToMove;
                itemToInstall.StackSize -= numToMove;
                
                return true;
            }

            item = itemToInstall.Clone();
            item.tile = this;
            itemToInstall.StackSize = 0; // Reset old stack

            return true;
        }

        public bool IsNeighbour(Tile tile, bool checkDiagonalMovement = false)
        {
            return Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Y - tile.Y) == 1 // Check horizontal/vertical adjustment
                ||
                (checkDiagonalMovement && ( Mathf.Abs(this.X - tile.X) == 1 && Mathf.Abs( this.Y - tile.Y ) == 1 )) // Check diag
            ;
        }

        public Tile[] GetNeighbours(bool checkDiagonal = false)
        {
            Tile[] tiles;
            if (!checkDiagonal)
            {
                tiles = new Tile[4]; // order: N E S W
            }
            else
            {
                tiles = new Tile[8]; // order: N E S W NE SE SW NW
            }

            Tile possibleNeighbour;
            possibleNeighbour = world.GetTileByPosition(X, Y + 1);
            tiles[0] = possibleNeighbour;

            possibleNeighbour = world.GetTileByPosition(X + 1, Y);
            tiles[1] = possibleNeighbour;

            possibleNeighbour = world.GetTileByPosition(X, Y - 1);
            tiles[2] = possibleNeighbour;

            possibleNeighbour = world.GetTileByPosition(X - 1, Y);
            tiles[3] = possibleNeighbour;

            if (checkDiagonal)
            {
                possibleNeighbour = world.GetTileByPosition(X + 1, Y + 1);
                tiles[4] = possibleNeighbour;

                possibleNeighbour = world.GetTileByPosition(X + 1, Y - 1);
                tiles[5] = possibleNeighbour;

                possibleNeighbour = world.GetTileByPosition(X - 1, Y - 1);
                tiles[6] = possibleNeighbour;

                possibleNeighbour = world.GetTileByPosition(X - 1, Y + 1);
                tiles[7] = possibleNeighbour;
            }

            return tiles;
        }
    }
}