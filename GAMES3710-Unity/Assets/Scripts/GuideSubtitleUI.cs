using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GuideSubtitleUI : MonoBehaviour
{
    public static GuideSubtitleUI Instance { get; private set; }

    [Header("Settings")]
    public float fadeDuration = 0.35f;
    public float defaultDuration = 3f;
    public Vector2 panelOffset = new Vector2(0f, 60f);
    public int fontSize = 28;

    private GameObject panelObject;
    private TextMeshProUGUI subtitleText;
    private CanvasGroup canvasGroup;
    private Coroutine currentRoutine;

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
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        CreateSubtitleUI();
        panelObject.SetActive(false);
    }

    public void Show(string text)
    {
        Show(text, defaultDuration);
    }

    public void Show(string text, float duration)
    {
        if (string.IsNullOrWhiteSpace(text) || duration <= 0f) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(text, duration));
    }

    private IEnumerator ShowRoutine(string text, float duration)
    {
        subtitleText.text = text;
        panelObject.SetActive(true);
        canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(duration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        panelObject.SetActive(false);
        currentRoutine = null;
    }

    private void CreateSubtitleUI()
    {
        panelObject = new GameObject("GuideSubtitlePanel");
        panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = panelOffset;

        Image bg = panelObject.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);

        VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 12, 12);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = panelObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        canvasGroup = panelObject.AddComponent<CanvasGroup>();

        GameObject textObject = new GameObject("GuideSubtitleText");
        textObject.transform.SetParent(panelObject.transform, false);

        subtitleText = textObject.AddComponent<TextMeshProUGUI>();
        subtitleText.fontSize = fontSize;
        subtitleText.color = Color.white;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.textWrappingMode = TextWrappingModes.Normal;
    }
}