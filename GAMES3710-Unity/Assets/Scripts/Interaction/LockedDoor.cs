using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public enum DoorAnimationType { None, SwingDoor, AnimatorTrigger }

public class LockedDoor : Interactable
{
    [Header("Key Requirements")]
    public ItemPickup[] requiredKeys;

    [Header("Dependencies")]
    public LockedDoor[] requiredMechanisms;

    [Header("Door Animation")]
    public DoorAnimationType animationType = DoorAnimationType.None;
    public float swingAngle = -90f;
    public float swingDuration = 1f;
    public Transform doorTransform;
    public Animator mechanismAnimator;
    public string animatorTriggerName = "Activate";

    [Header("Prompt")]
    public string lockedPrompt = "Requires a key";

    [Header("State")]
    public bool isOpen;

    private void Update()
    {
        if (PlayerInRange && !isOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (isOpen) return;

        if (!CheckDependencies())
        {
            if (PromptUI.Instance != null)
            {
                PromptUI.Instance.Show("Requires another mechanism first");
            }
            return;
        }

        if (requiredKeys != null && requiredKeys.Length > 0)
        {
            foreach (var key in requiredKeys)
            {
                if (key == null) continue;
                if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasKey(key))
                {
                    if (PromptUI.Instance != null)
                    {
                        PromptUI.Instance.Show(lockedPrompt);
                    }
                    return;
                }
            }

            foreach (var key in requiredKeys)
            {
                if (key != null && key.consumable)
                {
                    PlayerInventory.Instance.RemoveKey(key);
                }
            }
        }

        Open();
    }

    private bool CheckDependencies()
    {
        if (requiredMechanisms == null) return true;

        foreach (var mechanism in requiredMechanisms)
        {
            if (mechanism != null && !mechanism.isOpen)
            {
                return false;
            }
        }
        return true;
    }

    private void Open()
    {
        isOpen = true;
        HidePrompt();

        switch (animationType)
        {
            case DoorAnimationType.None:
                gameObject.SetActive(false);
                break;
            case DoorAnimationType.SwingDoor:
                StartCoroutine(SwingDoorCoroutine());
                break;
            case DoorAnimationType.AnimatorTrigger:
                if (mechanismAnimator != null)
                {
                    mechanismAnimator.SetTrigger(animatorTriggerName);
                }
                break;
        }
    }

    private IEnumerator SwingDoorCoroutine()
    {
        Quaternion startRot = doorTransform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, swingAngle, 0f);
        float elapsed = 0f;

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            doorTransform.localRotation = Quaternion.Lerp(startRot, endRot, elapsed / swingDuration);
            yield return null;
        }

        doorTransform.localRotation = endRot;
    }

    protected override void OnPlayerEnter()
    {
        if (!isOpen)
        {
            ShowPrompt("Press E to interact");
        }
    }

    protected override void OnPlayerExit()
    {
        HidePrompt();
    }
}
