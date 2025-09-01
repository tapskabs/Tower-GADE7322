using System.Collections.Generic;
using UnityEngine;

public class PathGizmos : MonoBehaviour
{
    public List<Transform> waypoints;
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        Gizmos.DrawWireSphere(waypoints[0].position, 0.5f);
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}