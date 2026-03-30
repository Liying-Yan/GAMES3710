using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using StarterAssets;

public class CabinetHidingSpot : MonoBehaviour
{
    [Header("Cabinet References")]
    [Tooltip("The cabinet body child – player camera moves to its position")]
    public Transform cabinetBody;
    [Tooltip("The cabinet door child – pivot must be at the hinge")]
    public Transform cabinetDoor;
    [Tooltip("Where the player appears after exiting (place in front of the cabinet)")]
    public Transform exitPoint;

    [Header("Door Settings")]
    [Tooltip("Door open angle around local Y axis (positive = clockwise)")]
    public float doorOpenAngle = 90f;
    [Tooltip("Duration of door open/close animation")]
    public float doorDuration = 0.5f;

    [Header("Camera Transition")]
    [Tooltip("Duration of camera move in/out")]
    public float cameraDuration = 0.5f;
    [Tooltip("Offset from cabinet body pivot in body's local space (tweak to fine-tune hiding position)")]
    public Vector3 hidePositionOffset = Vector3.zero;

    [Header("Hiding Look Clamp")]
    [Tooltip("Max yaw deviation (left/right) while hiding")]
    public float maxYaw = 45f;
    [Tooltip("Max pitch deviation (up/down) while hiding")]
    public float maxPitch = 30f;

    [Header("Prompt Settings")]
    public string enterPrompt = "Press E to hide";
    public string exitPrompt = "Press E to exit";

    [Header("Breath Audio")]
    public AudioSource breathAudio;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("SFX")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenClip;
    public AudioClip doorCloseClip;

    // runtime refs (found automatically)
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
    private Quaternion _cabinetForwardRot;

    // player ground height when entering (used for safe exit)
    private float _entryGroundY;

    // collider cache
    private Collider[] _cabinetColliders;

    private void Start()
    {
        if (breathAudio != null)
        {
            breathAudio.playOnAwake = false;
            breathAudio.loop = true;
        }

        // cache all solid (non-trigger) colliders on body & door for toggling
        var allColliders = new System.Collections.Generic.List<Collider>();
        if (cabinetBody != null)
            foreach (var c in cabinetBody.GetComponentsInChildren<Collider>())
                if (!c.isTrigger) allColliders.Add(c);
        if (cabinetDoor != null)
            foreach (var c in cabinetDoor.GetComponentsInChildren<Collider>())
                if (!c.isTrigger) allColliders.Add(c);
        _cabinetColliders = allColliders.ToArray();
    }

    private void Update()
    {
        if (_isAnimating) return;

        // toggle hide
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (_isHiding)
                StartCoroutine(ExitCabinet());
            else if (_playerInRange)
                StartCoroutine(EnterCabinet());
        }

