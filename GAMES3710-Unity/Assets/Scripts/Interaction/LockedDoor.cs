using UnityEngine;
using UnityEngine.InputSystem;

public class LockedDoor : Interactable
{
    [Header("Door Settings")]
    public ItemType requiredItem = ItemType.None;
    public string requiredItemName = "Key";
    public bool consumeItem = true;

    [Header("Dependencies")]
    public LockedDoor[] requiredMechanisms;

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

        if (requiredItem != ItemType.None)
        {
            if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasItem(requiredItem))
            {
                if (PromptUI.Instance != null)
                {
                    PromptUI.Instance.Show("Requires " + requiredItemName);
                }
                return;
            }

            if (consumeItem)
            {
                PlayerInventory.Instance.RemoveItem(requiredItem);
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
        gameObject.SetActive(false);
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
