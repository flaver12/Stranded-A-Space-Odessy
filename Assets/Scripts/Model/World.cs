using System.Collections;
using System.Collections.Generic;
using org.flaver.defintion;
using UnityEngine;
using System;
using org.flaver.pathfinding;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace org.flaver.model
{
    public class World : IXmlSerializable
    {
        public int Width { private set; get; }
        public int Height { private set; get; }
        public JobQueue JobQueue { private set; get; }
        public List<Furniture> Furnitures { get { return furnitures; } }
        public List<Character> Characters { get { return characters; } }
        public List<Room> Rooms { get { return rooms; } }
        public ItemManager itemManager;
        public TileGraph TileGraph { set; get; }
        public Dictionary<string, Job> FurnitureJobPrototypes { get { return furnitureJobPrototypes; } }
        public Dictionary<string, Furniture> FurniturePrototypes { get { return furniturePrototypes; } }
        public static World Instance { get; private set; }

        private Tile[,] tiles;
        private Dictionary<string, Furniture> furniturePrototypes;
        private Dictionary<string, Job> furnitureJobPrototypes;
        private Action<Furniture> installedFurnitureCreated;
        private Action<Tile> tileChanged;
        private Action<Character> characterCreated;
        private Action<Item> itemCreated;
        private List<Character> characters;
        private List<Furniture> furnitures;
        private List<Room> rooms;

        
        public World()
        {

        }
        public World(int width, int height)
        {
            SetupWord(width, height);
            CreateCharacter(GetTileByPosition(Width / 2, Height / 2));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter writer)
        {
            // save info
            writer.WriteAttributeString("Width", Width.ToString());
            writer.WriteAttributeString("Height", Height.ToString());

            writer.WriteStartElement("Rooms");
            foreach(Room item in rooms)
            {
                // Dont save the outside room
                if (GetOutsideRoom() == item)
                {
                    continue;
                }
                // <Room>
                writer.WriteStartElement("Room");
                item.WriteXml(writer);
                writer.WriteEndElement();
                // </Room>
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Tiles");
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (tiles[x,y].Type != TileType.Empty)
                    {
                        // <tile>
                        writer.WriteStartElement("Tile");
                        tiles[x,y].WriteXml(writer);
                        writer.WriteEndElement();
                        // </tile>
                    }
                }
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Furnitures");
            foreach(Furniture item in furnitures)
            {
                // <Furniture>
                writer.WriteStartElement("Furniture");
                item.WriteXml(writer);
                writer.WriteEndElement();
                // </Furniture>
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Characters");
            foreach(Character item in characters)
            {
                // <Character>
                writer.WriteStartElement("Character");
                item.WriteXml(writer);
                writer.WriteEndElement();
                // </Character>
            }
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            // load info
            Debug.Log("ReadXml called");

            Width = int.Parse(reader.GetAttribute("Width"));
            Height = int.Parse(reader.GetAttribute("Height"));

            SetupWord(Width, Height);

            while(reader.Read())
            {
                switch(reader.Name)
                {
                    case "Rooms":
                        ReadXmlRooms(reader);
                        break;
                    case "Tiles":
                        ReadXmlTiles(reader);
                        break;
                    case "Furnitures":
                        ReadXmlFurnitures(reader);
                        break;
                    case "Characters":
                        ReadXmlCharacters(reader);
                        break;

                }
            }

           // Debug only remove later
            Item item = new Item();
            item.StackSize = 50;
            itemManager.InstallItem(GetTileByPosition(Width / 2, Height / 2), item);
            if (itemCreated != null)
            {
                itemCreated(GetTileByPosition(Width / 2, Height / 2).Item);
            }

            Item item2 = new Item();
            item2.StackSize = 4;
            itemManager.InstallItem(GetTileByPosition(Width / 2 + 2, Height / 2), item2);
            if (itemCreated != null)
            {
                itemCreated(GetTileByPosition(Width / 2 + 2, Height / 2).Item);
            }

            Item item3 = new Item();
            item3.StackSize = 3;
            itemManager.InstallItem(GetTileByPosition(Width / 2 + 1, Height / 2 + 2), item3);
            if (itemCreated != null)
            {
                itemCreated(GetTileByPosition(Width / 2 + 1, Height / 2 + 2).Item);
            }
        }

        public void SetFurnitureJobPrototype(Job job, Furniture furniture)
        {
            furnitureJobPrototypes[furniture.ObjectType] = job;
        }

        public Room GetOutsideRoom()
        {
            return rooms[0];
        }

        public int GetRoomId(Room room)
        {
            return rooms.IndexOf(room);
        }

        public Room GetRoomFromId(int id)
        {
            if (id < 0 || id > rooms.Count - 1)
            {
                return null;
            }
            return rooms[id];
        }

        public void DeleteRoom(Room room)
        {
            if (room == GetOutsideRoom())
            {
                Debug.LogError("World: Tried to delete the outside room");
                return;
            }
            
            rooms.Remove(room);

            room.AssignAllTilesToOutsideRoom();
        }

        public void AddRoom(Room room)
        {
            rooms.Add(room);
        }

        private void SetupWord(int width, int height)
        {
            JobQueue = new JobQueue();
            // Setup instance
            // TODO what when we clear the world?
            Instance = this;

            this.Width = width;
            this.Height = height;

            tiles = new Tile[width, height];
            
            rooms = new List<Room>();
            rooms.Add(new Room()); // Add outside

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    tiles[x,y] = new Tile(x, y);
                    tiles[x,y].RegisterTileTypeChangedCallback(OnTileChanged);
                    tiles[x,y].room = GetOutsideRoom(); // Rooms 0 is all ways outside
                }
            }

            Debug.Log($"World generated with {width*height} tiles");
            CreateFurniturePrototypes();
            characters = new List<Character>();
            furnitures = new List<Furniture>();
            itemManager = new ItemManager();
        }

        public Character CreateCharacter(Tile tile)
        {
            Debug.Log("CreateCharacter");
            Character character = new Character(tile);
            characters.Add(character);
            if (characterCreated != null)
            {
                characterCreated(character);
            }

            return character;
        }

        public void SetupPathfindingExample()
        {
            Debug.Log("SetupPathfindingExample");

            int l = Width / 2 - 5;
            int b = Height / 2 - 5;

            for (int x = l-5; x < l + 15; x++) {
                for (int y = b-5; y < b + 15; y++)
                {
                    tiles[x,y].Type = TileType.Floor;

                    if (x == l || x == (l + 9) || y == b || y == (b + 9))
                    {
                        if (x != (l + 9) && y != (b + 4))
                        {
                            PlaceFurniture("furn_SteelWall", tiles[x,y]);
                        }
                    }
                }
            }
        }

        public void Tick(float deltaTime)
        {
            foreach (Character item in characters)
            {
                item.Tick(deltaTime);
            }

            foreach (Furniture item in furnitures)
            {
                item.Tick(deltaTime);
            }
        }

        public Furniture PlaceFurniture(string objectType, Tile tile, bool doRoomFloodFill = true)
        {
            // TODO fix the only 1x1 tile
            if (!furniturePrototypes.ContainsKey(objectType))
            {
                Debug.LogError($"InstalledObject does not contain a prototype with a key {objectType}");
                return null;
            }

            Furniture obj = Furniture.PlacePrototype(furniturePrototypes[objectType], tile);
            if (obj == null)
            {
                return null;
            }

            obj.RegisterOnRemoved(OnFurnitureRemoved);
            furnitures.Add(obj);

            if (doRoomFloodFill && obj.RoomEnclosure)
            {
                Room.RoomFloodFill(tile);
            }

            if (installedFurnitureCreated != null)
            {
                installedFurnitureCreated(obj);
                if (obj.MovementCost != 1)
                {
                    InvalidateTileGraph(); // Reset the pathfinding   
                }
            }

            return obj;
        }

        public void RandTiles()
        {
            Debug.Log("RandTiles");
            // Loop trough all the tiles
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (UnityEngine.Random.Range(0,2) == 0)
                    {
                        tiles[x,y].Type =  TileType.Empty;
                    }
                    else
                    {
                        tiles[x,y].Type =  TileType.Floor;
                    }
                }
            }
        }

        public void OnItemCreated(Item item)
        {
            if (itemCreated != null)
            {
                itemCreated(item);
            }
        }

        public Tile GetTileByPosition(int x, int y)
        {
            if (x >= Width || x < 0 || y >= Height || y < 0)
            {
                // Debug.LogError($"Tile width position {x},{y} is out for range");
                return null;
            }
            return tiles[x,y];
        }

        public void RegisterFurnitureCreated(Action<Furniture> callback)
        {
            installedFurnitureCreated += callback;
        }

        public void UnregisterFurnitureCreated(Action<Furniture> callback)
        {
            installedFurnitureCreated -= callback;
        }

        public void RegisterTileChanged(Action<Tile> callback)
        {
            tileChanged += callback;
        }

        public void UnregisterTileChanged(Action<Tile> callback)
        {
            tileChanged -= callback;
        }

        public void RegisterCharacterCreated(Action<Character> callback)
        {
            characterCreated += callback;
        }

        public void UnregisterCharacterCreated(Action<Character> callback)
        {
            characterCreated -= callback;
        }

        public void RegisterItemCreated(Action<Item> callback)
        {
            itemCreated += callback;
        }

        public void UnregisterItemCreated(Action<Item> callback)
        {
            itemCreated -= callback;
        }

        public bool IsFurniturePlacementValid(string furnitureType, Tile tile)
        {
            return furniturePrototypes[furnitureType].IsValidPosition(tile);
        }

        public Furniture GetFurniturePrototype(string objectType)
        {
            if (!furniturePrototypes.ContainsKey(objectType))
            {
                Debug.LogError($"No furniture with type {objectType} found");
                return null;
            }
            return furniturePrototypes[objectType];
        }

        // Call when the world changed
        // We have to regenerate the graph
        public void InvalidateTileGraph()
        {
            TileGraph = null;
        }

        public void OnFurnitureRemoved(Furniture furniture)
        {
            furnitures.Remove(furniture);
        }

        private void ReadXmlTiles(XmlReader reader)
        {
            if(reader.ReadToDescendant("Tile"))
            {
                // We found a tile
                do {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    tiles[x,y].ReadXml(reader);
                } while(reader.ReadToNextSibling("Tile"));
            }
        }

        private void ReadXmlFurnitures(XmlReader reader)
        {
            if(reader.ReadToDescendant("Furniture"))
            {
              do
              {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                
                Furniture furn = PlaceFurniture(reader.GetAttribute("ObjectType"), tiles[x,y], false);
                furn.ReadXml(reader);
              } while (reader.ReadToNextSibling("Furniture"));

              /*foreach (Furniture furn in furnitures)
              {
                Room.RoomFloodFill(furn.Tile, true);
              }*/
            }
        }

        private void ReadXmlRooms(XmlReader reader)
        {
            if(reader.ReadToDescendant("Room"))
            {
              do
              {
                /*int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                
                Furniture furn = PlaceFurniture(reader.GetAttribute("ObjectType"), tiles[x,y], false);
                furn.ReadXml(reader);
                */

                Room room = new Room();
                rooms.Add(room);
                room.ReadXml(reader);
              } while (reader.ReadToNextSibling("Room"));

              foreach (Furniture furn in furnitures)
              {
                Room.RoomFloodFill(furn.Tile, true);
              }
            }
        }

        private void ReadXmlCharacters(XmlReader reader)
        {
            Debug.Log("ReadXmlCharacters");
            if(reader.ReadToDescendant("Character"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    Debug.Log($"Create character at position {x}, {y}");
                    Character character = CreateCharacter(tiles[x,y]);
                    character.ReadXml(reader);
                } while (reader.ReadToNextSibling("Character"));

            }
        }

        private void OnTileChanged(Tile tile)
        {
            if (tileChanged == null)
            {
                return;
            }

            tileChanged(tile);
            InvalidateTileGraph();
        }

        private void LoadFurnitureLua()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "Lua", "Furniture.lua");
            string furnitureLuaRaw = File.ReadAllText(filePath);

            Debug.Log($"LoadFurnitureLua: Loaded this code: {furnitureLuaRaw}");
            FurnitureActions furnitureActions = new FurnitureActions(furnitureLuaRaw);
        }

        private void CreateFurniturePrototypes()
        {
            LoadFurnitureLua();

            furniturePrototypes = new Dictionary<string, Furniture>();
            furnitureJobPrototypes = new Dictionary<string, Job>();

            // TODO passed raw text/BufferStream etc.
            string filePath = Path.Combine(Application.streamingAssetsPath, "Data", "Furniture.xml");
            string furnitureXmlRaw = File.ReadAllText(filePath);

            XmlTextReader reader = new XmlTextReader( new StringReader(furnitureXmlRaw));
            
            int furnitureCount = 0;
            if (reader.ReadToDescendant("Furnitures"))
            {
                if(reader.ReadToDescendant("Furniture"))
                {
                    do
                    {
                       furnitureCount ++;
                       Furniture furniture = new Furniture();
                       furniture.ReadXmlPrototype(reader);

                       furniturePrototypes[furniture.ObjectType] = furniture;
                    } while (reader.ReadToNextSibling("Furniture"));
                }
                else
                {
                    Debug.LogError("CreateFurniturePrototypes: Did not find a Furniture defintion");
                }
               
            }
            else
            {
                Debug.LogError("CreateFurniturePrototypes: Did not find a Furnitures defintion");
            }
            Debug.Log($"CreateFurniturePrototypes: Loaded {furnitureCount} furnitures");

            // This bit will come from a lua file
//            furniturePrototypes["Door"].RegisterTickAction(FurnitureActions.DoorTickAction);
//            furniturePrototypes["Door"].isEnterable = FurnitureActions.IsDoorEnterable;
            // Read furnitures prototype xml
        }
