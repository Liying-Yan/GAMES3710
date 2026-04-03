using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : Interactable
{
    [Header("Item Settings")]
    public string displayName = "Key";
    public bool consumable = true;

    [HideInInspector]
    public bool isPickedUp = false;

    // ===== 新增：音效相关 =====
    [Header("Audio")]
    public AudioSource audioSource;   // 用来播放音效
    public AudioClip pickupSound;     // 拾取音效
    // ========================

    private void Update()
    {
        if (PlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (isPickedUp) return;

        isPickedUp = true;

        // ===== 新增：播放音效 =====
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        // ========================

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddKey(this);
        }

        if (PromptUI.Instance != null)
        {
            PromptUI.Instance.Show("Obtained " + displayName);
        }

        HidePrompt();
        gameObject.SetActive(false);
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