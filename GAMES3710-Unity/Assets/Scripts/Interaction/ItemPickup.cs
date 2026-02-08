using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : Interactable
{
    [Header("Item Settings")]
    public ItemType itemType;
    public string displayName = "Key";

    private void Update()
    {
        if (PlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(itemType);
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
