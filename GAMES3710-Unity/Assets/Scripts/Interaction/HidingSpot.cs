using UnityEngine;
using UnityEngine.InputSystem;

public class HidingSpot : MonoBehaviour
{
    [Header("Prompt Settings")]
    public string enterPrompt = "Press E to hide";
    public string exitPrompt = "Press E to exit";

    private bool _playerInRange;
    private bool _isPlayerHiding;

    private void Update()
    {
        if (_playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleHide();
        }
    }

    private void ToggleHide()
    {
        _isPlayerHiding = !_isPlayerHiding;

        if (PlayerHideState.Instance != null)
        {
            PlayerHideState.Instance.SetHiding(_isPlayerHiding);
        }

        UpdatePrompt();
    }

    private void UpdatePrompt()
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.Show(_isPlayerHiding ? exitPrompt : enterPrompt);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Show(enterPrompt);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Hide();
            }

            if (_isPlayerHiding)
            {
                _isPlayerHiding = false;
                if (PlayerHideState.Instance != null)
                {
                    PlayerHideState.Instance.SetHiding(false);
                }
            }
        }
    }
}
