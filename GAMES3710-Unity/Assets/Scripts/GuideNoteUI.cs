using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GuideNoteUI : MonoBehaviour
{
    public static GuideNoteUI Instance { get; private set; }

    [Header("References")]
    public GameObject notePanel;
    public TextMeshProUGUI noteText;

    [Header("Close Key")]
    public Key closeKey = Key.F;   // 用 F 关闭，避免和 Esc 冲突

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }

        IsOpen = false;
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[closeKey].wasPressedThisFrame)
        {
            HideNote();
        }
    }

    public void ShowNote(string content)
    {
        Debug.Log("ShowNote called");

        if (notePanel == null)
        {
            Debug.LogError("GuideNoteUI: notePanel is null");
            return;
        }

        if (noteText == null)
        {
            Debug.LogError("GuideNoteUI: noteText is null");
            return;
        }

        notePanel.SetActive(true);
        noteText.text = content;

        IsOpen = true;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideNote()
    {
        if (notePanel != null)
        {
            notePanel.SetActive(false);
        }

        IsOpen = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}