using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorSFX : MonoBehaviour
{
    public AudioClip openSound;
    public AudioClip closeSound;

    private AudioSource audioSource;
    private bool isOpen = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ToggleDoor()
    {
        if (!isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    void OpenDoor()
    {
        isOpen = true;

        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        Debug.Log("Door opened");
    }

    void CloseDoor()
    {
        isOpen = false;

        if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        Debug.Log("Door closed");
    }
}