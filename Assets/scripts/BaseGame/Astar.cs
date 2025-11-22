using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class AStarPathfinder
{
    // The main A* calculation method
    public static List<PathNode> FindPath(PathNode startNode, PathNode endNode)
    {
        if (startNode == null || endNode == null) return null;

        //  Setup Open and Closed Lists
        List<PathNode> openList = new List<PathNode> { startNode };
        List<PathNode> closedList = new List<PathNode>();

        //  Initialize Costs
        foreach (var node in GameObject.FindObjectsOfType<PathNode>())
        {
            node.G_Cost = float.MaxValue;
            node.cameFromNode = null;
        }

        startNode.G_Cost = 0;
        startNode.H_Cost = CalculateDistanceCost(startNode, endNode);
        startNode.F_Cost = startNode.H_Cost;

        //  Main A* Loop
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                // Reached destination, return the path
                return ReconstructPath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbor in currentNode.neighborNodes)
            {
                if (neighbor == null || closedList.Contains(neighbor)) continue;

                //  Calculate Tentative G Cost
                float tentativeGCost = currentNode.G_Cost + CalculateDistanceCost(currentNode, neighbor);
                
                if (tentativeGCost < neighbor.G_Cost)
                {
                    // Found a better path to the neighbor
                    neighbor.cameFromNode = currentNode;
                    neighbor.G_Cost = tentativeGCost;
                    neighbor.H_Cost = CalculateDistanceCost(neighbor, endNode);
                    neighbor.F_Cost = neighbor.G_Cost + neighbor.H_Cost;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        // Path not found
        return null;
    }

    // Heuristic: Straight line distance (Manhattan or Euclidean for a 2D grid)
    private static float CalculateDistanceCost(PathNode a, PathNode b)
    {
        return Vector3.Distance(a.WorldPosition, b.WorldPosition);
    }

    // Finds the node in the open list with the lowest F_Cost
    private static PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].F_Cost < lowestFCostNode.F_Cost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    // Reconstructs the path by following the 'cameFromNode' pointers back to the start
    private static List<PathNode> ReconstructPath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse(); // Path is currently end -> start, reverse it to be start -> end
        return path;
    }
}