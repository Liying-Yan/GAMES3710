using System;
using UnityEngine;

/// <summary>
/// Virtual pursuer that creates time pressure in corridor runner levels.
/// No physical entity — just a distance value that shrinks when the player is stuck.
/// Triggers game over via GameOverUI when the pursuer catches up.
/// </summary>
public class CorridorChaseManager : MonoBehaviour
{
    public static CorridorChaseManager Instance { get; private set; }

    [Header("Pursuer")]
    [Tooltip("Starting distance between pursuer and player")]
    public float InitialDistance = 30f;
    [Tooltip("Pursuer movement speed (should be slightly below RunSpeed)")]
    public float PursuerSpeed = 9f;
    [Tooltip("Bonus distance gained per second while running smoothly")]
    public float RecoveryBonus = 2f;

    [Header("Feedback")]
    [Tooltip("Distance at which danger ratio reaches 1.0 (max urgency)")]
    public float MaxDangerDistance = 10f;

    /// <summary>Current distance between pursuer and player. 0 = caught.</summary>
    public float PursuerDistance { get; private set; }

    /// <summary>
    /// Danger ratio from 0 (safe) to 1 (about to be caught).
    /// Use this to drive vignette intensity, heartbeat volume, etc.
    /// </summary>
    public float DangerRatio => Mathf.Clamp01(1f - PursuerDistance / MaxDangerDistance);

    /// <summary>Fired when the pursuer catches the player.</summary>
    public event Action OnCaught;

    private CorridorRunnerController _runner;
    private bool _chaseActive;
    private bool _caught;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        _runner = FindAnyObjectByType<CorridorRunnerController>();
        if (_runner == null)
        {
            Debug.LogWarning("CorridorChaseManager: No CorridorRunnerController found.");
            return;
        }

        PursuerDistance = InitialDistance;
        // Don't start chase yet — wait for runner activation
        _chaseActive = false;
    }

    public void StartChase()
    {
        PursuerDistance = InitialDistance;
        _caught = false;
        _chaseActive = true;
    }

    private void Update()
    {
        if (!_chaseActive || _caught) return;
        if (GameOverUI.IsGameOver) return;

        float playerSpeed = _runner != null ? _runner.ForwardSpeed : 0f;

        // Distance delta: player speed vs pursuer speed
        float delta = playerSpeed - PursuerSpeed;

        // When player is running well, grant a recovery bonus
        if (playerSpeed > PursuerSpeed)
            delta += RecoveryBonus;

        PursuerDistance += delta * Time.deltaTime;

        // Clamp to not exceed initial distance
        PursuerDistance = Mathf.Min(PursuerDistance, InitialDistance);

        if (PursuerDistance <= 0f)
        {
            PursuerDistance = 0f;
            TriggerCaught();
        }
    }

    private void TriggerCaught()
    {
        _caught = true;
        _chaseActive = false;

        OnCaught?.Invoke();

        if (GameOverUI.Instance != null)
            GameOverUI.Instance.Show();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
