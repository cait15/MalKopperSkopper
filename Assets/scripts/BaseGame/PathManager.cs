using UnityEngine;
using System.Collections.Generic;

public class PathNode : MonoBehaviour
{
    [Header("Connections")]
    // Manually assign the NEXT node(s) in the path here in the Inspector.
    // This is how you define branches (e.g., Node 5 connects to Node 7 AND Node 9).
    public List<PathNode> neighborNodes; 

    [Header("A* Data (Runtime Only)")]
    // F = G + H. The total estimated cost of the path through this node.
    [HideInInspector] public float F_Cost; 
    // G = Cost from the start node to this node.
    [HideInInspector] public float G_Cost; 
    // H = Heuristic (estimated cost from this node to the end node/target).
    [HideInInspector] public float H_Cost; 
    
    // Used to reconstruct the path after the destination is reached.
    [HideInInspector] public PathNode cameFromNode; 

    // Helper to get the world position
    public Vector3 WorldPosition => transform.position;

    // Optional: Draw lines between connected nodes in the Editor
    void OnDrawGizmos()
    {
        if (neighborNodes == null) return;
        
        Gizmos.color = Color.yellow;
        foreach (PathNode neighbor in neighborNodes)
        {
            if (neighbor != null)
            {
                // Draw an arrow or line to show the connection direction
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}