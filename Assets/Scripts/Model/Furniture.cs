using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using org.flaver.defintion;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using MoonSharp.Interpreter;

namespace org.flaver.model
{
    [MoonSharpUserData]
    public class Furniture : IXmlSerializable // Like wals doors and furnitures
    {
        public Tile Tile { private set; get; }
        public string ObjectType { private set; get; }
        public bool LinksToNeighbour { private set; get; }
        public Action<Furniture> onChanged;
        public Action<Furniture> onRemoved;
        public Color tint = Color.white;
        public string Name {
            get
            {
                if (name == null || name.Length == 0)
                {
                    return ObjectType;
                }
                
                return name;
            } 
            set
            {
                name = value;
            }
        }
        public float MovementCost {
            get {
                return movementCost;
            }

            private set {
                movementCost = value;
            }
        }
        public int Width {
            get {
                return width;
            }

            private set {
                width = value;
            }
        }
        public int Height {
            get {
                return height;
            }

            private set {
                height = value;
            }
        }
        public bool RoomEnclosure {
            get
            {
                return roomEnclosure;
            }

            private set
            {
                roomEnclosure = value;
            }
        }
        public Vector2 jobSpotOffset = Vector2.zero;
        public Vector2 jobSpawnSpotOffset = Vector2.zero;

        private Func<Tile, bool> funcPositionValidation;
        private float movementCost;
        private int width;
        private int height;
        private bool roomEnclosure;
        private Dictionary<string, float> furnParameters;
        private string IsEnterableTickAction;
        private List<string> tickActions;
        private List<Job> jobs;
        private string name;

        public Furniture()
        {
            tickActions = new List<string>();
            furnParameters = new Dictionary<string, float>();
            jobs = new List<Job>();
            this.funcPositionValidation = this.DEFAULT___IsValidPosition;
        }

        protected Furniture(Furniture toCopy) // copy constrcutor
        {
            ObjectType = toCopy.ObjectType;
            movementCost = toCopy.MovementCost;
            roomEnclosure = toCopy.RoomEnclosure;
            width = toCopy.Width;
            height = toCopy.Height;
            tint = toCopy.tint;
            LinksToNeighbour = toCopy.LinksToNeighbour;
            furnParameters = new Dictionary<string, float>(toCopy.furnParameters);
            jobs = new List<Job>();
            jobSpotOffset = toCopy.jobSpotOffset;
            jobSpawnSpotOffset = toCopy.jobSpawnSpotOffset;
            Name = toCopy.Name;
            IsEnterableTickAction = toCopy.IsEnterableTickAction;

            if (toCopy.tickActions != null)
            {
                tickActions = new List<string>(toCopy.tickActions);
            }

            if (toCopy.funcPositionValidation != null)
            {
                this.funcPositionValidation = (Func<Tile, bool>)toCopy.funcPositionValidation.Clone();
            }

        }

        /*public Furniture (string objectType, float movementCost = 2f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false)
        {
            ObjectType = objectType;
            this.movementCost = movementCost;
            this.width = width;
            this.height = height;
            this.roomEnclosure = roomEnclosure;
            LinksToNeighbour = linksToNeighbour;

            this.furnParameters = new Dictionary<string, float>();
            this.funcPositionValidation = this.DEFAULT___IsValidPosition;
        }*/

        public static Furniture PlacePrototype(Furniture prototype, Tile tile)
        {
            if (!prototype.funcPositionValidation(tile))
            {
                Debug.LogError("PlacePrototype -- invalid position");
                return null;
            }

            // Shadow copy
            Furniture obj = prototype.Clone();
            obj.Tile = tile;

            if (!tile.InstallFurniture(obj))
            {
                // We could not install the object on the tile
                return null;
            }

            if (obj.LinksToNeighbour)
            {
                // This furniture has to inform the neighbours that it has been placed
                // When you have a wall we need to inform them to change there sprite
                int x = tile.X;
                int y = tile.Y; 
                tile = World.Instance.GetTileByPosition(x, y + 1); // North
                if (tile != null && tile.Furniture != null && tile.Furniture.onChanged != null && tile.Furniture.ObjectType == obj.ObjectType)
                {
                    tile.Furniture.onChanged(tile.Furniture);
                }
                tile = World.Instance.GetTileByPosition(x + 1, y); // East
                if (tile != null && tile.Furniture != null && tile.Furniture.onChanged != null && tile.Furniture.ObjectType == obj.ObjectType)
                {
                    tile.Furniture.onChanged(tile.Furniture);
                }
                tile = World.Instance.GetTileByPosition(x, y - 1); // South
                if (tile != null && tile.Furniture != null && tile.Furniture.onChanged != null && tile.Furniture.ObjectType == obj.ObjectType)
                {
                    tile.Furniture.onChanged(tile.Furniture);
                }
                tile = World.Instance.GetTileByPosition(x - 1, y); // West
                if (tile != null && tile.Furniture != null && tile.Furniture.onChanged != null && tile.Furniture.ObjectType == obj.ObjectType)
                {
                    tile.Furniture.onChanged(tile.Furniture);
                }
            }

            return obj;
        }

