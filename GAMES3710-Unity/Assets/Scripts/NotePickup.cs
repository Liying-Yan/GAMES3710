using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NotePickup : MonoBehaviour
{
    [Header("Note Content")]
    [TextArea(3, 8)]
    public string noteContent = "Maintenance Notice:\nAll maintenance tools have been moved to the basement storage room.";

    [TextArea]
    public string afterHint = "Basement storage room... maybe I can find something there.";

    [Header("Prompt")]
    public string promptText = "Press E to read note";

    private bool playerInRange = false;
    private bool hasPicked = false;

    private void Update()
    {
        if (hasPicked) return;

        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickNote();
        }
    }

    private void PickNote()
    {
        Debug.Log("PickNote called");

        hasPicked = true;

        if (GuideNoteUI.Instance != null)
        {
            GuideNoteUI.Instance.ShowNote(noteContent);
            StartCoroutine(WaitThenShowSubtitle());
        }
        else
        {
            Debug.LogError("GuideNoteUI.Instance is NULL");
        }

        if (PromptUI.Instance != null)
        {
            PromptUI.Instance.Show("");
        }

        // 关掉碰撞和渲染，但不销毁物体
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
    }

    private IEnumerator WaitThenShowSubtitle()
    {
        while (GuideNoteUI.Instance != null && GuideNoteUI.Instance.IsOpen)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.1f);

        if (GuideSubtitleUI.Instance != null)
        {
            GuideSubtitleUI.Instance.Show(afterHint, 3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPicked) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (PromptUI.Instance != null)
            {
                PromptUI.Instance.Show(promptText);
            }

            Debug.Log("Player entered note trigger");
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

            Debug.Log("Player left note trigger");
        }
    }
}