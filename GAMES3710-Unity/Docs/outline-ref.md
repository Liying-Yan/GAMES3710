# Outline effects in Unity URP: a complete technical guide

**The Universal Render Pipeline's single-pass architecture fundamentally changes how outline (描边) effects must be implemented compared to the built-in pipeline.** URP only renders passes matching specific `LightMode` tags, breaks traditional multi-pass workflows, and introduces new constraints around the SRP Batcher, stencil buffer, and depth textures. Five primary techniques have emerged — each with distinct trade-offs. Unity 6's Render Graph API adds another migration layer, obsoleting the `Execute()` method that most existing tutorials rely on. This guide covers every major approach, their URP-specific pitfalls, and the differences between Unity 2022 and Unity 6.

---

## The five outline techniques and when to use each

URP outline implementations fall into five categories. **Inverted hull (back-face extrusion)** is the most popular for per-object toon/anime outlines; it renders each mesh twice with the second pass culling front faces and extruding vertices along normals. **Post-processing edge detection** applies Sobel or Roberts Cross filters to depth/normal buffers in a single full-screen pass — ideal for global stylized outlines. **Stencil-based approaches** use the stencil buffer to mask regions and control where outlines appear, often combined with inverted hull to suppress inner lines. **Jump Flood Algorithm (JFA)** enables very wide outlines (hundreds of pixels) at near-constant cost via an O(log₂ n) flood-fill on distance fields. Finally, **Fresnel/rim-based** methods use `1 - dot(viewDir, normal)` for soft edge glow — simple but only effective on smooth, rounded geometry.

For production work, the Chinese and English-speaking communities converge on the same recommendation: use **Renderer Features** (either the built-in `Render Objects` or custom `ScriptableRendererFeature`) with a **separate single-pass outline shader** rather than trying to force multi-pass rendering within a single shader. This preserves SRP Batcher compatibility on the main material while keeping the architecture clean.

---

## Inverted hull: the workhorse method and its many gotchas

The inverted hull method is conceptually simple — extrude vertices along normals, cull front faces, output a flat color — but URP's architecture creates several traps. Since **URP's `DrawObjectsPass` only recognizes four LightMode tags** (`UniversalForward`, `UniversalForwardOnly`, `LightweightForward`, `SRPDefaultUnlit`), a naive second pass in the same shader is silently dropped. Three workarounds exist:

**Approach 1 — Tag-based dual pass.** Tag the main pass as `UniversalForward` and the outline pass as `SRPDefaultUnlit`. Both get picked up by `DrawObjectsPass`. However, this **breaks SRP Batcher compatibility** because multi-pass shaders with different CBUFFER layouts cannot be batched. Multiple Chinese developer blogs confirm: "URP下双Pass是有代价的，shader无法被SRP batching机制优化."

**Approach 2 — Second material on MeshRenderer.** Add the outline shader as a second material slot. Each material renders its own pass independently. Both shaders can remain individually SRP-Batcher-compatible. This is the **most widely recommended approach** (endorsed by Cyanilux and the broader community).

**Approach 3 — Renderer Feature with override material.** Use URP's built-in `Render Objects` feature or a custom `ScriptableRendererFeature` to re-render objects on a specific layer with an outline-only material. This is the most URP-idiomatic pattern and avoids consuming material slots.

The vertex extrusion itself introduces further pitfalls. **Hard-edge models** (cubes, mechanical parts) have split normals at edges, causing the extruded outline to fork and tear (描边断裂). The fix is to pre-bake smoothed (averaged) normals into the vertex color or tangent channel, then sample those instead of mesh normals. For **skinned meshes**, the tangent channel is preferred because Unity transforms tangents during skinning — smoothed normals stored in tangents animate correctly with bones. Blender's auto-smooth (自动平滑顶点) is reported as an "instantly effective" quick fix for prototyping.

**Object-space extrusion produces outlines that scale with camera distance** (近大远小). The standard fix is clip-space extrusion with perspective correction:

```hlsl
float2 offset = normalize(clipNormal.xy) * _OutlineThickness;
output.positionCS.xy += offset * output.positionCS.w;  // cancel perspective division
```

Additionally, the NDC-to-screen mapping stretches non-uniformly on non-square displays. Dividing the x-offset by screen aspect ratio corrects this. For distance-adaptive width, Chinese developers commonly apply `smoothstep` between near/far distance thresholds.

---

