using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFootsteps : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepClips;

    public float stepInterval = 0.5f;   // 每一步时间
    public float minMoveSpeed = 0.1f;

    float stepTimer;
    int lastIndex = -1;

    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        stepTimer = stepInterval;
    }

    void Update()
    {
        if (agent.velocity.magnitude < minMoveSpeed)
            return;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0)
            return;

        int index = Random.Range(0, footstepClips.Length);

        if (index == lastIndex)
            index = (index + 1) % footstepClips.Length;

        audioSource.pitch = Random.Range(0.95f, 1.05f);

        audioSource.PlayOneShot(footstepClips[index]);

        lastIndex = index;
    }
}