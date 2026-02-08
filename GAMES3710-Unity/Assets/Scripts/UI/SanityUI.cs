using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SanityUI : MonoBehaviour
{
    [Header("References")]
    public Slider sanitySlider;
    public TMP_Text pillCountText;

    private void Start()
    {
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.OnSanityChanged += UpdateSanityDisplay;
            SanityManager.Instance.OnPillCountChanged += UpdatePillCount;
            
            UpdateSanityDisplay();
            UpdatePillCount();
        }
    }

    private void OnDestroy()
    {
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.OnSanityChanged -= UpdateSanityDisplay;
            SanityManager.Instance.OnPillCountChanged -= UpdatePillCount;
        }
    }

    private void UpdateSanityDisplay()
    {
        if (sanitySlider != null && SanityManager.Instance != null)
        {
            sanitySlider.value = SanityManager.Instance.GetSanityNormalized();
        }
    }

    private void UpdatePillCount()
    {
        if (pillCountText != null && SanityManager.Instance != null)
        {
            pillCountText.text = SanityManager.Instance.pillCount.ToString();
        }
    }
}