        public void Tick(float deltaTime)
        {
            if (tickActions != null)
            {
                //tickActions(this, deltaTime);
                FurnitureActions.CallFunctionsWithFurniture(tickActions.ToArray(), this, deltaTime);
            }
        }

        public virtual Furniture Clone()
        {
            return new Furniture(this);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

       public void WriteXml(XmlWriter writer)
        {
            // save info
            writer.WriteAttributeString("X", Tile.X.ToString());
            writer.WriteAttributeString("Y", Tile.Y.ToString());
            writer.WriteAttributeString("ObjectType", ObjectType);
            // writer.WriteAttributeString("movementCost", movementCost.ToString());

            foreach (string key in furnParameters.Keys)
            {
                writer.WriteStartElement("Param");
                writer.WriteAttributeString("name", key);
                writer.WriteAttributeString("value", furnParameters[key].ToString());
                writer.WriteEndElement();
            }
        }
        
        public void ReadXmlPrototype(XmlReader parentReader)
        {
            Debug.Log("ReadXmlPrototype");
            
            ObjectType = parentReader.GetAttribute("objectType");

            XmlReader reader = parentReader.ReadSubtree();
            while(reader.Read())
            {
                switch(reader.Name)
                {
                    case "Name":
                        reader.Read();
                        Name = reader.ReadContentAsString();
                        break;
                    case "MovementCost":
                        reader.Read();
                        movementCost = reader.ReadContentAsFloat();
                        break;
                    case "Width":
                        reader.Read();
                        width = reader.ReadContentAsInt();
                        break;
                    case "Height":
                        reader.Read();
                        height = reader.ReadContentAsInt();
                        break;
                    case "LinksToNeighbours":
                        reader.Read();
                        LinksToNeighbour = reader.ReadContentAsBoolean();
                        break;
                    case "EnclosesRooms":
                        reader.Read();
                        roomEnclosure = reader.ReadContentAsBoolean();
                        break;
                    case "BuildingJob":
                        float jobTime = float.Parse(reader.GetAttribute("jobTime"));
                        List<Item> items = new List<Item>();

                        XmlReader itemReader = reader.ReadSubtree();
                        while (itemReader.Read())
                        {
                            if (itemReader.Name == "Item")
                            {
                                Item item = new Item(
                                    itemReader.GetAttribute("objectType"),
                                    int.Parse(itemReader.GetAttribute("amount")),
                                    0
                                );
                                items.Add(item);
                            }
                        }

                        Job job = new Job(
                            null,
                            ObjectType,
                            FurnitureActions.JobCompleteFurnitureBuilding,
                            jobTime,
                            items.ToArray()
                        );

                        World.Instance.SetFurnitureJobPrototype(job, this);
                        break;
                    case "OnTick":
                        string functionName = reader.GetAttribute("functionName");
                        RegisterTickAction(functionName);
                        break;
                    case "JobSpotOffset":
                        jobSpotOffset = new Vector2(
                            int.Parse(reader.GetAttribute("X")),
                            int.Parse(reader.GetAttribute("Y"))
                        );
                        break;
                    case "JobSpawnSpotOffset":
                        jobSpawnSpotOffset = new Vector2(
                                int.Parse(reader.GetAttribute("X")),
                                int.Parse(reader.GetAttribute("Y"))
                            );
                        break;
                    case "OnIsEnterable":
                        IsEnterableTickAction = reader.GetAttribute("functionName");
                        break;
                    case "Params":
                        ReadXmlParams(reader);
                        break;

                }
            }

        }

        public void ReadXml(XmlReader reader)
        {
            // load info
            // movementCost = float.Parse(reader.GetAttribute("movementCost"));
            ReadXmlParams(reader);
        }

        public void ReadXmlParams(XmlReader reader)
        {
            // load info
            // movementCost = float.Parse(reader.GetAttribute("movementCost"));
            if (reader.ReadToDescendant("Param"))
            {
                do
                {
                    string key = reader.GetAttribute("name");
                    float value = float.Parse(reader.GetAttribute("value"));
                    furnParameters[key] = value;
                } while (reader.ReadToNextSibling("Param"));
            }
        }

        public float GetParameteter(string key, float defaultValue = 0f)
        {
            if (!furnParameters.ContainsKey(key))
            {
                return defaultValue;
            }

            return furnParameters[key];
        }

        public Enterability IsEnterable()
        {
            if (IsEnterableTickAction == null || IsEnterableTickAction.Length == 0)
            {
                return Enterability.Yes;
            }

            DynValue returnValue = FurnitureActions.CallFunction(IsEnterableTickAction, this);
            return (Enterability) returnValue.Number;
        }

        public Tile GetJobSpotTile()
        {
            return World.Instance.GetTileByPosition(Tile.X + (int)jobSpotOffset.x, Tile.Y + (int)jobSpotOffset.y);
        }

        public Tile GetSpawnSpotTile()
        {
            return World.Instance.GetTileByPosition(Tile.X + (int)jobSpawnSpotOffset.x, Tile.Y + (int)jobSpawnSpotOffset.y);
        }

        public int JobCount()
        {
            return jobs.Count;
        }

        public void AddJob(Job job)
        {
            job.furniture = this;
            jobs.Add(job);
            job.RegisterJobStoppedCallback(OnJobStopped);
            World.Instance.JobQueue.Enqueue(job);
        }

        private void RemoveJob(Job job)
        {
            job.UnregisterJobStoppedCallback(OnJobStopped);
            jobs.Remove(job);
            job.furniture = null;
        }

        private void ClearJobs()
        {
            List<Job> cachedJob = jobs;
            foreach (Job item in cachedJob)
            {
                RemoveJob(item);
            }
        }

        public void CancelJobs()
        {
            List<Job> cachedJob = jobs;
            foreach (Job item in cachedJob)
            {
                item.CancelJob();
            }
        }

        public void SetParameteter(string key, float value)
        {
            furnParameters[key] = value;
        }

        public void RegisterTickAction(string luaFunctionName)
        {
            tickActions.Add(luaFunctionName);
        }

        public void UnregisterTickAction(string luaFunctionName)
        {
            tickActions.Remove(luaFunctionName);
        }

        public void RegisterOnChange(Action<Furniture> callback)
        {
            onChanged += callback;
        }

        public void UnregisterOnChange(Action<Furniture> callback)
        {
            onChanged -= callback;
        }

        public void RegisterOnRemoved(Action<Furniture> callback)
        {
            onRemoved += callback;
        }

        public void UnregisterOnRemoved(Action<Furniture> callback)
        {
            onRemoved -= callback;
        }

        public bool IsValidPosition(Tile tile)
        {
            return funcPositionValidation(tile);
        }

        public bool DEFAULT___IsValidPosition(Tile tile)
        {
            for (int xOff = tile.X; xOff < (tile.X + Width); xOff++)
            {
                for (int yOff = tile.Y; yOff < (tile.Y + Height); yOff++)
                {
                    Tile tile2 = World.Instance.GetTileByPosition(xOff, yOff);
                    // Check if tile is floor
                    if (tile2.Type != TileType.Floor)
                    {
                        return false;
                    }

                    // Check tile is empty
                    if (tile2.Furniture != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsStockpile()
        {
            return ObjectType == "Stockpile";
        }

        public void Deconstruct()
        {
            Debug.Log("Deconstruct");

            Tile.UnplaceFurniture();

            if (onRemoved != null)
            {
                onRemoved(this);
            }

            if (RoomEnclosure)
            {
                Room.RoomFloodFill(Tile);
            }

            World.Instance.InvalidateTileGraph();
        }

        private void OnJobStopped(Job job)
        {
            RemoveJob(job);
        }
    }
}
