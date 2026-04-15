using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance { get; private set; }
    public static bool IsEndScreen { get; private set; }

    private GameObject _panel;
    private StarterAssetsInputs _playerInput;

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
        _panel.SetActive(false);
        IsEndScreen = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsEndScreen = false;
        }
    }

    public void Show()
    {
        if (IsEndScreen) return;

        _panel.SetActive(true);
        Time.timeScale = 0f;

        if (_playerInput == null)
            _playerInput = FindAnyObjectByType<StarterAssetsInputs>();
        if (_playerInput != null)
        {
            _playerInput.cursorInputForLook = false;
            _playerInput.cursorLocked = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsEndScreen = true;
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CreateUI()
    {
        // Panel - fullscreen container
        _panel = new GameObject("EndScreenPanel");
        _panel.transform.SetParent(transform, false);

        var panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Black base layer
        var panelImage = _panel.AddComponent<Image>();
        panelImage.color = Color.black;

        // Background image - load from Resources
        var tex = Resources.Load<Texture2D>("end_bg");
        if (tex != null)
        {
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(_panel.transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));

            var bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = sprite;
            bgImage.raycastTarget = false;

            var fitter = bgObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = (float)tex.width / tex.height;
        }

        // Button
        CreateButton("QuitBtn", "Quit Game", 0f, OnQuit);
    }

    private void CreateButton(string name, string label, float yOffset, UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(_panel.transform, false);

        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.3f);
        btnRect.anchorMax = new Vector2(0.5f, 0.3f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0f, yOffset);
        btnRect.sizeDelta = new Vector2(300f, 50f);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        var button = btnObj.AddComponent<Button>();
        button.targetGraphic = btnImage;
        button.onClick.AddListener(onClick);

        // Button text
        var textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(btnObj.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = label;
        btnText.fontSize = 28f;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
    }
}
