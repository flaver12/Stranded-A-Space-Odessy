using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace org.flaver.model
{
    public class JobQueue
    {
        public int Count {
            get {
                return jobQueue.Count;
            }
        }

        private Queue<Job> jobQueue;
        private Action<Job> jobCreated;

        public JobQueue()
        {
            jobQueue = new Queue<Job>();
        }

        public void Enqueue(Job job)
        {
            if (job.jobTime < 0) {
                // Job time is not positive
                // Complete job instead
                job.TickDoWork(0);
                return;
            }

            jobQueue.Enqueue(job);

            if (jobCreated != null)
            {
                jobCreated(job);
            }
        }

        public Job Dequeue()
        {
            if (jobQueue.Count == 0) {
                return null;
            }

            return jobQueue.Dequeue();
        }

        public void Remove(Job job)
        {
            // TODO meak its better
            List<Job> jobs = new List<Job>(jobQueue);
            if (!jobs.Contains(job))
            {
                // Debug.LogError("Try to remove job that not exists");
                return;
            }

            jobs.Remove(job);
            jobQueue = new Queue<Job>(jobs);
        }

        public void RegisterOnJobCreation(Action<Job> callback)
        {
            jobCreated += callback;
        }

        public void UnregisterOnJobCreation(Action<Job> callback)
        {
            jobCreated -= callback;
        }
    }
}