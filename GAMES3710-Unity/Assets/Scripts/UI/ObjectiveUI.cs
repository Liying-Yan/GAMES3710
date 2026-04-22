using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ObjectiveUI : MonoBehaviour
{
    public static ObjectiveUI Instance { get; private set; }

    [Header("Style (values are for 1080p, auto-scaled)")]
    [SerializeField] private float panelWidth = 480f;
    [SerializeField] private float panelPadding = 16f;
    [SerializeField] private float topOffset = 80f;
    [SerializeField] private float goalFontSize = 22f;
    [SerializeField] private float objectiveFontSize = 28f;
    [SerializeField] private float subTaskFontSize = 24f;
    [SerializeField] private Color bgColor = new Color(0f, 0f, 0f, 0.4f);
    [SerializeField] private Color goalColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color objectiveColor = Color.white;
    [SerializeField] private Color subTaskColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    [SerializeField] private Color completedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float strikethroughDelay = 0.2f;

    private const float ReferenceHeight = 1080f;
    private float _scale;

    private GameObject _panel;
    private TMP_Text _goalText;
    private TMP_Text _objectiveText;
    private CanvasGroup _canvasGroup;
    private List<TMP_Text> _subTaskTexts = new List<TMP_Text>();

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
    }

    private void Start()
    {
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        CreateUI();

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged += OnObjectiveChanged;
            ObjectiveManager.Instance.OnSubTaskCompleted += OnSubTaskCompleted;
            ObjectiveManager.Instance.OnAllObjectivesCompleted += OnAllCompleted;

            // Initial display
            if (ObjectiveManager.Instance.CurrentObjective != null)
                OnObjectiveChanged(ObjectiveManager.Instance.CurrentObjective);
            else
                _panel.SetActive(false);
        }
        else
        {
            _panel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveChanged -= OnObjectiveChanged;
            ObjectiveManager.Instance.OnSubTaskCompleted -= OnSubTaskCompleted;
            ObjectiveManager.Instance.OnAllObjectivesCompleted -= OnAllCompleted;
        }

        if (Instance == this)
            Instance = null;
    }

    private void OnObjectiveChanged(ObjectiveManager.Objective objective)
    {
        StopAllCoroutines();
        StartCoroutine(TransitionToObjective(objective));
    }

    private void OnSubTaskCompleted(ObjectiveManager.SubTask subTask)
    {
        for (int i = 0; i < _subTaskTexts.Count; i++)
        {
            var text = _subTaskTexts[i];
            if (text.name == subTask.id)
            {
                StartCoroutine(StrikethroughAnimation(text, subTask.description));
                break;
            }
        }
    }

    private void OnAllCompleted()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator TransitionToObjective(ObjectiveManager.Objective objective)
    {
        // Fade out
        if (_panel.activeSelf)
        {
            yield return FadeCanvasGroup(1f, 0f, fadeInDuration);
        }

        // Rebuild content
        RebuildContent(objective);

        // Fade in
        _panel.SetActive(true);
        yield return FadeCanvasGroup(0f, 1f, fadeInDuration);
    }

    private void RebuildContent(ObjectiveManager.Objective objective)
    {
        // Clear old sub-tasks
        foreach (var t in _subTaskTexts)
        {
            if (t != null)
                Destroy(t.gameObject);
        }
        _subTaskTexts.Clear();

        // Long-term goal
        if (ObjectiveManager.Instance != null &&
            !string.IsNullOrEmpty(ObjectiveManager.Instance.LongTermGoal))
        {
            _goalText.text = ObjectiveManager.Instance.LongTermGoal;
            _goalText.gameObject.SetActive(true);
        }
        else
        {
            _goalText.gameObject.SetActive(false);
        }

        // Current objective
        _objectiveText.text = ">" + " " + objective.description;

        // Sub-tasks
        foreach (var sub in objective.subTasks)
        {
            var textObj = CreateSubTaskText(sub);
            _subTaskTexts.Add(textObj);
        }
    }

    private TMP_Text CreateSubTaskText(ObjectiveManager.SubTask sub)
    {
        var obj = new GameObject(sub.id);
        obj.transform.SetParent(_panel.transform, false);

        var rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2((panelWidth - panelPadding * 2 - 16f) * _scale, 24f * _scale);

        var text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = subTaskFontSize * _scale;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

        if (sub.completed)
        {
            text.text = "  <s>" + sub.description + "</s>";
            text.color = completedColor;
        }
        else
        {
            text.text = "  " + sub.description;
            text.color = subTaskColor;
        }

        return text;
    }

    private IEnumerator StrikethroughAnimation(TMP_Text text, string description)
    {
        yield return new WaitForSeconds(strikethroughDelay);

        text.text = "  <s>" + description + "</s>";
        text.color = completedColor;
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(1f);
        yield return FadeCanvasGroup(1f, 0f, fadeInDuration);
        _panel.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        _canvasGroup.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = to;
    }

    private void CreateUI()
    {
        // Use Canvas rect height for scaling, not Screen.height
        var canvas = GetComponentInParent<Canvas>();
        float canvasHeight = ReferenceHeight;
        if (canvas != null)
        {
            var canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
                canvasHeight = canvasRect.rect.height;
        }
        _scale = canvasHeight / ReferenceHeight;

        float sw = panelWidth * _scale;
        float sp = panelPadding * _scale;
        float margin = 16f * _scale;
        float top = topOffset * _scale;

        // Panel - top right, below Sanity/Pill UI
        _panel = new GameObject("ObjectivePanel");
        _panel.transform.SetParent(transform, false);

        var panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-margin, -top);

        // Use ContentSizeFitter for auto-height
        var panelLayout = _panel.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset((int)sp, (int)sp, (int)sp, (int)sp);
        panelLayout.spacing = 4f * _scale;
        panelLayout.childAlignment = TextAnchor.UpperLeft;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        var sizeFitter = _panel.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        panelRect.sizeDelta = new Vector2(sw, 0f);

        var panelImage = _panel.AddComponent<Image>();
        panelImage.color = bgColor;
        panelImage.raycastTarget = false;

        _canvasGroup = _panel.AddComponent<CanvasGroup>();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        // Long-term goal text
        var goalObj = new GameObject("GoalText");
        goalObj.transform.SetParent(_panel.transform, false);
        goalObj.AddComponent<RectTransform>();
        _goalText = goalObj.AddComponent<TextMeshProUGUI>();
        _goalText.fontSize = goalFontSize * _scale;
        _goalText.fontStyle = FontStyles.Italic;
        _goalText.color = goalColor;
        _goalText.alignment = TextAlignmentOptions.TopLeft;
        _goalText.textWrappingMode = TMPro.TextWrappingModes.Normal;

        // Current objective text
        var objObj = new GameObject("ObjectiveText");
        objObj.transform.SetParent(_panel.transform, false);
        objObj.AddComponent<RectTransform>();
        _objectiveText = objObj.AddComponent<TextMeshProUGUI>();
        _objectiveText.fontSize = objectiveFontSize * _scale;
        _objectiveText.fontStyle = FontStyles.Bold;
        _objectiveText.color = objectiveColor;
        _objectiveText.alignment = TextAlignmentOptions.TopLeft;
        _objectiveText.textWrappingMode = TMPro.TextWrappingModes.Normal;

        // Sub-tasks are added directly to _panel.transform in RebuildContent
    }
}
