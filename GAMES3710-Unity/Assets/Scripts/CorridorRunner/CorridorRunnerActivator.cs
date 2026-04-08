using UnityEngine;

/// <summary>
/// Trigger zone that activates the corridor runner mode when the player enters.
/// Place at the corridor entrance with a BoxCollider (isTrigger = true).
/// </summary>
public class CorridorRunnerActivator : MonoBehaviour
{
    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        var runner = other.GetComponent<CorridorRunnerController>();
        if (runner == null) return;

        _triggered = true;
        runner.Activate();
    }
}
