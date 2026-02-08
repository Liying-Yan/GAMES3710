using UnityEngine;
using System;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    [Header("Sanity Settings")]
    public float maxSanity = 100f;
    public float currentSanity = 100f;
    public float decayRate = 1f;

    [Header("Pill Settings")]
    public int pillCount;
    public float pillRestoreAmount = 30f;

    [Header("Low Sanity Threshold")]
    public float lowSanityThreshold = 30f;

    public event Action OnSanityChanged;
    public event Action OnPillCountChanged;
    public event Action OnSanityDepleted;

    private float _decayMultiplier = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        DecaySanity();
        HandlePillInput();
    }

    private void DecaySanity()
    {
        if (currentSanity <= 0f) return;

        currentSanity -= decayRate * _decayMultiplier * Time.deltaTime;
        currentSanity = Mathf.Max(0f, currentSanity);
        
        OnSanityChanged?.Invoke();

        if (currentSanity <= 0f)
        {
            OnSanityDepleted?.Invoke();
        }
    }

    private void HandlePillInput()
    {
        if (UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
        {
            UsePill();
        }
    }

    public void AddPill()
    {
        pillCount++;
        OnPillCountChanged?.Invoke();
    }

    public void UsePill()
    {
        if (pillCount <= 0) return;

        pillCount--;
        currentSanity = Mathf.Min(maxSanity, currentSanity + pillRestoreAmount);
        
        OnPillCountChanged?.Invoke();
        OnSanityChanged?.Invoke();
    }

    public float GetSanityNormalized()
    {
        return currentSanity / maxSanity;
    }

    public bool IsLowSanity()
    {
        return currentSanity <= lowSanityThreshold;
    }

    public void SetDecayMultiplier(float multiplier)
    {
        _decayMultiplier = multiplier;
    }
}
