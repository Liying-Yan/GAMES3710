using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class FootStepSFX : MonoBehaviour
{
    public CharacterController controller;
    public AudioClip walkingLoopClip;

    public float moveThreshold = 0.02f;
    public float pitchWalk = 1.0f;
    public float pitchRun = 1.12f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (controller == null)
            controller = GetComponent<CharacterController>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 先强制2D排除距离问题
    }

    void Update()
    {
        // P 键强制测试，确认能出声
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (walkingLoopClip != null)
            {
                audioSource.Stop();
                audioSource.clip = walkingLoopClip;
                audioSource.pitch = 1f;
                audioSource.Play();
                Debug.Log("Loop test started");
            }
            else
            {
                Debug.Log("walkingLoopClip is NULL");
            }
        }

        if (controller == null) return;
        if (walkingLoopClip == null) return;

        Vector3 v = controller.velocity;
        float speed = new Vector3(v.x, 0f, v.z).magnitude;

        bool grounded = controller.isGrounded;
        bool moving = speed > moveThreshold;

        bool sprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        if (grounded && moving)
        {
            if (audioSource.clip != walkingLoopClip)
                audioSource.clip = walkingLoopClip;

            audioSource.pitch = sprinting ? pitchRun : pitchWalk;

            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}