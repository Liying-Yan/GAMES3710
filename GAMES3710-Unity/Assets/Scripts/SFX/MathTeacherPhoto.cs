using UnityEngine;

public class PhotoVoiceTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public AudioSource audioSource;

    [Header("Settings")]
    public float triggerDistance = 3f;
    public bool playOnlyOnce = true;

    private bool hasPlayed = false;

    void Update()
    {
        if (player == null || audioSource == null) return;
        if (playOnlyOnce && hasPlayed) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                hasPlayed = true;
            }
        }
    }
}