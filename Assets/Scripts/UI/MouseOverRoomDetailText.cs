using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using org.flaver.model;
using TMPro;
using UnityEngine;

namespace org.flaver.ui
{
    public class MouseOverRoomDetailText : MouseOver
    {
        private void Update()
        {
            Tile tile = mouseController.GetMouseOverTile();
            
            if (tile == null || tile.room == null)
            {
                text.text = "";
                return;
            }

            string s = "";
            foreach (string gas in tile.room.GetGasNames())
            {
                s += $"{gas}: {tile.room.GetGasAmount(gas)} ({tile.room.GetGasPercentage(gas) * 100}%)";
            }

            text.text = s;
        }
    }
}