## Post-processing edge detection: the cleanest global solution

A single full-screen pass sampling `_CameraDepthTexture` and `_CameraNormalsTexture` with a Sobel or Roberts Cross operator detects silhouette edges (depth discontinuities) and surface creases (normal discontinuities). This is **resolution-dependent rather than geometry-dependent**, making it ideal for scenes with many outlined objects.

Since Unity 2022.2 (URP 14), the simplest setup uses the **Full Screen Pass Renderer Feature** paired with a **Fullscreen Shader Graph**. The `URP Sample Buffer` node provides NormalWorldSpace and BlitSource inputs; the `Scene Depth` node provides depth. Sample four neighbors, compute gradient magnitudes, threshold, and lerp with the original color. No C# code required.

For earlier URP versions or more control, a custom `ScriptableRendererFeature` performs the same Blit operation. Call `ConfigureInput(ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Depth)` to ensure URP generates the required textures. The normals texture (`_CameraNormalsTexture`) has been available since **URP 10 (Unity 2020.2)**; before that, a custom pre-pass was required.

Critical limitations apply:

- **MSAA is incompatible** with screen-space outline methods. The depth/normal textures rely on the depth-stencil buffer, which MSAA modifies. Use FXAA or SMAA instead.
- **Transparent objects are excluded** from depth and normal textures entirely. They can only be detected via color-based edge detection.
- **Depth precision degrades at distance**, producing false edges on far objects. Multiplying the threshold by eye depth mitigates this.
- **TAA jitter breaks outline shaders** that sample `_CameraDepthTexture` (written before jitter). Use `_CameraDepthAttachment` or `activeDepthBuffer` instead.
- The approach outlines **everything in the scene** uniformly. For selective outlines, render target objects to a mask RT first via a separate Renderer Feature, then apply edge detection only on that mask.

---

## Stencil, JFA, and specialized approaches

**Stencil-based outlines** are most commonly used as a supplement to inverted hull — writing a reference value in the main pass, then testing `NotEqual` in the outline pass to suppress inner lines. Different stencil IDs per object control whether outlines merge or remain separate.

The biggest URP-specific stencil pitfall is that **the stencil buffer is coupled with the depth buffer**. If a depth texture copy or MSAA resolve occurs between the stencil write and read passes, **stencil values get silently cleared**. This has caused widespread confusion across Unity versions 11–15. The fix: either disable Depth Priming Mode in the URP Renderer Asset, or ensure both the stencil write and test execute within a single `ScriptableRenderPass` (technically multiple GPU draw calls, but guaranteed sequential execution). Shader Graph cannot access stencil operations at all.

**Jump Flood Algorithm** outlines, pioneered by Ben Golus and ported to URP by Scott Daley and Alexander Ameye, enable outlines of **2000+ pixels at under 1ms on 1080p**. The technique uses ⌈log₂(width)⌉ flood passes: a 14-pixel outline needs only 4 passes (step sizes 8, 4, 2, 1). The URP implementation uses a multi-pass shader within a custom `ScriptableRendererFeature`: stencil mask → silhouette fill → JFA init (edge detect) → JFA flood (iterated) → composite. It requires either MSAA enabled or Depth Texture disabled due to stencil buffer coupling. For moderate-width outlines under ~10 pixels, Gaussian blur is competitive; JFA dominates above that threshold.

---

## What broke between Unity 2022 and Unity 6

Unity 6 (URP 17) introduces the **Render Graph API** as the mandatory replacement for the legacy `ScriptableRenderPass` workflow. This is the single largest breaking change for outline implementations. Every custom outline Renderer Feature must be migrated.

| Aspect | Unity 2022 (URP 14–15) | Unity 6 (URP 17) |
|--------|------------------------|-------------------|
| Custom pass entry point | `Execute(ScriptableRenderContext, ref RenderingData)` | `RecordRenderGraph(RenderGraph, ContextContainer)` — **old method obsolete** |
| Texture access | `Shader.GetGlobalTexture("_CameraDepthTexture")` works | Returns **1×1 black texture**; must use `UniversalResourceData.cameraDepthTexture` |
| Render target setup | `cmd.SetRenderTarget()` | `builder.SetRenderAttachment()` via Render Graph builder |
| Temporary textures | `cmd.GetTemporaryRT()` or `RenderingUtils.ReAllocateIfNeeded()` | `renderGraph.CreateTexture(desc)` — lifecycle managed automatically |
| Blit operations | `CommandBuffer.Blit()` or `Blitter.BlitCameraTexture()` | `AddBlitPass()` or custom raster pass in Render Graph |
| `ClearFlag.Depth` behavior | Implicitly clears stencil | **Does NOT clear stencil** — must use explicit `ClearFlag.Stencil` |
| Compatibility mode | N/A | Available for migrating projects but **will be removed in future versions** |

