using UnityEngine;
using UnityEngine.Audio;

public class HidePropAudioController : MonoBehaviour
{
    public AudioMixer mixer;

    public string parameterName = "PropVol";

    public float hiddenVolume = -80f;
    public float normalVolume = 0f;

    private bool lastHidingState = false;

    void Update()
    {
        if (PlayerHideState.Instance == null || mixer == null)
            return;

        bool isHiding = PlayerHideState.Instance.IsHiding;

        if (isHiding != lastHidingState)
        {
            if (isHiding)
            {
                mixer.SetFloat(parameterName, hiddenVolume);
                Debug.Log("Prop muted");
            }
            else
            {
                mixer.SetFloat(parameterName, normalVolume);
                Debug.Log("Prop restored");
            }

            lastHidingState = isHiding;
        }
    }
}