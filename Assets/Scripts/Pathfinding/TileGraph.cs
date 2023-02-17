using System.Collections;
using System.Collections.Generic;
using org.flaver.defintion;
using org.flaver.model;
using UnityEngine;

namespace org.flaver.pathfinding {
    // Generates a Graph from our world
    // Each tile is a node. Each walkable neighbour 
    // from a tile is linked with an edge connection
    public class TileGraph
    {
        public Dictionary<Tile, Node<Tile>> Nodes { private set; get; }

        public TileGraph(World world)
        {
            Debug.Log("TileGraph");
            // Init node list
            Nodes = new Dictionary<Tile, Node<Tile>>();
            // Loop trough all tiles and build a node for each node
            for (int x = 0; x < world.Width; x++)
            {
                for (int y = 0; y < world.Height; y++)
                {
                    Tile tile = world.GetTileByPosition(x,y);
                    //if (tile.MovementCost > 0) // 0 means you can not walk on them
                    //{
                        Node<Tile> node = new Node<Tile>();
                        node.data = tile;
                        Nodes.Add(tile, node);
                    //}
                }
            }

            Debug.Log($"TileGraph: Created {Nodes.Count} nodes.");
            
            int edgeCount = 0;

            // Loop trough the nodes to create the edges
            foreach (Tile tile in Nodes.Keys)
            {
                Node<Tile> node = Nodes[tile];
                List<Edge<Tile>> edges = new List<Edge<Tile>>();

                // Get the neighbours from current tile
                Tile[] neighbours = tile.GetNeighbours(true);
                // When you can walk on it Create a edge
                for (int i = 0; i < neighbours.Length; i++)
                {
                    // You can walk on it and its not null
                    if (neighbours[i] != null && neighbours[i].MovementCost > 0)
                    {
                        // Check for clipping diagonal path
                        if (IsClippingCorner(tile, neighbours[i]))
                        {
                            continue; // Skip
                        }

                        Tile neighbour = neighbours[i];
                        Edge<Tile> edge = new Edge<Tile>();
                        edge.cost = neighbour.MovementCost;
                        edge.node = Nodes[neighbour];
                        edges.Add(edge);
                        edgeCount ++;
                    }
                }

                // Map the edges
                node.edges = edges.ToArray();
            }

            Debug.Log($"TileGraph: Created {edgeCount} edges.");
        }

        private bool IsClippingCorner(Tile currentTile, Tile neighborTile)
        {
            // IF movement is currentTile to neighborTile (N-E)
            // Check that we not clipping (N && E == walkable)
            int differenceX = currentTile.X - neighborTile.X;
            int differenceY = currentTile.Y - neighborTile.Y;

            if(Mathf.Abs(differenceX) + Mathf.Abs(differenceY) == 2)
            {
                // We are diagonal
                if (World.Instance.GetTileByPosition(currentTile.X - differenceX, currentTile.Y).MovementCost == 0)
                {
                    // East or West is not walkable
                    // This would mean we clip
                    return true;
                }

                if (World.Instance.GetTileByPosition(currentTile.X , currentTile.Y - differenceY).MovementCost == 0)
                {
                    // North or South is not walkable
                    // This would mean we clip
                    return true;
                }
            }

            return false;
        }
    }
}
