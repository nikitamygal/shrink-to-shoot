# Stylized URP visual restoration

The gameplay scene now uses lit environment materials, warm angled sunlight, soft shadows, contact AO and a restrained color grade. The existing geometry, scene hierarchy, prefab assets, gameplay code and camera transform/projection are preserved.

[View the saved gameplay-camera preview](Visuals/Gameplay-Preview.png). The final Mobile and PC Editor captures are identical with the current per-light shadow filter.

## Findings

- Unity is **6000.6.0f1**, URP **17.6.0**. The environment's `Spring.mat` uses **Universal Render Pipeline/2D/Mesh2D-Lit-Default**. This shader is for 2D lighting, so the 3D directional light cannot produce the required surface shading and cast/received shadows. This is the main cause of the flat silhouettes in `Assets/References/current.png`.
- All **233 environment MeshRenderers** in GamePlay use the Spring palette. Trees, their decorative leaves, rocks, grass, buildings and tiles already have shadow casting and receiving enabled. Their shader was the problem; no geometry or normal changes were necessary.
- The gameplay camera supports HDR and renders shadows, but **post-processing was disabled**. The existing global Volume referenced the pipeline's default profile, without a dedicated gameplay grade.
- Sun elevation was **81.7°**, nearly overhead. Lowering it exposes side shading and makes cast shadows easier to see.
- Android defaults to quality index **0, Mobile**. The Mobile pipeline had **80% render scale**, no antialiasing, one 1024 shadow atlas cascade, no soft-shadow support and no AO feature. PC defaults to index **1** and had a separate renderer with SSAO. Previewing PC alone did not represent Android quality.
- Linear color space and pipeline/camera HDR support were already enabled. `GraphicsSettings.m_CustomRenderPipeline` is empty, but both quality levels provide valid URP overrides; the empty fallback is not the cause.
- The vendor demo is `TEWCPL_Scene.unity`. It shares the converted vendor palette materials and ordinary skybox/ambient settings. The installed pack contains palette textures and no custom environment shader or supplied baked lighting that reproduces the promotional image. UI and FX intentionally use their own shaders and were left unchanged.

## Files changed

| File | Change |
| --- | --- |
| `Assets/_Scenes/GamePlay.unity` | 233 material-slot overrides on existing prefab instances; existing light, ambient lighting, Volume and camera rendering settings adjusted |
| `Assets/_Art/Rendering/Materials/Spring_Stylized.mat` | Project-owned URP Lit copy, referencing the original Spring texture |
| `Assets/_Art/Rendering/Stylized_Renderer.asset` | Dedicated Forward renderer with subtle SSAO |
| `Assets/_Art/Rendering/Stylized_Gameplay.asset` | Reusable gameplay Volume Profile |
| `Assets/Settings/Mobile_RPAsset.asset` | Added gameplay renderer at index 1; adjusted shadows, grading and render scale |
| `Assets/Settings/PC_RPAsset.asset` | Added gameplay renderer at index 1; aligned shadow and grading settings |
| `Assets/Editor/StylizedVisualSetup.cs` | Apply, validate and camera-capture menu commands |

New Unity assets include stable `.meta` files. No vendor material, texture, model, prefab, gameplay script, quality-platform mapping or unrelated scene was edited. The existing default renderers remain at index 0; only the gameplay camera selects renderer 1. Pipeline-level shadow, grading and render-scale changes apply to any other scene using the same quality pipeline.

## Rendering settings

