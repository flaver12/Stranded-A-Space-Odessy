using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using org.flaver.model;
using TMPro;
using UnityEngine;

namespace org.flaver.ui
{
    public class MouseOverRoomIndexText : MouseOver
    {
        private void Update()
        {
            Tile tile = mouseController.GetMouseOverTile();

            string roomId = "N/A";

            if (tile.room != null)
            {
                roomId = tile.room.Id.ToString();
            }

            text.text = $"Room Index: {roomId}";
        }
    }
}

