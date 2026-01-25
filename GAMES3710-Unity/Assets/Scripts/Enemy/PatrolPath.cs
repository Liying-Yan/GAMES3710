using UnityEngine;
using System.Collections.Generic;

public class PatrolPath : MonoBehaviour
{
    [Tooltip("Available path segments to choose from when reaching the end")]
    public List<PatrolPath> nextPaths;

    private Vector3[] _waypoints;

    public Vector3[] Waypoints
    {
        get
        {
            if (_waypoints == null || _waypoints.Length != transform.childCount)
            {
                CacheWaypoints();
            }
            return _waypoints;
        }
    }

    public int WaypointCount => Waypoints.Length;

    private void CacheWaypoints()
    {
        _waypoints = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            _waypoints[i] = transform.GetChild(i).position;
        }
    }

    public Vector3 GetWaypoint(int index)
    {
        return Waypoints[Mathf.Clamp(index, 0, WaypointCount - 1)];
    }

    public PatrolPath GetRandomNextPath()
    {
        if (nextPaths == null || nextPaths.Count == 0)
            return this;
        return nextPaths[Random.Range(0, nextPaths.Count)];
    }

    private void OnDrawGizmos()
    {
        if (transform.childCount < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 current = transform.GetChild(i).position;
            Gizmos.DrawSphere(current, 0.3f);

            if (i < transform.childCount - 1)
            {
                Vector3 next = transform.GetChild(i + 1).position;
                Gizmos.DrawLine(current, next);
            }
        }

        Gizmos.color = Color.yellow;
        if (nextPaths != null)
        {
            Vector3 lastPoint = transform.GetChild(transform.childCount - 1).position;
            foreach (var path in nextPaths)
            {
                if (path != null && path.transform.childCount > 0)
                {
                    Vector3 nextStart = path.transform.GetChild(0).position;
                    Gizmos.DrawLine(lastPoint, nextStart);
                }
            }
        }
    }
}
