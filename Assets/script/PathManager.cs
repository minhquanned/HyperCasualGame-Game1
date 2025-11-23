using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý đường đi của quái vật (waypoints) - 3D
/// </summary>
public class PathManager : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    
    /// <summary>
    /// Lấy danh sách waypoints (position trong world space)
    /// </summary>
    public List<Vector3> GetWaypoints()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                positions.Add(waypoint.position);
            }
        }
        return positions;
    }
    
    /// <summary>
    /// Lấy điểm xuất phát (waypoint đầu tiên)
    /// </summary>
    public Vector3 GetStartPosition()
    {
        if (waypoints.Count > 0 && waypoints[0] != null)
        {
            return waypoints[0].position;
        }
        return transform.position;
    }
    
    /// <summary>
    /// Lấy điểm đích (waypoint cuối)
    /// </summary>
    public Vector3 GetEndPosition()
    {
        if (waypoints.Count > 0 && waypoints[waypoints.Count - 1] != null)
        {
            return waypoints[waypoints.Count - 1].position;
        }
        return transform.position;
    }
    
    private void OnDrawGizmos()
    {
        if (waypoints.Count < 2) return;
        
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
        
        // Vẽ điểm xuất phát và đích
        if (waypoints[0] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(waypoints[0].position, 0.3f);
        }
        
        if (waypoints.Count > 0 && waypoints[waypoints.Count - 1] != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(waypoints[waypoints.Count - 1].position, 0.3f);
        }
    }
}

