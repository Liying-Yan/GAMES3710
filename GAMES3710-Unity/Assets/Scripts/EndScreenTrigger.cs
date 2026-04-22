using UnityEngine;

public class EndScreenTrigger : MonoBehaviour
{
    [SerializeField] private bool triggerOnce = true;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;

        if (EndScreenUI.Instance != null)
            EndScreenUI.Instance.Show();
    }
}
