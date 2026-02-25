using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    private GameObject _panel;
    private StarterAssetsInputs _playerInput;
    private FirstPersonController _fpsController;
    private TMP_Text _sensitivityValueText;
    private bool _wasReading;

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
        IsPaused = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsPaused = false;
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame
            && !ReadingUI.IsReading
            && !_wasReading
            && !GameOverUI.IsGameOver)
        {
            if (IsPaused)
                Hide();
            else
                Show();
        }

        // Update at end of frame so next frame knows if reading was active this frame
        _wasReading = ReadingUI.IsReading;
    }

    public void Show()
    {
        _panel.SetActive(true);
        Time.timeScale = 0f;

        if (_playerInput == null)
            _playerInput = FindObjectOfType<StarterAssetsInputs>();
        if (_playerInput != null)
        {
            _playerInput.cursorInputForLook = false;
            _playerInput.cursorLocked = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsPaused = true;
    }

    public void Hide()
    {
        _panel.SetActive(false);
        Time.timeScale = 1f;

        if (_playerInput == null)
            _playerInput = FindObjectOfType<StarterAssetsInputs>();
        if (_playerInput != null)
        {
            _playerInput.cursorInputForLook = true;
            _playerInput.cursorLocked = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsPaused = false;
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        // Panel - fullscreen semi-transparent overlay
        _panel = new GameObject("PausePanel");
        _panel.transform.SetParent(transform, false);

        var panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var panelImage = _panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.7f);

        // Title - "PAUSED"
        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(_panel.transform, false);

        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(400f, 80f);

        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "PAUSED";
        titleText.fontSize = 60f;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;

        // Sensitivity slider
        CreateSensitivitySlider(80f);

        // Buttons
        CreateButton("ResumeBtn", "Resume", 0f, Hide);
        CreateButton("RestartBtn", "Restart", -70f, OnRestart);
        CreateButton("QuitBtn", "Quit Game", -140f, OnQuit);
    }

    private void CreateSensitivitySlider(float yOffset)
    {
        // Container
        var container = new GameObject("SensitivityRow");
        container.transform.SetParent(_panel.transform, false);

        var containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0f, yOffset);
        containerRect.sizeDelta = new Vector2(300f, 30f);

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0.35f, 1f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "Sensitivity";
        labelText.fontSize = 22f;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        // Value text
        var valueObj = new GameObject("Value");
        valueObj.transform.SetParent(container.transform, false);

        var valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.88f, 0f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;

        _sensitivityValueText = valueObj.AddComponent<TextMeshProUGUI>();
        _sensitivityValueText.fontSize = 22f;
        _sensitivityValueText.color = Color.white;
        _sensitivityValueText.alignment = TextAlignmentOptions.MidlineRight;

        // Slider
        var sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(container.transform, false);

        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.36f, 0f);
        sliderRect.anchorMax = new Vector2(0.86f, 1f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        // Background track
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);

        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.4f);
        bgRect.anchorMax = new Vector2(1f, 0.6f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Fill area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);

        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.4f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.6f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);

        var fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        var fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

        // Handle area
        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);

        var handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        var handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleArea.transform, false);

        var handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(16f, 24f);

        var handleImage = handleObj.AddComponent<Image>();
        handleImage.color = Color.white;

        // Wire up Slider component
        var slider = sliderObj.AddComponent<Slider>();
        slider.targetGraphic = handleImage;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0.1f;
        slider.maxValue = 3f;

        // Read current sensitivity
        if (_fpsController == null)
            _fpsController = FindObjectOfType<FirstPersonController>();
        float current = _fpsController != null ? _fpsController.RotationSpeed : 1f;
        slider.value = current;
        _sensitivityValueText.text = current.ToString("F1");

        slider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        if (_fpsController == null)
            _fpsController = FindObjectOfType<FirstPersonController>();
        if (_fpsController != null)
            _fpsController.RotationSpeed = value;

        if (_sensitivityValueText != null)
            _sensitivityValueText.text = value.ToString("F1");
    }

    private void CreateButton(string name, string label, float yOffset, UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(_panel.transform, false);

        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
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
