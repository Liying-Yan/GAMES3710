using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }
    public static bool IsGameOver { get; private set; }

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
        IsGameOver = false;

        if (SanityManager.Instance != null)
            SanityManager.Instance.OnSanityDepleted += Show;
    }

    private void OnDestroy()
    {
        if (SanityManager.Instance != null)
            SanityManager.Instance.OnSanityDepleted -= Show;

        if (Instance == this)
        {
            Instance = null;
            IsGameOver = false;
        }
    }

    public void Show()
    {
        if (IsGameOver) return;

        _panel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_playerInput == null)
            _playerInput = FindObjectOfType<StarterAssetsInputs>();
        if (_playerInput != null)
            _playerInput.cursorInputForLook = false;

        IsGameOver = true;
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
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
        // Panel - fullscreen dark overlay
        _panel = new GameObject("GameOverPanel");
        _panel.transform.SetParent(transform, false);

        var panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var panelImage = _panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);

        // Title - "GAME OVER"
        var titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(_panel.transform, false);

        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.65f);
        titleRect.anchorMax = new Vector2(0.5f, 0.65f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(600f, 100f);

        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 72f;
        titleText.color = Color.red;
        titleText.alignment = TextAlignmentOptions.Center;

        // Buttons
        CreateButton("RestartBtn", "Restart", 0f, OnRestart);
        CreateButton("QuitBtn", "Quit Game", -70f, OnQuit);
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
