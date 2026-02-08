using UnityEngine;
using UnityEngine.InputSystem;

public class PillPickup : Interactable
{
    [Header("Pill Settings")]
    public string displayName = "Pill";

    private void Update()
    {
        if (PlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.AddPill();
        }

        if (PromptUI.Instance != null)
        {
            PromptUI.Instance.Show("Obtained " + displayName);
        }

        HidePrompt();
        Destroy(gameObject);
    }

    protected override void OnPlayerEnter()
    {
        ShowPrompt("Press E to pick up");
    }

    protected override void OnPlayerExit()
    {
        HidePrompt();
    }
}
