using System;
using UnityEngine;

/// <summary>
/// Add this component to any interactable object that has a Trigger collider.
/// When the Player enters the trigger, all child Renderers get an outline.
/// No need to modify existing interaction scripts.
/// </summary>
public class InteractionHighlight : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.white;
    [Range(0f, 0.1f)]
    public float outlineWidth = 0.03f;

    private Renderer[] _renderers;
    private Material[][] _originalMats;
    private Material[][] _outlinedMats;
    private Material _outlineMat;
    private bool _highlighted;

    private static readonly int ColorProp = Shader.PropertyToID("_OutlineColor");
    private static readonly int WidthProp = Shader.PropertyToID("_OutlineWidth");

    private void Start()
    {
        var shader = Shader.Find("Custom/Outline");
        if (shader == null)
        {
            Debug.LogError("InteractionHighlight: Custom/Outline shader not found.");
            enabled = false;
            return;
        }

        _outlineMat = new Material(shader);
        _outlineMat.SetColor(ColorProp, outlineColor);
        _outlineMat.SetFloat(WidthProp, outlineWidth);

        CacheRenderers();
    }

    private void CacheRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalMats = new Material[_renderers.Length][];
        _outlinedMats = new Material[_renderers.Length][];

        for (int i = 0; i < _renderers.Length; i++)
        {
            var shared = _renderers[i].sharedMaterials;
            _originalMats[i] = shared;

            _outlinedMats[i] = new Material[shared.Length + 1];
            Array.Copy(shared, _outlinedMats[i], shared.Length);
            _outlinedMats[i][shared.Length] = _outlineMat;
        }
    }

    private void SetHighlight(bool on)
    {
        if (_highlighted == on) return;
        _highlighted = on;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;
            _renderers[i].materials = on ? _outlinedMats[i] : _originalMats[i];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            SetHighlight(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            SetHighlight(false);
    }

    private void OnDisable()
    {
        if (_highlighted) SetHighlight(false);
    }
}
