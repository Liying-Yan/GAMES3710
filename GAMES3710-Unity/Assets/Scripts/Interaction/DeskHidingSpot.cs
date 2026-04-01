using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using StarterAssets;

public class DeskHidingSpot : MonoBehaviour, ICheckableHidingSpot
{
    [Header("Enemy Check")]
    [Tooltip("Allow enemies to find the player under this desk")]
    public bool canBeChecked = false;

    [Header("Desk References")]
    [Tooltip("Child transform whose position = hiding target, forward (Z) = view direction")]
    public Transform hidePoint;
    [Tooltip("Optional exit point. If empty, player returns to entry position")]
    public Transform exitPoint;

    [Header("Animation Durations")]
    [Tooltip("Duration of the crouch / stand-up phase")]
    public float crouchDuration = 0.4f;
    [Tooltip("Duration of the horizontal slide in / out")]
    public float moveDuration = 0.5f;
    [Tooltip("Duration of the turn-around phase")]
    public float rotateDuration = 0.3f;

    [Header("Hiding Position")]
    [Tooltip("Offset from hidePoint pivot in its local space")]
    public Vector3 hidePositionOffset = Vector3.zero;

    [Header("Camera")]
    [Tooltip("Near clip plane while hiding (smaller = less clipping on nearby surfaces)")]
    public float hidingNearClip = 0.01f;

    [Header("Hiding Look Clamp")]
    public float maxYaw = 45f;
    public float maxPitch = 30f;

    [Header("Prompt Settings")]
    public string enterPrompt = "Press E to hide";
    public string exitPrompt = "Press E to exit";

    [Header("Breath Audio")]
    public AudioSource breathAudio;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    // runtime refs
    private FirstPersonController _fps;
    private StarterAssetsInputs _input;
    private CharacterController _charCtrl;
    private PlayerInput _playerInput;
    private GameObject _cinemachineTarget;

    // state
    private bool _playerInRange;
    private bool _isHiding;
    private bool _isAnimating;

    // hiding look
    private float _hidingYaw;
    private float _hidingPitch;
    private Quaternion _hideForwardRot;

    // entry snapshot
    private float _entryGroundY;
    private Vector3 _entryPosition;
    private Quaternion _entryRotation;

    // camera local Y offset (so hidePoint = camera position, not feet position)
    private float _cameraOffsetY;

    // original near clip to restore on exit
    private float _originalNearClip;

    private void Start()
    {
        if (breathAudio != null)
        {
            breathAudio.playOnAwake = false;
            breathAudio.loop = true;
        }
    }

    private void Update()
    {
        if (_isAnimating) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (_isHiding)
                StartCoroutine(ExitDesk());
            else if (_playerInRange)
                StartCoroutine(EnterDesk());
        }

