using UnityEngine;
using System.Collections;

public class RemoteSoundTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform playerCapsule;
    public Transform soundSourcePoint;
    public AudioClip sound;

    [Header("Settings")]
    public float triggerDistance = 3f;
    public bool playOnce = true;

    [Header("Audio Settings")]
    public float volume = 1f;
    public float minDistance = 2f;
    public float maxDistance = 20f;

    [Header("Fade Settings")]
    public float fadeInTime = 1f;
    public float fadeOutTime = 2f;
    public float destroyDelayAfterFade = 0.3f;

    private bool hasPlayed = false;

    private void Update()
    {
        if (playerCapsule == null || soundSourcePoint == null || sound == null) return;
        if (playOnce && hasPlayed) return;

        float distance = Vector3.Distance(playerCapsule.position, transform.position);

        if (distance <= triggerDistance)
        {
            hasPlayed = true;
            StartCoroutine(PlayWithFade());
        }
    }

    private IEnumerator PlayWithFade()
    {
        GameObject obj = new GameObject("TempRemoteSound");
        obj.transform.position = soundSourcePoint.position;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = sound;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.volume = 0f;

        source.Play();

        float t = 0f;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeInTime);
            k = Mathf.SmoothStep(0f, 1f, k);
            source.volume = Mathf.Lerp(0f, volume, k);
            yield return null;
        }

        source.volume = volume;

        float middleTime = sound.length - fadeInTime - fadeOutTime;
        middleTime = Mathf.Max(0f, middleTime);

        if (middleTime > 0f)
        {
            yield return new WaitForSeconds(middleTime);
        }

        t = 0f;

        float startVolume = source.volume;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeOutTime);
            k = Mathf.SmoothStep(0f, 1f, k);
            source.volume = Mathf.Lerp(startVolume, 0f, k);
            yield return null;
        }

        source.volume = 0f;

        yield return new WaitForSeconds(destroyDelayAfterFade);

        Destroy(obj);
    }
}