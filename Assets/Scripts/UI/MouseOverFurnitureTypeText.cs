using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using org.flaver.model;
using TMPro;
using UnityEngine;

namespace org.flaver.ui
{
    public class MouseOverFurnitureTypeText : MouseOver
    {
        private void Update()
        {
            Tile tile = mouseController.GetMouseOverTile();

            string s = "Furniture Type: NULL"; 

            if (tile.Furniture != null)
            {
                s = $"Furniture Type: {tile.Furniture.Name}";
            }

            text.text = s;
        }
    }
}

