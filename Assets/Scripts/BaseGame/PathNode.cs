using UnityEngine;
using System.Collections.Generic;

public class PathNode : MonoBehaviour
{
    [Header("Connections")]
    public List<PathNode> neighborNodes; 

    [Header("A* Data (Runtime Only)")]
    [HideInInspector] public float F_Cost; 
    [HideInInspector] public float G_Cost; 
    [HideInInspector] public float H_Cost; 
    [HideInInspector] public PathNode cameFromNode; 

    public Vector3 WorldPosition => transform.position;

    void OnDrawGizmos()
    {
        if (neighborNodes == null) return;
        
        Gizmos.color = Color.yellow;
        foreach (PathNode neighbor in neighborNodes)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}