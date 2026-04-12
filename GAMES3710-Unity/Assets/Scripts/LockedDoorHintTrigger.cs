using UnityEngine;
using UnityEngine.InputSystem;

public class LockedDoorGuideTrigger : MonoBehaviour
{
    [TextArea]
    public string lockedHint = "Locked... I need something to break it open.";

    public bool triggerOnlyOnce = true;

    private bool hasTriggered = false;
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TriggerHint();
        }
    }

    public void TriggerHint()
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        if (GuideSubtitleUI.Instance != null)
            GuideSubtitleUI.Instance.Show(lockedHint, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}