using UnityEngine;

public class DeskVoiceTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public AudioSource audioSource;

    [Header("Settings")]
    public float triggerDistance = 3f;

    private bool hasPlayed = false;

    void Update()
    {
        if (hasPlayed) return;
        if (player == null || audioSource == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            hasPlayed = true;
            audioSource.Play();
        }
    }
}