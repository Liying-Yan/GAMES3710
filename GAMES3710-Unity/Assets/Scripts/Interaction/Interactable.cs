using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected bool PlayerInRange;

    public abstract void Interact();

    protected virtual void ShowPrompt(string message)
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.Show(message);
        }
    }

    protected virtual void HidePrompt()
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.Hide();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInRange = true;
            OnPlayerEnter();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInRange = false;
            OnPlayerExit();
        }
    }

    protected virtual void OnPlayerEnter() { }
    protected virtual void OnPlayerExit() { }
}
