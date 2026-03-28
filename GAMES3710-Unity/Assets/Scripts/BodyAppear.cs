using UnityEngine;
using System.Collections;

public class CorpseDistanceTrigger : MonoBehaviour
{
    public Transform player;
    public GameObject corpse;
    public AudioClip scareSound;

    public float triggerDistance = 5f;
    public float visibleTime = 1f;
    public float volume = 1f;

    private bool hasTriggered = false;

    void Start()
    {
        Debug.Log("Script started");

        if (corpse != null)
        {
            corpse.SetActive(false);
            Debug.Log("Corpse hidden at start");
        }
        else
        {
            Debug.LogError("Corpse is NOT assigned");
        }

        if (player == null)
        {
            Debug.LogError("Player is NOT assigned");
        }
    }

    void Update()
    {
        if (hasTriggered) return;
        if (player == null || corpse == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        Debug.Log("Distance = " + distance);

        if (distance <= triggerDistance)
        {
            Debug.Log("TRIGGERED");
            hasTriggered = true;
            StartCoroutine(ShowAndHide());
        }
    }

    IEnumerator ShowAndHide()
    {
        Debug.Log("Show corpse now");
        corpse.SetActive(true);

        if (scareSound != null)
        {
            AudioSource.PlayClipAtPoint(scareSound, transform.position, volume);
        }

        yield return new WaitForSeconds(visibleTime);

        corpse.SetActive(false);
        Debug.Log("Hide corpse now");
    }
}