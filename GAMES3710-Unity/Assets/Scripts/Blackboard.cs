using UnityEngine;

public class BlackboardHintTrigger : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Hint Object")]
    public GameObject hidePlane;

    [Header("Distance Settings")]
    public float showDistance = 3f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip appearSound;

    private bool isShowing = false;
    private bool hasTriggered = false; // 新增

    void Start()
    {
        if (hidePlane != null)
        {
            hidePlane.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || hidePlane == null)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);
        bool shouldShow = distance <= showDistance;

        if (shouldShow && !isShowing && !hasTriggered)
        {
            hidePlane.SetActive(true);
            isShowing = true;
            hasTriggered = true;

            if (audioSource != null && appearSound != null)
            {
                audioSource.PlayOneShot(appearSound);
            }
        }
    }
}