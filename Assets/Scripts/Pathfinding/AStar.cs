using System.Collections;
using System.Collections.Generic;
using org.flaver.model;
using UnityEngine;
using Priority_Queue;
using System.Linq;

namespace org.flaver.pathfinding {
    public class AStar
    {
        private Queue<Tile> path;

        public AStar(World world, Tile tileStart, Tile tileEnd)
        {
            // Check if tile graph is still valid
            if (world.TileGraph == null)
            {
                world.TileGraph = new TileGraph(world);
            }

            Dictionary<Tile, Node<Tile>> nodes = world.TileGraph.Nodes;

            if(!nodes.ContainsKey(tileStart))
            {
                Debug.LogError("AStar: Starting tile is not part of the nodes");
                return;
            }

            if(!nodes.ContainsKey(tileEnd))
            {
                Debug.LogError("AStar: Ending tile is not part of the nodes");
                return;
            }

            Node<Tile> start = nodes[tileStart];
            Node<Tile> end = nodes[tileEnd];

            List<Node<Tile>> closedSet = new List<Node<Tile>>();
            SimplePriorityQueue<Node<Tile>> openSet = new SimplePriorityQueue<Node<Tile>>();
            openSet.Enqueue(start, 0);

            Dictionary<Node<Tile>, Node<Tile>> cameFrom = new Dictionary<Node<Tile>, Node<Tile>>();

            // Init score
            // We use infinity so that a node we don't now has a to high cost
            Dictionary<Node<Tile>, float> gScore = new Dictionary<Node<Tile>, float>();
            foreach (Node<Tile> item in nodes.Values)
            {
                gScore[item] = Mathf.Infinity;
            }
            gScore[ start ] = 0; // Set the cost for the starting node
            
            Dictionary<Node<Tile>, float> fScore = new Dictionary<Node<Tile>, float>();
            foreach (Node<Tile> item in nodes.Values)
            {
                fScore[item] = Mathf.Infinity;
            }
            fScore[ start ] = HeuristicCostEstimate(start, end); // Set the cost for the starting node

            while (openSet.Count > 0)
            {
                Node<Tile> current = openSet.Dequeue();
                if (current == end)
                {
                    // We found a way
                    ReconstructPath(cameFrom, current);
                    return;
                }

                closedSet.Add(current);

                foreach (Edge<Tile> edge in current.edges)
                {
                    Node<Tile> neighbor = edge.node;

                    if (closedSet.Contains(neighbor))
                    {
                        continue; // ignore already completed neighbour
                    }

                    float movementCostToNeighbor = neighbor.data.MovementCost * DistanceBetween(current, neighbor);
                    float tentativeGScore = gScore[current] + movementCostToNeighbor;

                    if (openSet.Contains(neighbor) && tentativeGScore >= gScore[neighbor])
                    {
                        continue;
                    }
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, end);
                    
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]); // with the least fScore is at the top
                    }
                    else
                    {
                        // If we have to neighbour we update its fscore
                        openSet.UpdatePriority(neighbor, fScore[neighbor]);
                    }
                }
            }

            // If we reach here that means that we went trough the hole
            // Openset without reaching a point where current == goal
            // This happens where there is no path from start to goal (wall, goal)
            return;
        }

        public Tile Dequeue()
        {
            return path.Dequeue();
        }

        public int Lenght()
        {
            if (path == null)
            {
                return 0;
            }

            return path.Count;
        }

        private void ReconstructPath(Dictionary<Node<Tile>, Node<Tile>> cameFrom, Node<Tile> current)
        {
            // So at this point current is the goal
            // So waht we do is we walk backwards trough the cameFrom map
            // unit we reached the "end" of the map, which will be our starting node!
            Queue<Tile> totalPath = new Queue<Tile>();
            totalPath.Enqueue(current.data);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Enqueue(current.data);
            }
            
            path = new Queue<Tile>(totalPath.Reverse());
        }

        private float DistanceBetween(Node<Tile> a, Node<Tile> b)
        {
            if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
            {
                return 1f;
            }

            // 1.41421356237 root from 2
            if (Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1 ) {
                return 1.41421356237f;
            }

            // Otherwise
            return Mathf.Sqrt(
                Mathf.Pow(a.data.X - b.data.X, 2)
                +
                Mathf.Pow(a.data.Y - b.data.Y, 2)
            );
        }

        private float HeuristicCostEstimate(Node<Tile> a, Node<Tile> b)
        {
            return Mathf.Sqrt(
                Mathf.Pow(a.data.X - b.data.X, 2)
                +
                Mathf.Pow(a.data.Y - b.data.Y, 2)
            );
        }
    }
}
