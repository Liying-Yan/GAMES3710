using System;
using System.Collections;
using UnityEngine;
using StarterAssets;

/// <summary>
/// Auto-runner controller for Temple Run-like corridor levels.
/// Attach to the same GameObject as FirstPersonController.
/// Takes over movement via LockMovement/LockCamera flags.
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(CharacterController))]
public class CorridorRunnerController : MonoBehaviour
{
    [Header("Speed")]
    [Tooltip("Auto-run forward speed")]
    public float RunSpeed = 10.0f;
    [Tooltip("Lateral strafe speed (A/D)")]
    public float StrafeSpeed = 4.0f;
    [Tooltip("How fast speed ramps up to target")]
    public float SpeedChangeRate = 10.0f;

    [Header("Turn")]
    [Tooltip("Duration of a smooth 90-degree turn in seconds")]
    public float TurnDuration = 0.3f;

    [Header("Camera")]
    [Tooltip("Fixed camera pitch during auto-run (degrees, negative = look down)")]
    public float CameraPitch = -5.0f;

    [Header("Activation")]
    [Tooltip("Activate auto-run mode on Start")]
    public bool ActivateOnStart = false;

    /// <summary>Current forward speed (used by chase manager).</summary>
    public float ForwardSpeed { get; private set; }

    /// <summary>True while a turn coroutine is executing.</summary>
    public bool IsTurning { get; private set; }

    /// <summary>Fired when the player's forward speed is near zero (stuck on obstacle).</summary>
    public event Action OnPlayerStuck;

    private FirstPersonController _fps;
    private CharacterController _controller;
    private StarterAssetsInputs _input;

    private float _currentSpeed;
    private float _verticalVelocity;
    private float _stuckTimer;

    private const float Gravity = -15.0f;
    private const float TerminalVelocity = 53.0f;
    private const float GroundedVelocity = -2.0f;
    private const float StuckThreshold = 1.0f;
    private const float StuckNotifyInterval = 0.5f;

    private bool _active;

    private void Start()
    {
        _fps = GetComponent<FirstPersonController>();
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();

        if (ActivateOnStart)
            Activate();
    }

    public void Activate()
    {
        if (_active) return;
        _active = true;

        _fps.LockMovement = true;
        _fps.LockCamera = true;

        // Set a fixed camera pitch
        _fps.SetCameraPitch(CameraPitch);

        _currentSpeed = 0f;
        _stuckTimer = 0f;

        // Start the chase
        if (CorridorChaseManager.Instance != null)
            CorridorChaseManager.Instance.StartChase();
    }

    public void Deactivate()
    {
        if (!_active) return;
        _active = false;

        _fps.LockMovement = false;
        _fps.LockCamera = false;

        StopAllCoroutines();
        IsTurning = false;
    }

    private void Update()
    {
        if (!_active) return;
        if (GameOverUI.IsGameOver) return;

        ApplyGravity();
        HandleMovement();
        TrackStuck();
    }

    private void HandleMovement()
    {
        // Smooth acceleration to run speed
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, RunSpeed, SpeedChangeRate * Time.deltaTime);

        // Forward movement
        Vector3 move = transform.forward * _currentSpeed;

        // Lateral strafe (A/D), disabled during turns
        if (!IsTurning)
        {
            float strafe = _input.move.x;
            move += transform.right * (strafe * StrafeSpeed);
        }

        // Apply gravity
        move.y = _verticalVelocity;

        _controller.Move(move * Time.deltaTime);

        // Use actual velocity to detect if the player is blocked by obstacles
        Vector3 horizontalVel = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
        ForwardSpeed = Vector3.Dot(horizontalVel, transform.forward);
    }

    private void ApplyGravity()
    {
        if (_fps.Grounded)
        {
            if (_verticalVelocity < 0f)
                _verticalVelocity = GroundedVelocity;
        }

        if (_verticalVelocity > -TerminalVelocity)
            _verticalVelocity += Gravity * Time.deltaTime;
    }

    private void TrackStuck()
    {
        if (ForwardSpeed < StuckThreshold)
        {
            _stuckTimer += Time.deltaTime;
            if (_stuckTimer >= StuckNotifyInterval)
            {
                _stuckTimer = 0f;
                OnPlayerStuck?.Invoke();
            }
        }
        else
        {
            _stuckTimer = 0f;
        }
    }

    /// <summary>
    /// Execute a smooth turn. Called by CorridorTurnMarker.
    /// </summary>
    /// <param name="yawDelta">Rotation in degrees (+90 = right, -90 = left).</param>
    public void ExecuteTurn(float yawDelta)
    {
        if (IsTurning) return;
        StartCoroutine(TurnCoroutine(yawDelta));
    }

    private IEnumerator TurnCoroutine(float yawDelta)
    {
        IsTurning = true;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, yawDelta, 0f);
        float elapsed = 0f;

        while (elapsed < TurnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / TurnDuration);
            // Smooth step for nicer feel
            t = t * t * (3f - 2f * t);

            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            // Keep moving forward during the turn
            Vector3 move = transform.forward * (_currentSpeed * Time.deltaTime);
            move.y = _verticalVelocity * Time.deltaTime;
            _controller.Move(move);

            yield return null;
        }

        transform.rotation = endRot;
        IsTurning = false;
    }

    private void OnDisable()
    {
        if (_active)
            Deactivate();
    }
}
