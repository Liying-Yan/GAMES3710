using System.Collections;
using UnityEngine;

public class FanAnimationTrigger : MonoBehaviour
{
    public Animator fanAnimator;
    public AudioSource fanAudioSource;
    public AudioClip impactClip;

    public float delayBeforeFall = 0.8f;
    public float delayBeforeImpactSound = 1.2f;

    private bool hasTriggered = false;

    private void Start()
    {
        fanAnimator.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.GetComponent<CharacterController>() != null)
        {
            hasTriggered = true;
            StartCoroutine(TriggerFan());
        }
    }

    private IEnumerator TriggerFan()
    {
        yield return new WaitForSeconds(delayBeforeFall);

        fanAnimator.enabled = true;
        fanAnimator.Play("Fan_Fall", 0, 0f);

        yield return new WaitForSeconds(delayBeforeImpactSound - delayBeforeFall);

        fanAudioSource.PlayOneShot(impactClip);
    }
}