using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Search,
    CheckHidingSpot
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

    [Header("Vision Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Catch Settings")]
    public float catchDistance = 1.5f;

    [Header("Search Settings")]
    public float searchDuration = 5f;
    public float searchRadius = 8f;

    [Header("Hiding Spot Check")]
    [Tooltip("Enable to let this enemy check hiding spots when it sees the player hide (requires spot-side toggle too)")]
    public bool canCheckHidingSpots = false;

    private NavMeshAgent _agent;
    private EnemyState _currentState = EnemyState.Patrol;

    private PatrolPath _currentPath;
    private int _currentWaypointIndex;

    private Transform _player;
    private Vector3 _lastKnownPlayerPosition;
    private float _searchTimer;

    private ICheckableHidingSpot _targetSpot;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = false;
    }

    private void Start()
    {
        _agent.enabled = true;

        if (!_agent.isOnNavMesh)
        {
            Debug.LogError($"EnemyAI: {name} is not on NavMesh. Check its position or re-bake the NavMesh.");
            enabled = false;
            return;
        }

        if (initialPath != null)
        {
            SetPath(initialPath);
        }
        SetState(EnemyState.Patrol);
    }

    private void Update()
    {
        if (!_agent.isOnNavMesh) return;

        CheckVision();

        switch (_currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Search:
                UpdateSearch();
                break;
            case EnemyState.CheckHidingSpot:
                UpdateCheckHidingSpot();
                break;
        }
    }

    private void CheckVision()
    {
        if (PlayerHideState.Instance != null && PlayerHideState.Instance.IsHiding)
        {
            if (_currentState == EnemyState.Chase)
            {
                var spot = PlayerHideState.Instance.CurrentSpot;
                if (canCheckHidingSpots && spot != null && spot.CanBeChecked)
                {
                    _targetSpot = spot;
                    SetState(EnemyState.CheckHidingSpot);
                }
                else
                {
                    SetState(EnemyState.Search);
                }
            }
            return;
        }

        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, viewDistance, playerMask);

        foreach (var target in targetsInRange)
        {
            Transform targetTransform = target.transform;
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < viewAngle / 2f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    _player = targetTransform;
                    _lastKnownPlayerPosition = _player.position;

                    if (_currentState != EnemyState.Chase)
                    {
                        SetState(EnemyState.Chase);
                    }
                    return;
                }
            }
        }

        if (_currentState == EnemyState.Chase)
        {
            SetState(EnemyState.Search);
        }
    }

    private bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 directionToPlayer = (_player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer > viewAngle / 2f) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distanceToPlayer > viewDistance) return false;

        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
            return false;

        return true;
    }

    private void SetState(EnemyState newState)
    {
        _currentState = newState;
        switch (newState)
        {
            case EnemyState.Patrol:
                _agent.speed = patrolSpeed;
                if (_currentPath != null && _currentPath.WaypointCount > 0 && _agent.isOnNavMesh)
                {
                    _agent.SetDestination(_currentPath.GetWaypoint(_currentWaypointIndex));
                }
                break;
            case EnemyState.Chase:
                _agent.speed = chaseSpeed;
                break;
            case EnemyState.Search:
                _agent.speed = searchSpeed;
                _searchTimer = searchDuration;
                _agent.SetDestination(_lastKnownPlayerPosition);
                break;
            case EnemyState.CheckHidingSpot:
                _agent.speed = chaseSpeed;
                if (_targetSpot != null && _targetSpot.CheckTarget != null)
                    _agent.SetDestination(_targetSpot.CheckTarget.position);
                break;
        }
    }

    private void UpdateChase()
    {
        if (_player != null && _agent.isOnNavMesh)
        {
            _lastKnownPlayerPosition = _player.position;
            _agent.SetDestination(_player.position);

            if (Vector3.Distance(transform.position, _player.position) <= catchDistance)
            {
                if (GameOverUI.Instance != null)
                    GameOverUI.Instance.Show();
            }
        }
    }

    private void UpdateSearch()
    {
        _searchTimer -= Time.deltaTime;

        if (_searchTimer <= 0f)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachThreshold)
        {
            SetRandomSearchDestination();
        }
    }

    private void UpdateCheckHidingSpot()
    {
        if (_targetSpot == null || _targetSpot.CheckTarget == null)
        {
            SetState(EnemyState.Search);
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachThreshold)
        {
            _targetSpot.OnEnemyCheck();
            _targetSpot = null;
        }
    }

    private void SetRandomSearchDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
        randomDirection += _lastKnownPlayerPosition;

        if (_agent.isOnNavMesh && NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    private void SetPath(PatrolPath path)
    {
        _currentPath = path;
        _currentWaypointIndex = 0;
        if (_currentPath.WaypointCount > 0 && _agent.isOnNavMesh)
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        if (_player != null && _currentState == EnemyState.Chase)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _player.position);
        }

        if (_currentState == EnemyState.Search)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_lastKnownPlayerPosition, searchRadius);
        }
    }
}
