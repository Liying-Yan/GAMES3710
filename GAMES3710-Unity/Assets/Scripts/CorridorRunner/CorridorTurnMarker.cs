using UnityEngine;

/// <summary>
/// Place at corridor turn points with a BoxCollider (isTrigger = true).
/// Detects player entry and triggers a turn on the CorridorRunnerController.
/// </summary>
public class CorridorTurnMarker : MonoBehaviour
{
    public enum TurnType
    {
        SingleTurn,
        TJunction
    }

    [Header("Turn Configuration")]
    [Tooltip("SingleTurn: always turn in TurnDirection. TJunction: direction based on player position.")]
    public TurnType Type = TurnType.SingleTurn;

    [Tooltip("Yaw delta for SingleTurn (+90 = right, -90 = left). Ignored for TJunction.")]
    public float TurnDirection = 90f;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        var runner = other.GetComponent<CorridorRunnerController>();
        if (runner == null) return;
        if (runner.IsTurning) return;

        _triggered = true;

        float yaw = ResolveTurnDirection(other.transform.position);
        runner.ExecuteTurn(yaw);
    }

    private float ResolveTurnDirection(Vector3 playerPosition)
    {
        if (Type == TurnType.SingleTurn)
            return TurnDirection;

        // T-Junction: project player position onto the marker's local X axis.
        // Negative = player is on the left side → turn left (-90)
        // Positive = player is on the right side → turn right (+90)
        Vector3 offset = playerPosition - transform.position;
        float lateral = Vector3.Dot(offset, transform.right);

        return lateral >= 0f ? 90f : -90f;
    }

    private void OnDrawGizmos()
    {
        // Visualize the turn marker in the editor
        Gizmos.color = Type == TurnType.SingleTurn
            ? new Color(0f, 1f, 0f, 0.4f)
            : new Color(1f, 1f, 0f, 0.4f);

        Gizmos.matrix = transform.localToWorldMatrix;

        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.center, col.size);
        }
        else
        {
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        Gizmos.matrix = Matrix4x4.identity;

        // Draw turn direction arrow
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Type == TurnType.SingleTurn)
        {
            Vector3 dir = Quaternion.Euler(0f, TurnDirection, 0f) * transform.forward;
            Gizmos.DrawRay(origin, dir * 2f);
        }
        else
        {
            // Show both possible directions for T-junction
            Gizmos.DrawRay(origin, -transform.right * 2f);
            Gizmos.DrawRay(origin, transform.right * 2f);
        }
    }
}
