using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class HidingSpot : MonoBehaviour
{
    [Header("Prompt Settings")]
    public string enterPrompt = "Press E to hide";
    public string exitPrompt = "Press E to exit";

    [Header("Breath Audio")]
    public AudioSource breathAudio;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    private bool _playerInRange;
    private bool _isPlayerHiding;

    private void Start()
    {
        if (breathAudio != null)
        {
            breathAudio.playOnAwake = false;
            breathAudio.loop = true;
        }

        if (mixer != null)
        {
            mixer.SetFloat("PropVol", 0f);
        }
    }

    private void Update()
    {
        if (_playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleHide();
        }
    }

    private void ToggleHide()
    {
        _isPlayerHiding = !_isPlayerHiding;

        if (PlayerHideState.Instance != null)
        {
            PlayerHideState.Instance.SetHiding(_isPlayerHiding);
        }

        if (_isPlayerHiding)
        {
            if (breathAudio != null)
            {
                breathAudio.Play();
            }

            if (mixer != null)
            {
                mixer.SetFloat("PropVol", -80f);
            }
        }
        else
        {
            if (breathAudio != null)
            {
                breathAudio.Stop();
            }

            if (mixer != null)
            {
                mixer.SetFloat("PropVol", 0f);
            }
        }

        UpdatePrompt();
    }

    private void UpdatePrompt()
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.Show(_isPlayerHiding ? exitPrompt : enterPrompt);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;

            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Show(enterPrompt);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;

            if (!_isPlayerHiding && InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Hide();
            }
        }
    }
}