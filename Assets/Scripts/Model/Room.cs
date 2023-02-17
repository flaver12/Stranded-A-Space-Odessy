using System.Collections;
using System.Collections.Generic;
using System.Linq;
using org.flaver.defintion;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using MoonSharp.Interpreter;

namespace org.flaver.model
{
    [MoonSharpUserData]
    public class Room : IXmlSerializable
    {
        public int Id {
            get
            {
                return World.Instance.GetRoomId(this);
            }
        }

        private Dictionary<string, float> atmosphericGasses;
        private List<Tile> tiles;

        public Room()
        {
            tiles = new List<Tile>();
            atmosphericGasses = new Dictionary<string, float>();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

       public void WriteXml(XmlWriter writer)
        {
            // Write a list of all this tiles to the room
            // Write Gas info
            foreach (string key in atmosphericGasses.Keys)
            {
                writer.WriteStartElement("Param");
                writer.WriteAttributeString("name", key);
                writer.WriteAttributeString("value", atmosphericGasses[key].ToString());
                writer.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader reader)
        {
            // load info
            if (reader.ReadToDescendant("Param"))
            {
                do
                {
                    string key = reader.GetAttribute("name");
                    float value = float.Parse(reader.GetAttribute("value"));
                    atmosphericGasses[key] = value;
                } while (reader.ReadToNextSibling("Param"));
            }
        }

        public void AssingTile (Tile tile)
        {
            if (tiles.Contains(tile))
            {
                return;
            }

            if (tile.room != null)
            {
                tile.room.tiles.Remove(tile); // Remove tile from old room
            }

            tile.room = this;
            tiles.Add(tile);
        }

        public bool IsOutsideRoom()
        {
            return this == World.Instance.GetOutsideRoom();
        }

        public void ChangeGas(string name, float amount)
        {
            if (IsOutsideRoom())
            {
                return;
            }

            if (atmosphericGasses.ContainsKey(name))
            {
                atmosphericGasses[name] += amount;
            }
            else
            {
                atmosphericGasses[name] = amount;
            }

            if (atmosphericGasses[name] < 0)
            {
                atmosphericGasses[name] = 0;
            }
        }
        
        public float GetGasAmount(string name)
        {
            if (atmosphericGasses.ContainsKey(name))
            {
                return atmosphericGasses[name];
            }

            return 0f;
        }

        public float GetGasPercentage(string name)
        {
            if (!atmosphericGasses.ContainsKey(name))
            {
                return 0f;
            }

            float total = 0f;

            foreach (string n in atmosphericGasses.Keys)
            {
                total += atmosphericGasses[n];
            }

            return atmosphericGasses[name] / total;
        }

        public string[] GetGasNames()
        {
            return atmosphericGasses.Keys.ToArray();
        }

        public void AssignAllTilesToOutsideRoom()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].room = World.Instance.GetOutsideRoom();
            }
            tiles = new List<Tile>();
        }

        public static void RoomFloodFill(Tile sourceTile, bool onlyIfNull = false)
        {
            // Source is the piece that may split two existing room etc.
            World world = World.Instance;
            Room oldRoom = sourceTile.room;

            if (oldRoom != null)
            {

                // Try to create new room
                foreach(Tile t in sourceTile.GetNeighbours())
                {
                    if ( t.room != null && (!onlyIfNull || t.room.IsOutsideRoom()))
                    {
                        FloodFill(t, oldRoom);
                    }
                }
                
                sourceTile.room = null;
                oldRoom.tiles.Remove(sourceTile);

                if (!oldRoom.IsOutsideRoom())
                {
                    if (oldRoom.tiles.Count > 0)
                    {
                        Debug.Log("oldRoom still have assignd room");
                    }
                    world.DeleteRoom(oldRoom);
                }
            }
            else
            {
                // old room is null, tile was propably a tile?
                FloodFill(sourceTile, null);
            }
        }

        protected static void FloodFill(Tile tile, Room oldRoom)
        {
            if (tile == null)
            {
                // Flood fill out of the map
                return;
            }

            if (tile.room != oldRoom)
            {
                // tile was already assign to an old room
                // Just return without a room
                return;
            }

            if (tile.Furniture != null && tile.Furniture.RoomEnclosure)
            {
                // Tile has a wall/door in it, we cant do room here
                return;
            }

            if (tile.Type == TileType.Empty)
            {
                // Tile is empty space, we can return
                return;
            }

            // Well now create the new room
            Room newRoom = new Room();
            Queue<Tile> tilesToCheck = new Queue<Tile>();
            tilesToCheck.Enqueue(tile);

            bool isConnectedToSpace = false;
            int processedTiles = 0;

            while (tilesToCheck.Count > 0) {
                Tile t = tilesToCheck.Dequeue();
                processedTiles ++;
                

                if (t.room != newRoom)
                {
                    newRoom.AssingTile(t);
                    Tile[] neighbours = t.GetNeighbours();
                    foreach(Tile tileToadd in neighbours) {
                        if (tileToadd == null || tileToadd.Type == TileType.Empty) // We hit open space
                        {
                            isConnectedToSpace = true;
                            /*if (oldRoom != null)
                            {
                                newRoom.UnassingAllTiles();
                                return;
                            }*/
                        }
                        else
                        {
                            if (tileToadd.room != newRoom && (tileToadd.Furniture == null || tileToadd.Furniture.RoomEnclosure == false))
                            {
                                tilesToCheck.Enqueue(tileToadd);
                            }
                        }
                    }
                }
            }

            Debug.Log($"FloodFill: Proccessed {processedTiles} for flood fill");

            if (isConnectedToSpace)
            {
                // All tiles that where found, should
                // actually be assigned to the outside
                newRoom.AssignAllTilesToOutsideRoom();
                return;
            }

            // Copy datat over
            // FIXME we could use a copy method off some kind?
            if (oldRoom != null)
            {
                newRoom.CopyGas(oldRoom);
            }
            else
            {
                // TODO
            }

            // Inform world that we have a world
            World.Instance.AddRoom(newRoom);
        }

        private void CopyGas(Room other)
        {
            foreach (string name in other.atmosphericGasses.Keys)
            {
                this.atmosphericGasses[name] = other.atmosphericGasses[name];
            }
        }
    }
}