The **Full Screen Pass Renderer Feature** with Fullscreen Shader Graph requires **minimal migration** — it works nearly identically across both versions. This makes it the safest path for screen-space outlines.

For inverted hull via `Render Objects`, the feature still works in Unity 6. Custom `ScriptableRendererFeature` classes retain the same `Create()` and `AddRenderPasses()` structure — only the inner pass logic changes. The new Render Graph uses a **PassData pattern**: resources are declared during recording, then a static execution function receives them. Frame data is accessed through `ContextContainer`:

```csharp
UniversalResourceData frameData = frameContext.Get<UniversalResourceData>();
TextureHandle depth = frameData.cameraDepthTexture;
TextureHandle normals = frameData.cameraNormalsTexture;
```

Unity 6 also added a **Camera History API** for accessing previous-frame textures, potentially useful for temporal outline stabilization. No major new Shader Graph nodes for outlines were added in Unity 6.0, though Unity 6.3 brought custom lighting support.

---

## Practical recommendations and the community consensus

**For global/stylized outlines** (NPR, cel-shading scene-wide), post-processing edge detection via Full Screen Pass + Fullscreen Shader Graph is the most efficient and forward-compatible approach. A single full-screen pass costs **~0.1–0.3ms** regardless of object count, works identically in Unity 2022 and Unity 6, and requires no per-object setup. Combine depth and normal edge detection for best results; add color-based detection for transparent objects.

**For per-object toon outlines** (anime-style character rendering), inverted hull via a second material or Renderer Feature remains standard. Use clip-space extrusion with `*w` correction, smoothed normals in tangent channel, and distance-adaptive width. The popular open-source references — ColinLeung-NiloCat's `UnityURPToonLitShaderExample` and `GenshinCelShaderURP` — demonstrate production-quality SRP-Batcher-compatible implementations with these techniques.

**For selection/highlight outlines** (interaction feedback, RTS unit selection), the JFA approach via custom Renderer Feature provides width-independent performance. For narrow outlines under 5 pixels, a simpler stencil + single-dilation approach suffices.

Key defensive practices the community emphasizes:

- **Always check Frame Debugger** when outlines disappear — a single broken Renderer Feature entry can cascade-fail all subsequent features (documented CSDN bug)
- **Disable Depth Priming Mode** if using stencil-based outlines to prevent silent stencil clearing
- **Disable MSAA** when using screen-space outlines or stencil techniques; use FXAA/SMAA instead
- **Never use `UsePass`** — it imports CBUFFER from another shader, breaking SRP Batcher compatibility
- **Avoid `MaterialPropertyBlock`** for per-object outline customization — it disables SRP Batcher. Use material instances instead
- **For Unity 6 migration**, start with Compatibility Mode enabled, then rewrite passes to Render Graph before Unity removes compatibility mode in a future release. The official URP RenderGraph Samples (available via Package Manager) provide migration templates

The most-referenced community resources include Cyanilux's Renderer Features tutorial, Alexander Ameye's "5 Ways to Draw an Outline," Ben Golus's "Quest for Very Wide Outlines," the 《Unity Shader入门精要》textbook by 冯乐乐, and Ned Makes Games' video series. Commercial solutions like **Linework** (Alexander Ameye) and **Flat Kit** (Dustyroom) offer production-ready URP outline rendering with Unity 6 Render Graph support built in.

## Conclusion

URP outline implementation is not a single technique but a toolkit — the right choice depends on whether outlines are global or per-object, how wide they need to be, and whether SRP Batcher compatibility matters. The pipeline's single-pass constraint, which initially seemed limiting, has pushed the community toward cleaner Renderer Feature architectures that are actually more maintainable than the old multi-pass approach. The most significant near-term risk is Unity 6's Render Graph migration: any team still using `Execute()`-based custom passes should begin porting now, as compatibility mode is explicitly temporary. The Full Screen Pass approach offers the smoothest upgrade path, while inverted hull implementations need the most careful migration planning due to their reliance on custom draw calls and stencil operations.