        if (_isHiding && !_isAnimating)
            HandleHidingLook();
    }

    // ──────────────────────────────────────────────
    // Enter: crouch → slide in → turn
    // ──────────────────────────────────────────────
    private IEnumerator EnterDesk()
    {
        _isAnimating = true;
        CachePlayerRefs();
        HidePrompt();

        // immediately invisible to enemy
        SetHideState(true);
        if (PlayerHideState.Instance != null)
            PlayerHideState.Instance.CurrentSpot = this;

        // switch near clip
        _originalNearClip = Camera.main.nearClipPlane;
        Camera.main.nearClipPlane = hidingNearClip;

        _fps.LockMovement = true;
        _fps.LockCamera = true;

        // snapshot entry state
        _entryGroundY = _fps.transform.position.y;
        _entryPosition = _fps.transform.position;
        _entryRotation = _fps.transform.rotation;
        _cameraOffsetY = _cinemachineTarget.transform.localPosition.y;

        _charCtrl.enabled = false;

        // hidePoint = intended camera position; lower player by cameraOffsetY so camera lands there
        Vector3 hidePos = hidePoint.TransformPoint(hidePositionOffset);
        float targetPlayerY = hidePos.y - _cameraOffsetY;
        _hideForwardRot = Quaternion.LookRotation(hidePoint.forward, Vector3.up);

        // 1. crouch – lower Y while keeping XZ
        Vector3 crouchPos = new Vector3(
            _fps.transform.position.x,
            targetPlayerY,
            _fps.transform.position.z);
        yield return StartCoroutine(AnimatePosition(_fps.transform.position, crouchPos, crouchDuration));

        // 2. slide in – move XZ while keeping Y
        Vector3 slideTarget = new Vector3(hidePos.x, targetPlayerY, hidePos.z);
        yield return StartCoroutine(AnimatePosition(crouchPos, slideTarget, moveDuration));

        // 3. turn to face hide direction
        yield return StartCoroutine(AnimateRotation(_fps.transform.rotation, _hideForwardRot, rotateDuration));

        _fps.SetCameraPitch(0f);

        _isHiding = true;
        _isAnimating = false;
        _hidingYaw = 0f;
        _hidingPitch = 0f;

        UpdatePrompt();
    }

    // ──────────────────────────────────────────────
    // Exit: turn → slide out → stand up
    // ──────────────────────────────────────────────
    private IEnumerator ExitDesk()
    {
        _isAnimating = true;
        HidePrompt();

        // snap view back to hide forward before animating out
        _fps.transform.rotation = _hideForwardRot;
        _fps.SetCameraPitch(0f);
        _hidingYaw = 0f;
        _hidingPitch = 0f;

        // resolve exit target
        Vector3 exitPos;
        Quaternion exitRot;
        if (exitPoint != null)
        {
            exitPos = new Vector3(exitPoint.position.x, _fps.transform.position.y, exitPoint.position.z);
            exitRot = exitPoint.rotation;
        }
        else
        {
            exitPos = new Vector3(_entryPosition.x, _fps.transform.position.y, _entryPosition.z);
            exitRot = _entryRotation;
        }

        // 1. turn toward exit
        yield return StartCoroutine(AnimateRotation(_fps.transform.rotation, exitRot, rotateDuration));

        // 2. slide out horizontally
        yield return StartCoroutine(AnimatePosition(_fps.transform.position, exitPos, moveDuration));

        // 3. stand up – raise Y back to entry ground level
        Vector3 standPos = new Vector3(exitPos.x, _entryGroundY, exitPos.z);
        yield return StartCoroutine(AnimatePosition(exitPos, standPos, crouchDuration));

        _charCtrl.enabled = true;

        _fps.SetCameraPitch(0f);
        _fps.LockCamera = false;
        _fps.LockMovement = false;

        _isHiding = false;
        _isAnimating = false;
        SetHideState(false);

        // restore near clip
        Camera.main.nearClipPlane = _originalNearClip;

        UpdatePrompt();
    }

    // ──────────────────────────────────────────────
    // Hiding look (restricted)
    // ──────────────────────────────────────────────
    private void HandleHidingLook()
    {
        // live-sync position (hidePoint = camera target, so lower by cameraOffsetY)
        Vector3 pos = hidePoint.TransformPoint(hidePositionOffset);
        pos.y -= _cameraOffsetY;
        _fps.transform.position = pos;

        if (_input == null || _input.look.sqrMagnitude < 0.01f) return;

        float dtMul = (_playerInput != null && _playerInput.currentControlScheme == "KeyboardMouse")
            ? 1.0f
            : Time.deltaTime;
        float rotSpeed = _fps.RotationSpeed;

        _hidingYaw += _input.look.x * rotSpeed * dtMul;
        _hidingPitch += _input.look.y * rotSpeed * dtMul;
        _hidingYaw = Mathf.Clamp(_hidingYaw, -maxYaw, maxYaw);
        _hidingPitch = Mathf.Clamp(_hidingPitch, -maxPitch, maxPitch);

        _fps.transform.rotation = _hideForwardRot * Quaternion.Euler(0f, _hidingYaw, 0f);
        _cinemachineTarget.transform.localRotation = Quaternion.Euler(_hidingPitch, 0f, 0f);
    }

    // ──────────────────────────────────────────────
    // Animation helpers
    // ──────────────────────────────────────────────
    private IEnumerator AnimatePosition(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _fps.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        _fps.transform.position = to;
    }

    private IEnumerator AnimateRotation(Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _fps.transform.rotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        _fps.transform.rotation = to;
    }

    // ──────────────────────────────────────────────
    // State helpers
    // ──────────────────────────────────────────────
    private void SetHideState(bool hiding)
    {
        if (PlayerHideState.Instance != null)
            PlayerHideState.Instance.SetHiding(hiding);

        if (hiding)
        {
            if (breathAudio != null) breathAudio.Play();
            if (mixer != null) mixer.SetFloat("PropVol", -80f);
        }
        else
        {
            if (breathAudio != null) breathAudio.Stop();
            if (mixer != null) mixer.SetFloat("PropVol", 0f);
        }
    }

    // ──────────────────────────────────────────────
    // ICheckableHidingSpot
    // ──────────────────────────────────────────────
    public bool CanBeChecked => canBeChecked;
    public Transform CheckTarget => exitPoint != null ? exitPoint : transform;

    public void OnEnemyCheck()
    {
        if (GameOverUI.Instance != null)
            GameOverUI.Instance.Show();
    }

    // ──────────────────────────────────────────────
    // Trigger & Prompt
    // ──────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            CachePlayerRefs();
            if (!_isHiding && InteractionPromptUI.Instance != null)
                InteractionPromptUI.Instance.Show(enterPrompt);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (!_isHiding) HidePrompt();
        }
    }

    private void UpdatePrompt()
    {
        if (InteractionPromptUI.Instance != null)
            InteractionPromptUI.Instance.Show(_isHiding ? exitPrompt : enterPrompt);
    }

    private void HidePrompt()
    {
        if (InteractionPromptUI.Instance != null)
            InteractionPromptUI.Instance.Hide();
    }

    private void CachePlayerRefs()
    {
        if (_fps != null) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        _fps = player.GetComponent<FirstPersonController>();
        _input = player.GetComponent<StarterAssetsInputs>();
        _charCtrl = player.GetComponent<CharacterController>();
        _playerInput = player.GetComponent<PlayerInput>();
        _cinemachineTarget = _fps.CinemachineCameraTarget;
    }
}
