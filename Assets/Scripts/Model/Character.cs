using System;
using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using org.flaver.pathfinding;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using org.flaver.defintion;

namespace org.flaver.model
{
    public class Character : IXmlSerializable
    {
        public float X {
            get {
                return Mathf.Lerp(currentTile.X, nextTile.X, movementPercentage);
            }
        }

        public float Y {
            get {
                return Mathf.Lerp(currentTile.Y, nextTile.Y, movementPercentage);
            }
        }

        public Tile Tile {
            get {
                return currentTile;
            }
        }

        public Tile DestionationTile {
            get {
                return destionationTile;
            }
        }

        public Item Item {
            get
            {
                return item;
            }

            set
            {
                item = value;
            }
        }

        private Tile currentTile;
        private Tile _destionationTile;
        private Tile destionationTile {
            get
            {
                return _destionationTile;
            }

            set {
                if (_destionationTile != value)
                {
                    _destionationTile = value;
                    aStar = null; // invalidate path finding
                }
            }
        }
        private Tile nextTile;
        private float movementPercentage; // 0 -> 1 as we move from current to destionation
        private float speed = 2f; // Tiles per second
        private Action<Character> changed;
        private Job job;
        private AStar aStar;
        private Item item;

        public Character()
        {

        }

        public Character(Tile tile)
        {
            currentTile = destionationTile = nextTile = tile;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

       public void WriteXml(XmlWriter writer)
        {
            // save info
            writer.WriteAttributeString("X", currentTile.X.ToString());
            writer.WriteAttributeString("Y", currentTile.Y.ToString());
        }

        public void ReadXml(XmlReader reader)
        {
            // load info
        }

        public void Tick(float deltaTime)
        {
            TickDoJob(deltaTime);
            TickDoMovement(deltaTime);

            if (changed != null)
            {
                changed(this);
            }
        }

        public void AbandonJob()
        {
            nextTile = destionationTile = currentTile;
            World.Instance.JobQueue.Enqueue(job);
            job = null;
        }

        private void TickDoMovement(float deltaTime)
        {
            if (currentTile == destionationTile)
            {
                aStar = null;
                return;
            }

            if (nextTile == null || nextTile == currentTile)
            {
                // Get the next tile from the pathfinding
                if (aStar == null || aStar.Lenght() == 0)
                {
                    // Generate a path to our destionation
                    aStar = new AStar(World.Instance, currentTile, destionationTile);
                    if (aStar.Lenght() == 0)
                    {
                        Debug.LogError("AStar return no path to destionation");
                        AbandonJob();
                        return;
                    }
                    // Get rid of the fisr tile, thats the one we standing on
                    aStar.Dequeue();
                }

                nextTile = aStar.Dequeue();
                if (nextTile == currentTile)
                {
                    Debug.LogError("TickDoMovement - nextTile is currTile?");
                }
            }

            if (nextTile.IsEnterable() == Enterability.Never)
            {
                Debug.LogError("FIXME: I want to walk tough a wall");
                nextTile = null;
                aStar = null; // Invalidate path
                return;
            } else if(nextTile.IsEnterable() == Enterability.Soon) {
                // We can not enter the tile now
                // But later / like a door
                // Do not walk right now
                return;
            }

            // Total distance from point a to point b
            float totalDistance = Mathf.Sqrt(Mathf.Pow(currentTile.X - nextTile.X, 2) + Mathf.Pow(currentTile.Y - nextTile.Y, 2));
            float distanceThisFrame = (speed / nextTile.MovementCost) * deltaTime; // Distance to travel this frame
            float percentageThisFrame = distanceThisFrame / totalDistance; // Calculate the percantage

            movementPercentage += percentageThisFrame;
            if (movementPercentage >= 1) {
                // We are here
                // TODO get the next node from the pathfinding system
                // If no path is given we have reach the end!
                currentTile = nextTile;
                movementPercentage = 0;
            }
        }

        private void TickDoJob(float deltaTime)
        {
            // Check for job
            if(job == null)
            {
                GetNewJob();
                if (job == null)
                {
                    destionationTile = currentTile;
                    return;
                }
            }

            // Check if we have all mats needed
            if (!job.HasAllMaterials())
            {
                if (item != null)
                {
                    if (job.DesiresItemType(item) > 0)
                    {
                        if (currentTile == job.Tile)
                        {
                            // We are at the jobs site, drop the item
                            World.Instance.itemManager.InstallItem(job, item);
                            job.TickDoWork(0); // call the cb to get notified

                            if (item.StackSize == 0)
                            {
                                item = null;
                            }
                            else
                            {
                                Debug.LogError("Character is still carrying item which shoudn't be.");
                                item = null;
                            }
                        }
                        else 
                        {
                            destionationTile = job.Tile;
                            return;
                        }
                    } 
                    else
                    {
                        if (!World.Instance.itemManager.InstallItem(currentTile, item))
                        {
                            Debug.LogError("Character tried to dump item to invalid tile");
                            // FIXME 
                            item = null;
                        }
                    }
                }
                else
                {
                    if (
                        currentTile.Item != null
                        &&
                        (job.canTakeFromStockpile || currentTile.Furniture == null || !currentTile.Furniture.IsStockpile())
                        &&
                        job.DesiresItemType(currentTile.Item) > 0)
                    {
                        // Pick up the stuff
                        World.Instance.itemManager.InstallItem(
                            this,
                            currentTile.Item,
                            job.DesiresItemType(currentTile.Item)
                        );
                    }
                    else
                    {
                        // At this point the job
                        // needs item but we have none
                        // We have to walk to a tile who has the goods
                        Item desired = job.GetFirstDesiredItem();
                        Item supplier = World.Instance.itemManager.GetClosestItemOfType(
                            desired.objectType,
                            currentTile,
                            desired.maxStackSize - desired.StackSize,
                            job.canTakeFromStockpile
                        );

                        if (supplier == null)
                        {
                            Debug.Log($"No tail contains of type {desired.objectType} to satisfy job requirements");
                            AbandonJob();
                            return;
                        }
                        destionationTile = supplier.tile;
                        return;
                    }
                }

                return; // We cant work like this
            }

            // We need to set here the destionation tile
            // because we might have hauled some mats
            destionationTile = job.Tile;
            if (currentTile == job.Tile)
            {
                // Correct tile for the job
                // Exec job to work
                job.TickDoWork(deltaTime);
            }
        }

        private void GetNewJob()
        {
            job = World.Instance.JobQueue.Dequeue();
            if (job == null)
            {
                return;
            }

            destionationTile = job.Tile;
            job.RegisterJobStoppedCallback(OnJobStopped);

            aStar = new AStar(World.Instance, currentTile, destionationTile);
            if (aStar.Lenght() == 0)
            {
                Debug.LogError("AStar return no path to target job");
                AbandonJob();
                destionationTile = currentTile;
            }
        }

        public void SetDestination(Tile tile)
        {
            if (!currentTile.IsNeighbour(tile, true))
            {
                Debug.Log("Char SetDestination this is not my neighbour");
            }

            destionationTile = tile;
        }

        public void RegisterOnCharacterChanged(Action<Character> changed)
        {
            this.changed += changed;
        }

        public void UnregisterOnCharacterChanged(Action<Character> changed)
        {
            this.changed -= changed;
        }

        private void OnJobStopped(Job job)
        {
            job.UnregisterJobStoppedCallback(OnJobStopped);

            // Job done / ended
            if (job != this.job)
            {
                Debug.LogError("HELP! I got informed of the wrong job");
                return;
            }

            this.job = null;
        }
    }
}