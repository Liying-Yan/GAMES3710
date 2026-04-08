using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class SubtitleUI : MonoBehaviour
{
    public static SubtitleUI Instance { get; private set; }

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    private GameObject _panel;
    private TMP_Text _text;
    private CanvasGroup _canvasGroup;
    private Coroutine _current;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Stretch to fill Canvas so child anchors work correctly
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        CreateUI();
        _panel.SetActive(false);
    }

    public void Show(string subtitle, AudioSource source)
    {
        if (string.IsNullOrEmpty(subtitle) || source == null) return;

        if (_current != null)
            StopCoroutine(_current);

        _current = StartCoroutine(ShowCoroutine(subtitle, source));
    }

    public void Show(string subtitle, float duration)
    {
        if (string.IsNullOrEmpty(subtitle) || duration <= 0f) return;

        if (_current != null)
            StopCoroutine(_current);

        _current = StartCoroutine(ShowCoroutine(subtitle, duration));
    }

    private IEnumerator ShowCoroutine(string subtitle, float duration)
    {
        _text.text = subtitle;
        _panel.SetActive(true);
        _canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(duration);

        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = 1f - elapsed / fadeDuration;
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _panel.SetActive(false);
        _current = null;
    }

    private IEnumerator ShowCoroutine(string subtitle, AudioSource source)
    {
        _text.text = subtitle;
        _panel.SetActive(true);
        _canvasGroup.alpha = 1f;

        // Wait until audio finishes
        while (source != null && source.isPlaying)
            yield return null;

        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = 1f - elapsed / fadeDuration;
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _panel.SetActive(false);
        _current = null;
    }

    private void CreateUI()
    {
        // Panel – bottom-center strip with semi-transparent background
        _panel = new GameObject("SubtitlePanel");
        _panel.transform.SetParent(transform, false);

        var panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 60f);

        var bg = _panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);

        // VerticalLayoutGroup provides padding; ContentSizeFitter makes height follow text
        var layout = _panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 12, 12);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = _panel.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _canvasGroup = _panel.AddComponent<CanvasGroup>();

        // Text (direct child of layout group)
        var textObj = new GameObject("SubtitleText");
        textObj.transform.SetParent(_panel.transform, false);

        _text = textObj.AddComponent<TextMeshProUGUI>();
        _text.fontSize = 28f;
        _text.color = Color.white;
        _text.alignment = TextAlignmentOptions.Center;
        _text.textWrappingMode = TextWrappingModes.Normal;
    }
}
