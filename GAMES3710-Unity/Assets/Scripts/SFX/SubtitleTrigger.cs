using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SubtitleTrigger : MonoBehaviour
{
    [Header("Subtitle")]
    [TextArea] public string subtitle;
    public float duration = 3f;

    [Header("Settings")]
    public bool playOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (playOnlyOnce && hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (string.IsNullOrEmpty(subtitle) || SubtitleUI.Instance == null) return;

        hasTriggered = true;
        SubtitleUI.Instance.Show(subtitle, duration);
    }
}
