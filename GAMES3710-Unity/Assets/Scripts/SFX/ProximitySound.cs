using UnityEngine;

public class ProximitySound : MonoBehaviour
{
    public AudioClip sound;

    public float triggerDistance = 3f;
    public bool playOnce = true;

    private Transform player;
    private bool hasPlayed = false;

    private void Start()
    {
        player = Camera.main.transform; // 用摄像机当玩家
    }

    private void Update()
    {
        if (player == null || sound == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            if (!playOnce || !hasPlayed)
            {
                hasPlayed = true;
                PlaySound3D();
            }
        }
    }

    private void PlaySound3D()
    {
        GameObject obj = new GameObject("ProximitySound");
        obj.transform.position = transform.position;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = sound;
        source.spatialBlend = 1f; // 3D声音
        source.minDistance = 1f;
        source.maxDistance = 10f;

        source.Play();

        Destroy(obj, sound.length + 0.1f);
    }
}