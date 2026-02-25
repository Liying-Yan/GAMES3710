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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_playerInput == null)
            _playerInput = FindObjectOfType<StarterAssetsInputs>();
        if (_playerInput != null)
            _playerInput.cursorInputForLook = false;

        IsPaused = true;
    }

    public void Hide()
    {
        _panel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_playerInput == null)
            _playerInput = FindObjectOfType<StarterAssetsInputs>();
        if (_playerInput != null)
            _playerInput.cursorInputForLook = true;

        IsPaused = false;
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        IsPaused = false;
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

        // Buttons
        CreateButton("ResumeBtn", "Resume", 35f, Hide);
        CreateButton("RestartBtn", "Restart", -35f, OnRestart);
        CreateButton("QuitBtn", "Quit Game", -105f, OnQuit);
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
