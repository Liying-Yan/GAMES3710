using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : Interactable
{
    [Header("Item Settings")]
    public string displayName = "Key";
    public bool consumable = true;

    [HideInInspector]
    public bool isPickedUp = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

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

        if (pickupSound != null)
        {
            PlayPickupSound();
        }

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

    private void PlayPickupSound()
    {
        GameObject tempSound = new GameObject("TempPickupSound");
        tempSound.transform.position = transform.position;

        AudioSource tempSource = tempSound.AddComponent<AudioSource>();
        tempSource.clip = pickupSound;

        if (audioSource != null)
        {
            tempSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            tempSource.volume = audioSource.volume;
            tempSource.pitch = audioSource.pitch;
            tempSource.spatialBlend = audioSource.spatialBlend;
            tempSource.minDistance = audioSource.minDistance;
            tempSource.maxDistance = audioSource.maxDistance;
            tempSource.rolloffMode = audioSource.rolloffMode;
        }
        else
        {
            tempSource.volume = 1f;
            tempSource.spatialBlend = 1f;
            tempSource.minDistance = 1f;
            tempSource.maxDistance = 15f;
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        tempSource.Play();
        Destroy(tempSound, pickupSound.length + 0.1f);
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