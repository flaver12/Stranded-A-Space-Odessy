using System.Collections;
using System.Collections.Generic;
using org.flaver.model;
using UnityEngine;

namespace org.flaver.pathfinding {
    public class Node<T>
    {
        public T data;
        public Edge<T>[] edges; // Nodes out from this node
    }
}
