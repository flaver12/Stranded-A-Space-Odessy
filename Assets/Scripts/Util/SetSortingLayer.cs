using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace org.flaver.util
{
    public class SetSortingLayer : MonoBehaviour
    {
        public string sortingLayerName = "default";

        private void Start()
        {
            GetComponent<Renderer>().sortingLayerName = sortingLayerName;
        }
    }
}

