using UnityEngine;

public class RemoteSoundTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform playerCapsule; // 拖你的 PlayerCapsule
    public Transform soundSourcePoint;
    public AudioClip sound;

    [Header("Settings")]
    public float triggerDistance = 3f;
    public bool playOnce = true;
    public float volume = 1f;
    public float minDistance = 3f;
    public float maxDistance = 50f;

    private bool hasPlayed = false;

    private void Update()
    {
        if (playerCapsule == null || sound == null || soundSourcePoint == null) return;
        if (playOnce && hasPlayed) return;

        float distance = Vector3.Distance(playerCapsule.position, transform.position);

        if (distance <= triggerDistance)
        {
            hasPlayed = true;
            PlayRemoteSound();
        }
    }

    private void PlayRemoteSound()
    {
        GameObject tempSound = new GameObject("TempRemoteSound");
        tempSound.transform.position = soundSourcePoint.position;

        AudioSource source = tempSound.AddComponent<AudioSource>();
        source.clip = sound;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.Play();

        Destroy(tempSound, sound.length + 0.2f);
    }
}