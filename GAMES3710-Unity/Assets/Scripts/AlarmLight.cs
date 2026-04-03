using UnityEngine;

public class AlarmLightController : MonoBehaviour
{
    [Header("References")]
    public Renderer glowRenderer;
    public Light pointLight;

    [Header("Emission")]
    public Color emissionOnColor = new Color(6f, 0f, 0f, 1f);
    public Color emissionOffColor = new Color(0f, 0f, 0f, 1f);

    [Header("Light")]
    public float lightOnIntensity = 5f;
    public float lightOffIntensity = 0f;

    [Header("Blink Timing")]
    public float blinkInterval = 0.35f;

    [Header("Alarm Audio")]
    public AudioSource alarmAudioSource;
    public AudioClip alarmClip;
    public bool loopAlarm = true;

    [Header("State")]
    public bool generatorStarted = false;

    [Header("Testing")]
    public bool testMode = true;

    private Material glowMaterial;
    private float timer = 0f;
    private bool isOn = false;

    private void Start()
    {
        if (glowRenderer != null)
        {
            glowMaterial = glowRenderer.material;
        }

        SetLightState(false);

        if (alarmAudioSource != null)
        {
            alarmAudioSource.playOnAwake = false;
            alarmAudioSource.loop = loopAlarm;
        }

        if (testMode)
        {
            StartAlarm();
        }
    }

    private void Update()
    {
        if (!generatorStarted)
            return;

        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            timer = 0f;
            isOn = !isOn;
            SetLightState(isOn);
        }
    }

    private void SetLightState(bool on)
    {
        if (glowMaterial != null)
        {
            glowMaterial.SetColor("_EmissionColor", on ? emissionOnColor : emissionOffColor);
        }

        if (pointLight != null)
        {
            pointLight.intensity = on ? lightOnIntensity : lightOffIntensity;
        }
    }

    public void StartAlarm()
    {
        generatorStarted = true;
        timer = 0f;
        isOn = false;
        SetLightState(false);

        if (alarmAudioSource != null && alarmClip != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.clip = alarmClip;
            alarmAudioSource.Play();
        }
    }

    public void StopAlarm()
    {
        generatorStarted = false;
        timer = 0f;
        isOn = false;
        SetLightState(false);

        if (alarmAudioSource != null && alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Stop();
        }
    }
}