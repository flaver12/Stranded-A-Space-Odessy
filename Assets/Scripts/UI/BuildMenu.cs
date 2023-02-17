using System.Collections;
using System.Collections.Generic;
using org.flaver.model;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using org.flaver.controller;

namespace org.flaver.ui
{
    public class BuildMenu : MonoBehaviour
    {
        public GameObject buildFurniturePrefab;

        private void Start()
        {
            Debug.Log("BuildMenu: Setup build menu");
            BuildModeController  buildModeController = GameObject.FindObjectOfType<BuildModeController>();

            // Add a button for each type of furniture
            foreach (string furnitureKey in World.Instance.FurniturePrototypes.Keys)
            {
                GameObject go = Instantiate(buildFurniturePrefab);
                go.transform.SetParent(this.transform);
                go.name = "Button - Build " + furnitureKey;
                go.GetComponentInChildren<TMP_Text>().text = "Build " + furnitureKey;
                
                Button button = go.GetComponent<Button>(); 
                button.onClick.AddListener(
                    delegate
                    {
                        buildModeController.SetModeBuildFurniture(furnitureKey);
                    }
                );
            }
        }
    }
}

