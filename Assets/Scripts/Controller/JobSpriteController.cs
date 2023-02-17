using System.Collections;
using System.Collections.Generic;
using org.flaver.model;
using UnityEngine;

namespace org.flaver.controller
{
    public class JobSpriteController : MonoBehaviour
    {
        private FurnitureSpriteController furnitureSpriteController;
        private Dictionary<Job, GameObject> mappedJobToGameObject;

        private void Start()
        {
            mappedJobToGameObject = new Dictionary<Job, GameObject>();
            furnitureSpriteController = GameObject.FindObjectOfType<FurnitureSpriteController>();
            WorldController.Instance.World.JobQueue.RegisterOnJobCreation(OnJobCreated);
        }

        private void OnJobCreated(Job job)
        {
            if (job.JobObjectType == null)
            {
                // This job has no sprite
                // no need to render
                return;
            }

            // FIXME
            if (mappedJobToGameObject.ContainsKey(job))
            {
                Debug.LogError("OnJobCreated for a job game object that all ready exists!");
                return;
            }

            // Create a visual game object lined to the data
            GameObject jobGameObject = new GameObject();

            // Add the go to the map
            mappedJobToGameObject.Add(job, jobGameObject);
            
            // Setup the tile
            jobGameObject.name = "Job_" + job.JobObjectType;
            jobGameObject.transform.position = new Vector3(job.Tile.X + ((job.furniturePrototype.Width - 1) / 2f), job.Tile.Y + ((job.furniturePrototype.Height - 1) / 2f), 0f);
            jobGameObject.transform.SetParent(transform, true); // true = stay in world pos

            
            // FIXME maybe later make it rotate better
            if (job.JobObjectType == "Door")
            {
                // By default the door graphic is for east west
                // Check for a wall north south
                // Rotate then rotate 90degres
                Tile nortTile = World.Instance.GetTileByPosition(job.Tile.X, job.Tile.Y + 1);
                Tile southTile = World.Instance.GetTileByPosition(job.Tile.X, job.Tile.Y - 1);
                
                if (nortTile != null && southTile != null && nortTile.Furniture != null && southTile.Furniture != null && nortTile.Furniture.ObjectType == "Wall" && southTile.Furniture.ObjectType == "Wall")
                {
                    jobGameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
            }


            jobGameObject.AddComponent<SpriteRenderer>().sprite = furnitureSpriteController.GetSpriteForFurniture(job.JobObjectType);
            jobGameObject.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Jobs");
            jobGameObject.GetComponent<SpriteRenderer>().color = new Color(0.5f, 1f, 0.5f, 0.25f);

            job.RegisterJobCompletedCallback(OnJobEnded);
            job.RegisterJobStoppedCallback(OnJobEnded);
        }

        private void OnJobEnded(Job job)
        {
            // Called when job is done or is cancelled
            // Get the go 
            GameObject jobGameobject = mappedJobToGameObject[job];

            // Unhook us from the callbacks
            job.UnregisterJobStoppedCallback(OnJobEnded);
            job.UnregisterJobCompletedCallback(OnJobEnded);
            
            // Cleanup
            Destroy(jobGameobject);
            mappedJobToGameObject.Remove(job);
        }
    }
}
