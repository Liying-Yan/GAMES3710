using UnityEngine;

public class VentOpenSFX : MonoBehaviour
{
    public AudioClip openSfx;
    public float minDistance = 1f;
    public float maxDistance = 15f;

    private LockedDoor lockedDoor;
    private bool hasPlayed = false;

    private void Start()
    {
        lockedDoor = GetComponent<LockedDoor>();
    }

    private void Update()
    {
        if (!hasPlayed && lockedDoor != null && lockedDoor.isOpen)
        {
            hasPlayed = true;
            PlaySound3D();
        }
    }

    private void PlaySound3D()
    {
        if (openSfx == null) return;

        GameObject tempSound = new GameObject("TempVentOpenSFX");
        tempSound.transform.position = transform.position;

        AudioSource source = tempSound.AddComponent<AudioSource>();
        source.clip = openSfx;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.Play();

        Destroy(tempSound, openSfx.length + 0.1f);
    }
}