        // handle restricted look while hiding
        if (_isHiding && !_isAnimating)
        {
            HandleHidingLook();
        }
    }

    // ──────────────────────────────────────────────
    // Enter
    // ──────────────────────────────────────────────
    private IEnumerator EnterCabinet()
    {
        _isAnimating = true;
        CachePlayerRefs();
        HidePrompt();

        // lock everything
        _fps.LockMovement = true;
        _fps.LockCamera = true;

        // disable cabinet colliders so door/body don't push the player
        SetCabinetColliders(false);

        // 1. open door
        PlayDoorSfx(doorOpenClip);
        yield return StartCoroutine(AnimateDoor(0f, doorOpenAngle, doorDuration));

        // 2. move player into cabinet
        _entryGroundY = _fps.transform.position.y;
        _charCtrl.enabled = false;

        Vector3 startPos = _fps.transform.position;
        Quaternion startRot = _fps.transform.rotation;
        Vector3 targetPos = cabinetBody.TransformPoint(hidePositionOffset);
        targetPos.y = _entryGroundY + hidePositionOffset.y;
        _cabinetForwardRot = Quaternion.LookRotation(transform.forward, Vector3.up);

        yield return StartCoroutine(MovePlayer(startPos, targetPos, startRot, _cabinetForwardRot, cameraDuration));

        // sync pitch to 0 (looking straight)
        _fps.SetCameraPitch(0f);

        // 3. close door
        PlayDoorSfx(doorCloseClip);
        yield return StartCoroutine(AnimateDoor(doorOpenAngle, 0f, doorDuration));

        // 4. enter hiding state
        _isHiding = true;
        _isAnimating = false;
        SetHideState(true);

        // reset hiding look accumulators
        _hidingYaw = 0f;
        _hidingPitch = 0f;

        UpdatePrompt();
    }

    // ──────────────────────────────────────────────
    // Exit
    // ──────────────────────────────────────────────
    private IEnumerator ExitCabinet()
    {
        _isAnimating = true;
        HidePrompt();

        // 1. snap view back to cabinet forward
        _fps.transform.rotation = _cabinetForwardRot;
        _fps.SetCameraPitch(0f);
        _hidingYaw = 0f;
        _hidingPitch = 0f;

        // 2. open door
        PlayDoorSfx(doorOpenClip);
        yield return StartCoroutine(AnimateDoor(0f, doorOpenAngle, doorDuration));

        // 3. move player out (use entry ground Y so player doesn't clip through floor)
        Vector3 startPos = _fps.transform.position;
        Quaternion startRot = _fps.transform.rotation;
        Vector3 targetPos = new Vector3(exitPoint.position.x, _entryGroundY, exitPoint.position.z);
        Quaternion targetRot = exitPoint.rotation;

        yield return StartCoroutine(MovePlayer(startPos, targetPos, startRot, targetRot, cameraDuration));

        _charCtrl.enabled = true;

        // 4. close door
        PlayDoorSfx(doorCloseClip);
        yield return StartCoroutine(AnimateDoor(doorOpenAngle, 0f, doorDuration));

        // 5. restore control
        _fps.SetCameraPitch(0f);
        _fps.LockCamera = false;
        _fps.LockMovement = false;

        _isHiding = false;
        _isAnimating = false;
        SetHideState(false);

        // restore cabinet colliders now that player is fully out
        SetCabinetColliders(true);

        UpdatePrompt();
    }

    // ──────────────────────────────────────────────
    // Hiding look (restricted yaw / pitch)
    // ──────────────────────────────────────────────
    private void HandleHidingLook()
    {
        // continuously sync position so hidePositionOffset can be tweaked at runtime
        Vector3 pos = cabinetBody.TransformPoint(hidePositionOffset);
        pos.y = _entryGroundY + hidePositionOffset.y;
        _fps.transform.position = pos;

        if (_input == null || _input.look.sqrMagnitude < 0.01f) return;

        float deltaTimeMul = (_playerInput != null && _playerInput.currentControlScheme == "KeyboardMouse")
            ? 1.0f
            : Time.deltaTime;

        float rotSpeed = _fps.RotationSpeed;

        _hidingYaw += _input.look.x * rotSpeed * deltaTimeMul;
        _hidingPitch += _input.look.y * rotSpeed * deltaTimeMul;

        _hidingYaw = Mathf.Clamp(_hidingYaw, -maxYaw, maxYaw);
        _hidingPitch = Mathf.Clamp(_hidingPitch, -maxPitch, maxPitch);

        // apply yaw to player body
        _fps.transform.rotation = _cabinetForwardRot * Quaternion.Euler(0f, _hidingYaw, 0f);

        // apply pitch to cinemachine target
        _cinemachineTarget.transform.localRotation = Quaternion.Euler(_hidingPitch, 0f, 0f);
    }

    // ──────────────────────────────────────────────
    // Animation helpers
    // ──────────────────────────────────────────────
    private IEnumerator AnimateDoor(float fromAngle, float toAngle, float duration)
    {
        Quaternion startRot = Quaternion.Euler(0f, fromAngle, 0f);
        Quaternion endRot = Quaternion.Euler(0f, toAngle, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            cabinetDoor.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        cabinetDoor.localRotation = endRot;
    }

    private IEnumerator MovePlayer(Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _fps.transform.position = Vector3.Lerp(fromPos, toPos, t);
            _fps.transform.rotation = Quaternion.Slerp(fromRot, toRot, t);
            yield return null;
        }

        _fps.transform.position = toPos;
        _fps.transform.rotation = toRot;
    }

    // ──────────────────────────────────────────────
    // Audio & State helpers
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

    private void PlayDoorSfx(AudioClip clip)
    {
        if (doorAudioSource != null && clip != null)
            doorAudioSource.PlayOneShot(clip);
    }

    private void SetCabinetColliders(bool enabled)
    {
        foreach (var c in _cabinetColliders)
            if (c != null) c.enabled = enabled;
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

            if (!_isHiding)
                HidePrompt();
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
