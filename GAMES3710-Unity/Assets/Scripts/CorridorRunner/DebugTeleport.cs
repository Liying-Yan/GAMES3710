using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

/// <summary>
/// Debug utility: press F3 to teleport the player to a target position.
/// Attach to the player GameObject and assign the target in the inspector.
/// </summary>
public class DebugTeleport : MonoBehaviour
{
    [Tooltip("Teleport destination (any GameObject in the scene)")]
    public Transform Target;

    private CharacterController _controller;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Target == null) return;
        if (!Keyboard.current[Key.F3].wasPressedThisFrame) return;

        // CharacterController must be disabled to set position directly
        _controller.enabled = false;
        transform.position = Target.position;
        transform.rotation = Target.rotation;
        _controller.enabled = true;
    }
}
