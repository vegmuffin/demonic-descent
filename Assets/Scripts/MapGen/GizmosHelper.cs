using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosHelper : MonoBehaviour
{
    private Vector3 gizmoVector = new Vector3(15f, 9f, 1f);
    private Vector3 gizmoCenter = new Vector3(0.5f, 0.5f, 0);
    
    // Helps define the edges of a room as well as the center when making room presets.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gizmoCenter, gizmoVector);
        Gizmos.DrawWireCube(gizmoCenter, Vector3.one);
    }
}
