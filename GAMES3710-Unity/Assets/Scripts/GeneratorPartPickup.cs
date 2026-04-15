using UnityEngine;
using UnityEngine.InputSystem;

public class GeneratorPartPickup : MonoBehaviour
{
    [Header("References")]
    public GeneratorCollectionManager collectionManager;

    [Header("Prompt")]
    public string promptText = "Press E to pick up";

    private bool playerInRange = false;
    private bool isPicked = false;

    private void Update()
    {
        if (isPicked) return;

        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUpPart();
        }
    }

    private void PickUpPart()
    {
        if (isPicked) return;

        isPicked = true;

        if (collectionManager != null)
        {
            collectionManager.RegisterCollected();
        }
        else
        {
            Debug.LogError("GeneratorPartPickup: collectionManager is null");
        }

        if (PromptUI.Instance != null)
        {
            PromptUI.Instance.Show("");
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        var highlight = GetComponent<InteractionHighlight>();
        if (highlight != null) highlight.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPicked) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (PromptUI.Instance != null)
            {
                PromptUI.Instance.Show(promptText);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (PromptUI.Instance != null)
            {
                PromptUI.Instance.Show("");
            }
        }
    }
}