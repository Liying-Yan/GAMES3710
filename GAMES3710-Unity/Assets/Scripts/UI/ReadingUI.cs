using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class ReadingUI : MonoBehaviour
{
    public static ReadingUI Instance { get; private set; }
    public static bool IsReading { get; private set; }

    private GameObject _readingPanel;
    private TMP_Text _contentText;
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
        // Ensure this GameObject's RectTransform stretches to fill the Canvas
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        CreateUI();
        _readingPanel.SetActive(false);
    }

    private void Update()
    {
        if (IsReading && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
        }
    }

    public void Show(string content)
    {
        _contentText.text = content;
        _readingPanel.SetActive(true);

        // Disable player look input
        if (_playerInput == null)
            _playerInput = FindObjectOfType<StarterAssetsInputs>();
        if (_playerInput != null)
            _playerInput.cursorInputForLook = false;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsReading = true;
    }

    public void Hide()
    {
        _readingPanel.SetActive(false);

        // Re-enable player look input
        if (_playerInput != null)
            _playerInput.cursorInputForLook = true;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsReading = false;
    }

    private void CreateUI()
    {
        // Panel - fullscreen semi-transparent black background
        _readingPanel = new GameObject("ReadingPanel");
        _readingPanel.transform.SetParent(transform, false);

        var panelRect = _readingPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = new Vector2(-10f, -10f);
        panelRect.offsetMax = new Vector2(10f, 10f);

        var panelImage = _readingPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        // Content text - centered with padding
        var contentObj = new GameObject("ContentText");
        contentObj.transform.SetParent(_readingPanel.transform, false);

        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.15f);
        contentRect.anchorMax = new Vector2(0.9f, 0.9f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        _contentText = contentObj.AddComponent<TextMeshProUGUI>();
        _contentText.fontSize = 36f;
        _contentText.color = Color.white;
        _contentText.alignment = TextAlignmentOptions.Center;
        _contentText.enableWordWrapping = true;

        // Hint text - bottom center
        var hintObj = new GameObject("HintText");
        hintObj.transform.SetParent(_readingPanel.transform, false);

        var hintRect = hintObj.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0f, 30f);
        hintRect.sizeDelta = new Vector2(400f, 30f);

        var hintText = hintObj.AddComponent<TextMeshProUGUI>();
        hintText.text = "Press ESC to exit";
        hintText.fontSize = 24f;
        hintText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        hintText.alignment = TextAlignmentOptions.Center;
    }
}