| Setting | Final value |
| --- | --- |
| Environment shader | Standard URP Lit, opaque, metallic 0, smoothness 0.12; specular highlights and environment reflections disabled |
| Palette | Original Spring texture, tint, UV scale/offset retained; `_MainTex` explicitly mapped to `_BaseMap`. The original texture has no alpha channel, so opaque rendering preserves its intended appearance |
| Sun | Existing light; rotation (50, -35, 0), intensity 1.6, temperature 5800 K |
| Shadows | Soft; strength 0.7; 2048 atlas, two cascades, split 0.4, 50-unit distance, depth/normal bias 0.25/0.35; the existing light uses the Low filter. Pipeline defaults are Low on Mobile and Medium on PC |
| Ambient | Trilight: sky (0.68, 0.76, 0.88), equator (0.55, 0.61, 0.62), ground (0.43, 0.39, 0.30); existing skybox retained |
| Camera | Existing HDR support retained; post-processing, shadows, dithering and FXAA enabled; transform, lens, orthographic size and viewport unchanged |
| Pipeline | HDR retained; HDR color grading, render scale 1.0; SRP Batcher retained; no MSAA increase |
| Tonemapping | Neutral |
| Grade | Exposure +0.35 EV, contrast -5, saturation +6, white-balance temperature +3, tint 0 |
| Bloom | Intensity 0.06, threshold 1.15, scatter 0.35, quarter-resolution, four iterations, inexpensive filtering |
| SSAO | Standard renderer feature; half-resolution, depth-reconstructed normals at Medium quality, four samples, bilateral blur; intensity 0.45, radius 0.25, direct-light influence 0.12, falloff 50 |

SSAO uses the settings serialized by the installed URP 17.6 renderer feature, without private API calls, a custom render pass or GTAO. The depth-based, downsampled path follows [Unity's SSAO configuration guidance](https://docs.unity.cn/6000.6/Documentation/Manual/urp/ssao-renderer-feature-reference.html). AO is kept small to avoid broad dark halos; bloom affects only bright areas. There is no depth of field, motion blur, temporal antialiasing, realtime GI or additional shadow-casting light.

## Using the Editor

1. Let Unity import the changed assets and reload `Assets/_Scenes/GamePlay.unity` from disk.
2. Run **Tools → Shrink to Shoot → Visuals → Validate Gameplay Visuals**. The committed scene already contains the setup.
3. To reapply the scene settings, run **Apply Stylized Setup to Gameplay**, then save the scene. It reuses the existing light, camera and global Volume, and records scene/prefab changes for Undo. It only runs on GamePlay in Edit Mode. Pipeline asset settings are saved separately.
4. **Capture Gameplay Camera** writes an sRGB preview to `Temp/Stylized-Gameplay.png`, retaining the camera's transform and projection. The output is a 1200 × 2000 portrait preview; its aspect ratio may differ from your Game View. Screen-space overlay UI is excluded from the single-camera request.

For isolated automation, `ShrinkToShoot.Editor.StylizedVisualSetup.ValidateAndCaptureBatch` opens GamePlay, validates it, renders the startup quality tier into `VisualValidation/<quality>.png`, then validates again. Use `-batchmode -executeMethod ShrinkToShoot.Editor.StylizedVisualSetup.ValidateAndCaptureBatch -quit`. Run each startup quality tier in a separate project copy/process.

## Validation and limits

- Compiled and rendered using the installed Unity 6000.6.0f1 Editor in an isolated copy of this project.
- Unity validation confirms all 233 environment renderers use URP Lit with their palette textures, cast and receive shadows, and that camera HDR, post-processing, Volume visibility, renderer selection and both pipeline configurations are correct.
- Final capture runs check shader errors after rendering. The scene comparison confirms identical object IDs and prefab hierarchy/placement, unchanged camera transform and lens, and no prefab edits beyond material references. The sun is the only transform changed.
- The isolated project logs existing legacy DOTween PluginImporter-version messages. These third-party assets were left untouched.
- Preview captures show the shading gradients, decorative leaf relief, contact shading and cast shadows that were absent from the supplied current image. They are Editor renders; Android graphics API builds, device GPU timings, thermals and gameplay-in-motion testing still require an Android device.
- Mobile cost is bounded by one main shadow light, two cascades, FXAA and half-resolution four-sample AO. Full render scale costs more than the previous 0.8 setting. Profile on the minimum supported device; AO is the first optional effect to disable if needed, followed by shadow atlas resolution or render scale.
- The target has a lower camera angle, denser set dressing, different palette regions and much broader diffuse shadowing/bounce light. Preserving the existing camera, geometry and texture colors prevents an exact match. Standard realtime directional shadows plus SSAO approximate the look; reproducing broad area-light shadows and full multibounce illumination would require baked lighting or a more expensive lighting solution.
