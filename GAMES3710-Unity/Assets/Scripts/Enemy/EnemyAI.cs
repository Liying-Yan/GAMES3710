using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Search
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public PatrolPath initialPath;
    public float waypointReachThreshold = 0.5f;

    [Header("Movement Speed")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float searchSpeed = 3f;

    private NavMeshAgent _agent;
    private EnemyState _currentState = EnemyState.Patrol;

    private PatrolPath _currentPath;
    private int _currentWaypointIndex;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (initialPath != null)
        {
            SetPath(initialPath);
        }
        SetState(EnemyState.Patrol);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                // TODO: 追逐逻辑
                break;
            case EnemyState.Search:
                // TODO: 搜索逻辑
                break;
        }
    }

    private void SetState(EnemyState newState)
    {
        _currentState = newState;
        switch (newState)
        {
            case EnemyState.Patrol:
                _agent.speed = patrolSpeed;
                break;
            case EnemyState.Chase:
                _agent.speed = chaseSpeed;
                break;
            case EnemyState.Search:
                _agent.speed = searchSpeed;
                break;
        }
    }

    private void SetPath(PatrolPath path)
    {
        _currentPath = path;
        _currentWaypointIndex = 0;
        if (_currentPath.WaypointCount > 0)
        {
            _agent.SetDestination(_currentPath.GetWaypoint(0));
        }
    }

    private void UpdatePatrol()
    {
        if (_currentPath == null || _currentPath.WaypointCount == 0)
            return;

        if (!_agent.isOnNavMesh)
            return;

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachThreshold)
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= _currentPath.WaypointCount)
            {
                PatrolPath nextPath = _currentPath.GetRandomNextPath();
                SetPath(nextPath);
            }
            else
            {
                _agent.SetDestination(_currentPath.GetWaypoint(_currentWaypointIndex));
            }
        }
    }
}
