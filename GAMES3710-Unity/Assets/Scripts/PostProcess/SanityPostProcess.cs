using UnityEngine;

public class SanityPostProcess : MonoBehaviour
{
    [Header("References")]
    public FullscreenEffect noiseEffect;
    public Material noiseMaterial;

    [Header("Settings")]
    public float minNoiseIntensity = 0f;
    public float maxNoiseIntensity = 0.5f;

    private static readonly int NoiseIntensityId = Shader.PropertyToID("_NoiseIntensity");

    private void Update()
    {
        if (SanityManager.Instance == null || noiseEffect == null || noiseMaterial == null)
            return;

        bool isLowSanity = SanityManager.Instance.IsLowSanity();
        noiseEffect.enabled = isLowSanity;

        if (isLowSanity)
        {
            float sanityNormalized = SanityManager.Instance.GetSanityNormalized();
            float threshold = SanityManager.Instance.lowSanityThreshold / SanityManager.Instance.maxSanity;
            
            float t = 1f - (sanityNormalized / threshold);
            float intensity = Mathf.Lerp(minNoiseIntensity, maxNoiseIntensity, t);
            
            noiseMaterial.SetFloat(NoiseIntensityId, intensity);
        }
    }
}