/*
        private void CreateFurniturePrototypes()
        {
            furniturePrototypes = new Dictionary<string, Furniture>();
            furnitureJobPrototypes = new Dictionary<string, Job>();

            Furniture wallPrototype = new Furniture(
                "furn_SteelWall",
                0, // not passable
                1, // width
                1, // height
                true, // links to neighbours
                true // Enclose rooms
            );
            Job wallPrototypeJob = new Job(null, "furn_SteelWall", FurnitureActions.JobCompleteFurnitureBuilding, 1f, new Item[]{ new Item("Steel Plate", 5, 0) });
            wallPrototype.Name = "Basic Wall";

            Furniture doorPrototype = new Furniture(
                "Door",
                1, // dooor cost
                1, // width
                1, // height
                false, // links to neighbours
                true // Enclose rooms
            );
            doorPrototype.SetParameteter("openness", 0f);
            doorPrototype.SetParameteter("isOpening", 0f);
            doorPrototype.RegisterTickAction(FurnitureActions.DoorTickAction);
            doorPrototype.isEnterable = FurnitureActions.IsDoorEnterable;

            Furniture stockpilePrototype = new Furniture(
                "Stockpile",
                1, // not passable
                1, // width
                1, // height
                true, // links to neighbours
                false // Enclose rooms
            );
            stockpilePrototype.tint = new Color32(186, 31, 31, 255); 
            Job stockPilePrototypeJob = new Job(null, "Stockpile", FurnitureActions.JobCompleteFurnitureBuilding, -1, null);
            stockpilePrototype.RegisterTickAction(FurnitureActions.StockpileTickAction);

            Furniture o2GeneratorPrototype = new Furniture(
                "Oxygen Generator",
                10, // dooor cost
                2, // width
                2, // height
                false, // links to neighbours
                false // Enclose rooms
            );
            o2GeneratorPrototype.RegisterTickAction(FurnitureActions.OxygenGeneratorTickAction);

            Furniture miningDroneStationPrototype = new Furniture(
                "Mining Drone Station",
                1, // dooor cost
                3, // width
                3, // height
                false, // links to neighbours
                false // Enclose rooms
            );
            miningDroneStationPrototype.jobSpotOffset = new Vector2(1, 0);
            miningDroneStationPrototype.RegisterTickAction(FurnitureActions.MiningDroneStationTickAction);

            // Register the stuff
            furniturePrototypes.Add("furn_SteelWall", wallPrototype);
            furnitureJobPrototypes.Add("furn_SteelWall", wallPrototypeJob);

            furniturePrototypes.Add("Stockpile", stockpilePrototype);
            furnitureJobPrototypes.Add("Stockpile", stockPilePrototypeJob);

            furniturePrototypes.Add("Door", doorPrototype);

            furniturePrototypes.Add("Oxygen Generator", o2GeneratorPrototype);

            furniturePrototypes.Add("Mining Drone Station", miningDroneStationPrototype);
        }
*/

    }
}