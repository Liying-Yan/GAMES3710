using UnityEngine;

public class VentOpenSFXWatcher : MonoBehaviour
{
    public LockedDoor targetDoor;
    public AudioClip openSfx;

    public float minDistance = 1f;
    public float maxDistance = 15f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float volume = 1f;

    private bool hasPlayed = false;

    private void Update()
    {
        if (hasPlayed) return;
        if (targetDoor == null) return;

        if (targetDoor.isOpen)
        {
            hasPlayed = true;
            PlaySound3DAtDoor();
        }
    }

    private void PlaySound3DAtDoor()
    {
        if (openSfx == null) return;

        Vector3 soundPosition = targetDoor.transform.position;

        GameObject tempSound = new GameObject("TempVentOpenSFX");
        tempSound.transform.position = soundPosition;

        AudioSource source = tempSound.AddComponent<AudioSource>();
        source.clip = openSfx;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = rolloffMode;
        source.Play();

        Destroy(tempSound, openSfx.length + 0.1f);
    }
}