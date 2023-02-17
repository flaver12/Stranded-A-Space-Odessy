using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using org.flaver.model;
using TMPro;
using UnityEngine;

namespace org.flaver.ui
{
    public class MouseOverTileTypeText : MouseOver
    {
        private void Update()
        {
            Tile tile = mouseController.GetMouseOverTile();
            text.text = $"Tile Type: {tile.Type.ToString()}";
        }
    }
}

