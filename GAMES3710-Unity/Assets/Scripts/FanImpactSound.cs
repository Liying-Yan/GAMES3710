using System.Collections;
using UnityEngine;

public class FanImpactSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip impactClip;
    public float delayBeforeSound = 0.2f;

    public void PlayImpactSound()
    {
        StartCoroutine(PlaySoundCoroutine());
    }

    private IEnumerator PlaySoundCoroutine()
    {
        yield return new WaitForSeconds(delayBeforeSound);

        if (audioSource != null && impactClip != null)
        {
            audioSource.PlayOneShot(impactClip);
        }
    }
}