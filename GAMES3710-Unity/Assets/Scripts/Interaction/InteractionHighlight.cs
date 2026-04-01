using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Add this component to any interactable object that has a Trigger collider.
/// When the Player enters the trigger, all child Renderers get an outline.
/// Smooth normals are baked into UV3 at startup to eliminate hard-edge seams.
///
/// Renders outlines via a self-injected ScriptableRenderPass (no Renderer Feature
/// setup required). Stencil fill + outline draw happen in the same pass to avoid
/// URP's stencil-clearing pitfall.
/// </summary>
public class InteractionHighlight : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.white;
    [Range(0f, 0.02f)]
    public float outlineWidth = 0.005f;

    private Renderer[] _renderers;
    private Material _outlineMat;
    private bool _highlighted;
    private readonly List<Mesh> _bakedMeshes = new();
    private OutlinePass _outlinePass;

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
        _renderers = GetComponentsInChildren<Renderer>();
        _outlinePass = new OutlinePass(_renderers, _outlineMat);
    }

    // ──────────────────────────────────────────────
    // Smooth normal baking (fixes hard-edge outline seams)
    // ──────────────────────────────────────────────
    private void BakeSmoothNormals()
    {
        foreach (var mf in GetComponentsInChildren<MeshFilter>())
        {
            var mesh = mf.mesh;
            if (mesh == null) continue;
            if (!mesh.isReadable)
            {
                Debug.LogWarning($"InteractionHighlight: Mesh '{mesh.name}' on '{mf.gameObject.name}' is not readable. Outline may have hard-edge seams.", mf);
                continue;
            }
            BakeSmooth(mesh);
            _bakedMeshes.Add(mesh);
        }

        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var src = smr.sharedMesh;
            if (src == null) continue;
            if (!src.isReadable)
            {
                Debug.LogWarning($"InteractionHighlight: Mesh '{src.name}' on '{smr.gameObject.name}' is not readable. Outline may have hard-edge seams.", smr);
                continue;
            }
            var copy = Instantiate(src);
            BakeSmooth(copy);
            smr.sharedMesh = copy;
            _bakedMeshes.Add(copy);
        }
    }

    private static void BakeSmooth(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        if (normals == null || normals.Length == 0) return;

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

        var smooth = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            smooth[i] = accumulated[keys[i]].normalized;

        mesh.SetUVs(3, smooth);
    }

    // ──────────────────────────────────────────────
    // Highlight toggle
    // ──────────────────────────────────────────────
    private void SetHighlight(bool on)
    {
        if (_highlighted == on) return;
        _highlighted = on;

        if (on)
            RenderPipelineManager.beginCameraRendering += InjectPass;
        else
            RenderPipelineManager.beginCameraRendering -= InjectPass;
    }

    private void InjectPass(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView) return;
        cam.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_outlinePass);
    }

    private void Update()
    {
        if (_outlineMat == null) return;
        _outlineMat.SetColor(ColorProp, outlineColor);
        _outlineMat.SetFloat(WidthProp, outlineWidth);
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

    private void OnDestroy()
    {
        if (_highlighted)
            RenderPipelineManager.beginCameraRendering -= InjectPass;
        if (_outlineMat != null) Destroy(_outlineMat);
        foreach (var mesh in _bakedMeshes)
            if (mesh != null) Destroy(mesh);
    }

    private void OnDisable()
    {
        if (_highlighted) SetHighlight(false);
    }

    // ──────────────────────────────────────────────
    // Outline render pass
    // ──────────────────────────────────────────────
    private class OutlinePass : ScriptableRenderPass
    {
        private readonly Renderer[] _renderers;
        private readonly Material _material;

        public OutlinePass(Renderer[] renderers, Material material)
        {
            _renderers = renderers;
            _material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        // ── Legacy path (pre-Unity 6.0.4) ────────────
#if !UNITY_6000_4_OR_NEWER
#pragma warning disable CS0618
#pragma warning disable CS0672
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("OutlineEffect");
            DrawOutline(cmd);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
#endif

        // ── Render Graph path ────────────────────────
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddUnsafePass<OutlinePassData>("OutlineEffect", out var passData))
            {
                passData.renderers = _renderers;
                passData.material = _material;
                passData.colorTarget = resourceData.activeColorTexture;
                passData.depthTarget = resourceData.activeDepthTexture;

                builder.UseTexture(passData.colorTarget, AccessFlags.Write);
                builder.UseTexture(passData.depthTarget, AccessFlags.ReadWrite);

                builder.SetRenderFunc(static (OutlinePassData data, UnsafeGraphContext context) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                    cmd.SetRenderTarget(data.colorTarget, data.depthTarget);

                    foreach (var r in data.renderers)
                    {
                        if (r == null) continue;
                        int count = GetSubmeshCount(r);
                        for (int i = 0; i < count; i++)
                            cmd.DrawRenderer(r, data.material, i, 0);
                    }
                });
            }
        }

        private void DrawOutline(CommandBuffer cmd)
        {
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                int count = GetSubmeshCount(r);
                for (int i = 0; i < count; i++)
                    cmd.DrawRenderer(r, _material, i, 0);
            }
        }

        private static int GetSubmeshCount(Renderer r)
        {
            if (r is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                return smr.sharedMesh.subMeshCount;
            var mf = r.GetComponent<MeshFilter>();
            return mf != null && mf.sharedMesh != null ? mf.sharedMesh.subMeshCount : 1;
        }

        private class OutlinePassData
        {
            public Renderer[] renderers;
            public Material material;
            public TextureHandle colorTarget;
            public TextureHandle depthTarget;
        }
    }
}
