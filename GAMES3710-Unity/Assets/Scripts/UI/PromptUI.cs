using UnityEngine;
using TMPro;
using System.Collections;

public class PromptUI : MonoBehaviour
{
    public static PromptUI Instance { get; private set; }

    [Header("References")]
    public TMP_Text promptText;

    [Header("Settings")]
    public float displayDuration = 2f;
    public float fadeDuration = 0.5f;

    private Coroutine _currentCoroutine;
    private CanvasGroup _canvasGroup;

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

        _canvasGroup = promptText.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = promptText.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        promptText.gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        _currentCoroutine = StartCoroutine(ShowCoroutine(message));
    }

    private IEnumerator ShowCoroutine(string message)
    {
        promptText.text = message;
        promptText.gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        promptText.gameObject.SetActive(false);
        _currentCoroutine = null;
    }
}
