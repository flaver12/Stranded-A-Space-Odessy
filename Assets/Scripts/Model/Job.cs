using System.Collections;
using System.Collections.Generic;
using org.flaver.model;
using UnityEngine;
using System;

namespace org.flaver.model
{
    public class Job
    {
        public Tile Tile {
            get {
                return tile;
            }

            set {
                tile = value;
            }
        }
        public string JobObjectType { get; private set; }
        public Dictionary<string, Item> itemRequirements;
        public float jobTime { get; private set;}
        public bool accpetsAnyItems = false;
        public bool canTakeFromStockpile = true;
        public Furniture furniturePrototype;
        public Furniture furniture; // Pice of furniture, that owns this job

        private Tile tile;
        private Action<Job> jobCompleted;
        private Action<Job> jobStopped;
        private Action<Job> jobWorked;
        private bool jobIsRepeating;
        private float jobTimeRequired;

        public Job(Tile tile, string jobObjectType, Action<Job> jobCompleted, float jobTime, Item[] itemRequirements, bool jobIsRepeating = false)
        {
            this.tile = tile;
            JobObjectType = jobObjectType;
            this.jobCompleted += jobCompleted;
            this.jobTimeRequired = this.jobTime = jobTime;
            this.itemRequirements = new Dictionary<string, Item>();
            this.jobIsRepeating = jobIsRepeating;

            if (itemRequirements != null)
            {
                foreach (Item item in itemRequirements)
                {
                    this.itemRequirements[item.objectType] = item.Clone();
                }
            }
        }

        protected Job(Job toClone)
        {
            this.tile = toClone.tile;
            JobObjectType = toClone.JobObjectType;
            this.jobCompleted += toClone.jobCompleted;
            this.jobTime = toClone.jobTime;
            this.itemRequirements = new Dictionary<string, Item>();

            if (itemRequirements != null)
            {
                foreach (Item item in toClone.itemRequirements.Values)
                {
                    this.itemRequirements[item.objectType] = item.Clone();
                }
            }
        }

        public void TickDoWork(float workTime)
        {
            jobTime -= workTime;
            
            // Check to make sure we have everything we need
            if (!HasAllMaterials())
            {
                // Debug.LogError("Tried to do work on a job, that we dont have the mats for");
                if (jobWorked != null)
                {
                    jobWorked(this);
                }
                return;
            }

            if (jobWorked != null)
            {
                jobWorked(this);
            }

            if (jobTime <= 0)
            {
                if (jobCompleted != null)
                {
                    jobCompleted(this);
                }

                if (!jobIsRepeating)
                {
                    if (jobStopped != null)
                    {
                        jobStopped(this);
                    }
                }
                else
                {
                    // Reset job
                    jobTime += jobTimeRequired;
                }
            }
        }

        public Item GetFirstDesiredItem()
        {
            foreach (Item item in itemRequirements.Values)
            {
                if (item.maxStackSize > item.StackSize)
                {
                    return item;
                }
            }

            return null;
        }

        public bool HasAllMaterials()
        {
            foreach (Item item in itemRequirements.Values)
            {
                if (item.maxStackSize > item.StackSize)
                {
                    return false;
                }
            }

            return true;
        }

        public int DesiresItemType(Item item)
        {
            if (accpetsAnyItems)
            {
                return item.maxStackSize;
            }

            if (!itemRequirements.ContainsKey(item.objectType))
            {
                return 0;
            }

            if (itemRequirements[item.objectType].StackSize >= itemRequirements[item.objectType].maxStackSize)
            {
                return 0; // We have all we need
            }

            return itemRequirements[item.objectType].maxStackSize - itemRequirements[item.objectType].StackSize; // We have wat we want but still need more
        }

        public void CancelJob()
        {
            if (jobStopped != null)
            {
                jobStopped(this);
            }

            World.Instance.JobQueue.Remove(this);
        }

        public virtual Job Clone()
        {
            return new Job(this);
        }

        public void RegisterJobWorkedCallback(Action<Job> jobWorked)
        {
            this.jobWorked += jobWorked;
        }

        public void UnregisterJobWorkedCallback(Action<Job> jobWorked)
        {
            this.jobWorked -= jobWorked;
        }

        public void RegisterJobCompletedCallback(Action<Job> jobCompleted)
        {
            this.jobCompleted += jobCompleted;
        }

        public void UnregisterJobCompletedCallback(Action<Job> jobCompleted)
        {
            this.jobCompleted -= jobCompleted;
        }

        public void RegisterJobStoppedCallback(Action<Job> jobStopped)
        {
            this.jobStopped += jobStopped;
        }

        public void UnregisterJobStoppedCallback(Action<Job> jobStopped)
        {
            this.jobStopped -= jobStopped;
        }
    }
}
