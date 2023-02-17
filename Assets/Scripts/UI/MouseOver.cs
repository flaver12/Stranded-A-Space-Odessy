using System.Collections;
using System.Collections.Generic;
using org.flaver.controller;
using TMPro;
using UnityEngine;

namespace org.flaver.ui
{
    public class MouseOver : MonoBehaviour
    {
        protected TMP_Text text;
        protected MouseController mouseController;

        private void Start()
        {
            text = GetComponent<TMP_Text>();
            mouseController = GameObject.FindObjectOfType<MouseController>();
        }
    }
}
