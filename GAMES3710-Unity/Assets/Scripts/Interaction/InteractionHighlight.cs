using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this component to any interactable object that has a Trigger collider.
/// When the Player enters the trigger, all child Renderers get an outline.
/// Smooth normals are baked into UV3 at startup to eliminate hard-edge seams.
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

        BakeSmoothNormals();
        CacheRenderers();
    }

    // ──────────────────────────────────────────────
    // Smooth normal baking (fixes hard-edge outline seams)
    // ──────────────────────────────────────────────
    private void BakeSmoothNormals()
    {
        foreach (var mf in GetComponentsInChildren<MeshFilter>())
        {
            // .mesh returns an instance copy – safe to modify without affecting other objects
            var mesh = mf.mesh;
            if (mesh == null || !mesh.isReadable) continue;
            BakeSmooth(mesh);
        }

        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var src = smr.sharedMesh;
            if (src == null || !src.isReadable) continue;
            var copy = Instantiate(src);
            BakeSmooth(copy);
            smr.sharedMesh = copy;
        }
    }

    private static void BakeSmooth(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        if (normals == null || normals.Length == 0) return;

        // Accumulate normals per unique position (hard-edge vertices share position but differ in normal)
        var accumulated = new Dictionary<string, Vector3>();
        var keys = new string[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            var v = vertices[i];
            var key = $"{v.x:F5},{v.y:F5},{v.z:F5}";
            keys[i] = key;

            if (accumulated.TryGetValue(key, out var sum))
                accumulated[key] = sum + normals[i];
            else
                accumulated[key] = normals[i];
        }

        // Write averaged smooth normals to UV3
        var smooth = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            smooth[i] = accumulated[keys[i]].normalized;

        mesh.SetUVs(3, smooth);
    }

    // ──────────────────────────────────────────────
    // Material caching
    // ──────────────────────────────────────────────
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

    // ──────────────────────────────────────────────
    // Toggle
    // ──────────────────────────────────────────────
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
