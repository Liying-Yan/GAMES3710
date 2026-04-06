using UnityEngine;

public class ProximityRemoteSound : MonoBehaviour
{
    public AudioClip sound;

    public Transform soundSourcePoint; // 声音从这里发出

    public float triggerDistance = 3f;
    public bool playOnce = true;

    private Transform player;
    private bool hasPlayed = false;

    private void Start()
    {
        player = Camera.main.transform;
    }

    private void Update()
    {
        if (player == null || sound == null || soundSourcePoint == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            if (!playOnce || !hasPlayed)
            {
                hasPlayed = true;
                PlaySoundAtPoint();
            }
        }
    }

    private void PlaySoundAtPoint()
    {
        GameObject obj = new GameObject("RemoteSound");
        obj.transform.position = soundSourcePoint.position;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = sound;
        source.spatialBlend = 1f; // 3D声音
        source.minDistance = 1f;
        source.maxDistance = 15f;

        source.Play();

        Destroy(obj, sound.length + 0.1f);
    }
}