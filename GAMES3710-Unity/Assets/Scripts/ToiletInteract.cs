using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

public class ToiletInteract : Interactable
{
    [Header("References")]
    public Transform lidPivot;
    public Transform toiletLookTarget;
    public Camera playerCamera;

    [Header("Player Control")]
    public MonoBehaviour playerControllerScript;

    [Header("Camera (Cinemachine)")]
    public CinemachineBrain cinemachineBrain;

    [Header("Lid Rotation")]
    public Vector3 closedRotation = Vector3.zero;
    public Vector3 openRotation = new Vector3(-90f, 0f, 0f);
    public float lidRotateSpeed = 4f;

    [Header("Camera")]
    public Vector3 cameraOffset = new Vector3(0f, 1.2f, -0.8f);
    public float cameraMoveDuration = 0.6f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip flushSound;

    [Header("Prompt")]
    public string enterPrompt = "Press E to use toilet";
    public string exitPrompt = "Press E to stop using toilet";

    private bool isUsing = false;
    private bool isAnimating = false;

    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private Transform originalCamParent;

    private void Start()
    {
        if (lidPivot != null)
            lidPivot.localEulerAngles = closedRotation;
    }

    private void Update()
    {
        if (PlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && !isAnimating)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (isAnimating) return;

        if (!isUsing)
            StartCoroutine(EnterToiletUse());
        else
            StartCoroutine(ExitToiletUse());
    }

    private IEnumerator EnterToiletUse()
    {
        isAnimating = true;
        isUsing = true;

        HidePrompt();

        if (playerControllerScript != null)
            playerControllerScript.enabled = false;

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = false;

        if (playerCamera != null)
        {
            originalCamPosition = playerCamera.transform.position;
            originalCamRotation = playerCamera.transform.rotation;
            originalCamParent = playerCamera.transform.parent;
            playerCamera.transform.SetParent(null);
        }

        StartCoroutine(RotateLid(openRotation));

        if (playerCamera != null && toiletLookTarget != null)
        {
            Vector3 targetPos = toiletLookTarget.position + toiletLookTarget.TransformDirection(cameraOffset);
            Quaternion targetRot = Quaternion.LookRotation(toiletLookTarget.position - targetPos);

            float elapsed = 0f;
            Vector3 startPos = playerCamera.transform.position;
            Quaternion startRot = playerCamera.transform.rotation;

            while (elapsed < cameraMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / cameraMoveDuration);

                playerCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                playerCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

                yield return null;
            }

            playerCamera.transform.position = targetPos;
            playerCamera.transform.rotation = targetRot;
        }

        if (PlayerInRange)
            ShowPrompt(exitPrompt);

        isAnimating = false;
    }

    private IEnumerator ExitToiletUse()
    {
        isAnimating = true;
        isUsing = false;

        HidePrompt();

        if (playerCamera != null)
        {
            float elapsed = 0f;
            Vector3 startPos = playerCamera.transform.position;
            Quaternion startRot = playerCamera.transform.rotation;

            while (elapsed < cameraMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / cameraMoveDuration);

                playerCamera.transform.position = Vector3.Lerp(startPos, originalCamPosition, t);
                playerCamera.transform.rotation = Quaternion.Slerp(startRot, originalCamRotation, t);

                yield return null;
            }

            playerCamera.transform.position = originalCamPosition;
            playerCamera.transform.rotation = originalCamRotation;
            playerCamera.transform.SetParent(originalCamParent);
        }

        yield return StartCoroutine(RotateLid(closedRotation));

        yield return new WaitForSeconds(0.2f);

        if (audioSource != null && flushSound != null)
            audioSource.PlayOneShot(flushSound);

        if (playerControllerScript != null)
            playerControllerScript.enabled = true;

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = true;

        if (PlayerInRange)
            ShowPrompt(enterPrompt);

        isAnimating = false;
    }

    private IEnumerator RotateLid(Vector3 targetRotation)
    {
        if (lidPivot == null) yield break;

        Quaternion startRot = lidPivot.localRotation;
        Quaternion endRot = Quaternion.Euler(targetRotation);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * lidRotateSpeed;
            lidPivot.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        lidPivot.localRotation = endRot;
    }

    protected override void OnPlayerEnter()
    {
        if (!isUsing)
            ShowPrompt(enterPrompt);
        else
            ShowPrompt(exitPrompt);
    }

    protected override void OnPlayerExit()
    {
        HidePrompt();
    }
}