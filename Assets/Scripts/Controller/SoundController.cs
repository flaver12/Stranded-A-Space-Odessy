using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using org.flaver.model;
using UnityEngine;

namespace flaver.org.controller {
    public class SoundController : MonoBehaviour
    {
        private float soundCooldown = 0f;

        private void Start()
        {
            // Register the callbacks for sound effects
            WorldController.Instance.World.RegisterFurnitureCreated(OnFurnitureCreated);
            WorldController.Instance.World.RegisterTileChanged(OnTileTypeChanged);
        }

        private void Update()
        {
            soundCooldown -= Time.deltaTime;
        }

        public void OnFurnitureCreated(Furniture furniture)
        {
            Debug.Log("OnFurnitureCreated: Play audio");
            // Build stuff

            if (soundCooldown > 0)
            {
                return;
            }

            AudioClip audioClip = Resources.Load<AudioClip>($"Sounds/{furniture.ObjectType}_OnCreated");

            if (audioClip == null)
            {
                // We have  a missing audio clip for oncreated
                // We just play the default sound
                // Which is Wall_Created
                audioClip = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
            }

            AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
            soundCooldown = 0.1f;
        }

        private void OnTileTypeChanged(Tile tile)
        {
            Debug.Log("OnTileTypeChanged: Play audio");
            // Build stuff

            if (soundCooldown > 0)
            {
                return;
            }
            AudioClip audioClip = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
            AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
            soundCooldown = 0.1f;
        }
    }
}
