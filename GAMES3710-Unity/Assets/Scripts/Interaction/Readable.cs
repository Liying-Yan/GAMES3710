using UnityEngine;
using UnityEngine.InputSystem;

public class Readable : Interactable
{
    [Header("Reading Settings")]
    [TextArea(3, 10)]
    public string content;

    [SerializeField] private string promptMessage = "Press E to read";

    private void Update()
    {
        if (PlayerInRange && Keyboard.current.eKey.wasPressedThisFrame && !ReadingUI.IsReading)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        HidePrompt();

        if (ReadingUI.Instance != null)
        {
            ReadingUI.Instance.Show(content);
        }
    }

    protected override void OnPlayerEnter()
    {
        ShowPrompt(promptMessage);
    }

    protected override void OnPlayerExit()
    {
        HidePrompt();
    }
}
