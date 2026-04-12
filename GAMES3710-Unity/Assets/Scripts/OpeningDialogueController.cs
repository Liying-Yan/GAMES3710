using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class OpeningDialogueController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)]
        public string text;
    }

    [Header("Dialogue")]
    public DialogueLine[] lines;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("UI")]
    public GameObject dialoguePanel;
    public Image blackFadeImage;
    public float fadeDuration = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip boomSfx;

    [Header("Scene")]
    public string nextSceneName = "MainGameScene";

    private int currentIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        if (blackFadeImage != null)
        {
            Color c = blackFadeImage.color;
            c.a = 0f;
            blackFadeImage.color = c;
        }

        ShowCurrentLine();
    }

    private void Update()
    {
        if (isTransitioning) return;

        bool pressedContinue =
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (pressedContinue)
        {
            currentIndex++;

            if (currentIndex < lines.Length)
            {
                ShowCurrentLine();
            }
            else
            {
                StartCoroutine(EndSequence());
            }
        }
    }

    private void ShowCurrentLine()
    {
        if (lines == null || lines.Length == 0) return;

        nameText.text = lines[currentIndex].speaker;
        dialogueText.text = lines[currentIndex].text;
    }

    private IEnumerator EndSequence()
    {
        isTransitioning = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (boomSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(boomSfx);
        }

        float elapsed = 0f;
        Color startColor = blackFadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (blackFadeImage != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(0f, 1f, t);
                blackFadeImage.color = c;
            }

            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}