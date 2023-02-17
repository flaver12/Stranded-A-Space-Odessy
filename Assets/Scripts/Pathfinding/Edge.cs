using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace org.flaver.pathfinding {
    public class Edge<T>
    {
        public Node<T> node;
        public float cost;
    }
}