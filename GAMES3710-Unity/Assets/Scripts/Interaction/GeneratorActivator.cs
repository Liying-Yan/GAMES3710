using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GeneratorActivator : Interactable
{
    [Header("Key Requirements")]
    public ItemPickup[] requiredKeys;

    [Header("Dependencies")]
    public LockedDoor[] requiredMechanisms;

    [Header("Generator Animation")]
    public DoorAnimationType animationType = DoorAnimationType.None;
    public float swingAngle = -90f;
    public float swingDuration = 1f;
    public Transform generatorTransform;
    public Animator mechanismAnimator;
    public string animatorTriggerName = "Activate";

    [Header("Alarm Lights")]
    public AlarmLightController[] alarmLights;

    [Header("Prompt")]
    public string lockedPrompt = "Requires a key";
    public string dependencyPrompt = "Requires another mechanism first";
    public string activatedPrompt = "Generator activated";

    [Header("Escape Hint Subtitle")]
    [TextArea]
    public string exitHintSubtitle = "I need to find the exit... fast.";
    public float exitHintDuration = 3f;

    [Header("SFX")]
    public AudioClip lockedSfx;

    [Header("State")]
    public bool isActivated;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (PlayerInRange && !isActivated && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (isActivated) return;

        if (!CheckDependencies())
        {
            if (audioSource != null && lockedSfx != null)
            {
                audioSource.PlayOneShot(lockedSfx);
            }

            if (PromptUI.Instance != null)
            {
                PromptUI.Instance.Show(dependencyPrompt);
            }

            return;
        }

        if (requiredKeys != null && requiredKeys.Length > 0)
        {
            foreach (var key in requiredKeys)
            {
                if (key == null) continue;

                if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasKey(key))
                {
                    if (audioSource != null && lockedSfx != null)
                    {
                        audioSource.PlayOneShot(lockedSfx);
                    }

                    if (PromptUI.Instance != null)
                    {
                        PromptUI.Instance.Show(lockedPrompt);
                    }

                    return;
                }
            }

            foreach (var key in requiredKeys)
            {
                if (key != null && key.consumable && PlayerInventory.Instance != null)
                {
                    PlayerInventory.Instance.RemoveKey(key);
                }
            }
        }

        ActivateGenerator();
    }

    private bool CheckDependencies()
    {
        if (requiredMechanisms == null) return true;

        foreach (var mechanism in requiredMechanisms)
        {
            if (mechanism != null && !mechanism.isOpen)
            {
                return false;
            }
        }

        return true;
    }

    private void ActivateGenerator()
    {
        isActivated = true;
        HidePrompt();

        if (PromptUI.Instance != null)
        {
            PromptUI.Instance.Show(activatedPrompt);
        }

        TriggerAlarmLights();

        if (GuideSubtitleUI.Instance != null)
        {
            GuideSubtitleUI.Instance.Show(exitHintSubtitle, exitHintDuration);
        }

        switch (animationType)
        {
            case DoorAnimationType.None:
                break;

            case DoorAnimationType.SwingDoor:
                if (generatorTransform != null)
                {
                    StartCoroutine(SwingGeneratorCoroutine());
                }
                break;

            case DoorAnimationType.AnimatorTrigger:
                if (mechanismAnimator != null)
                {
                    mechanismAnimator.SetTrigger(animatorTriggerName);
                }
                break;
        }
    }

    private void TriggerAlarmLights()
    {
        if (alarmLights == null || alarmLights.Length == 0) return;

        foreach (var alarm in alarmLights)
        {
            if (alarm != null)
            {
                alarm.StartAlarm();
            }
        }
    }

    private IEnumerator SwingGeneratorCoroutine()
    {
        var colliders = generatorTransform.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                col.enabled = false;
            }
        }

        Quaternion startRot = generatorTransform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, swingAngle, 0f);
        float elapsed = 0f;

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            generatorTransform.localRotation = Quaternion.Lerp(startRot, endRot, elapsed / swingDuration);
            yield return null;
        }

        generatorTransform.localRotation = endRot;
    }

    protected override void OnPlayerEnter()
    {
        if (!isActivated)
        {
            ShowPrompt("Press E to activate generator");
        }
    }

    protected override void OnPlayerExit()
    {
        HidePrompt();
    }
}