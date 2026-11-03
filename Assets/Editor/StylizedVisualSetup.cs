using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ShrinkToShoot.Editor
{
    // Editor only. Scene overrides keep the original vendor materials and prefabs intact.
    public static class StylizedVisualSetup
    {
        private const string Gameplay = "Assets/_Scenes/GamePlay.unity";
        private const string Art = "Assets/_Art/Rendering/";
        private const string Vendor = "Assets/AssetsStuff/Meshes/Selcuk Gerceker/Toon Environments - World Creator Pack Lite/Materials/";
        private const string Menu = "Tools/Shrink to Shoot/Visuals/";

        [MenuItem(Menu + "Apply Stylized Setup to Gameplay")]
        public static void Apply()
        {
            var scene = RequireGameplay();
            var renderer = Load<UniversalRendererData>(Art + "Stylized_Renderer.asset");
            var profile = Load<VolumeProfile>(Art + "Stylized_Gameplay.asset");
            // Keep renderer zero unchanged for other scenes. Both quality levels use slot one.
            foreach (var tier in new[] { "Mobile", "PC" })
            {
                var pipeline = Load<UniversalRenderPipelineAsset>($"Assets/Settings/{tier}_RPAsset.asset");
                var serialized = new SerializedObject(pipeline);
                var list = serialized.FindProperty("m_RendererDataList");
                if (list.arraySize < 2) list.arraySize = 2;
                list.GetArrayElementAtIndex(1).objectReferenceValue = renderer;
                serialized.FindProperty("m_SoftShadowsSupported").boolValue = true;
                serialized.FindProperty("m_SoftShadowQuality").intValue = tier == "Mobile" ? 1 : 2;
                serialized.ApplyModifiedProperties();
                pipeline.supportsHDR = true;
                pipeline.renderScale = 1;
                pipeline.mainLightShadowmapResolution = 2048;
                pipeline.shadowCascadeCount = 2;
                pipeline.cascade2Split = 0.4f;
                pipeline.cascadeBorder = 0.15f;
                pipeline.shadowDistance = 50;
                pipeline.shadowDepthBias = 0.25f;
                pipeline.shadowNormalBias = 0.35f;
                pipeline.colorGradingMode = ColorGradingMode.HighDynamicRange;
                EditorUtility.SetDirty(pipeline);
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Apply stylized gameplay visuals");
            var lights = Components<Light>(scene).Where(l => l.type == LightType.Directional).ToArray();
            if (lights.Length != 1) throw new InvalidOperationException("Expected one existing directional light; no light was added.");
            var sun = lights[0];
            Undo.RecordObject(sun, "Set warm sunlight");
            Undo.RecordObject(sun.transform, "Angle sunlight");
            sun.transform.rotation = Quaternion.Euler(50, -35, 0);
            sun.color = Color.white;
            sun.useColorTemperature = true;
            sun.colorTemperature = 5800;
            sun.intensity = 1.6f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.7f;
            Record(sun);
            Record(sun.transform);

            Undo.RecordObject(Unsupported.GetRenderSettings(), "Set ambient lighting");
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.68f, 0.76f, 0.88f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.61f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.43f, 0.39f, 0.30f);

            var volumes = Components<Volume>(scene).Where(v => v.isGlobal).ToArray();
            if (volumes.Length != 1) throw new InvalidOperationException("Expected one existing global Volume; no Volume was added.");
            var volume = volumes[0];
            Undo.RecordObject(volume, "Use stylized profile");
            volume.sharedProfile = profile;
            volume.enabled = true;
            volume.weight = 1;
            Record(volume);

            var camera = GameplayCamera(scene);
            var data = camera.GetUniversalAdditionalCameraData();
            Undo.RecordObject(camera, "Enable HDR");
            Undo.RecordObject(data, "Enable post processing");
            camera.allowHDR = true;
            data.SetRenderer(1);
            data.renderPostProcessing = true;
            data.renderShadows = true;
            data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            data.dithering = true;
            data.volumeLayerMask |= 1 << volume.gameObject.layer;
            Record(camera);
            Record(data);

            int changed = 0;
            foreach (var mesh in Components<MeshRenderer>(scene))
            {
                var materials = mesh.sharedMaterials;
                bool replace = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    var original = materials[i];
                    if (original == null || !AssetDatabase.GetAssetPath(original).StartsWith(Vendor, StringComparison.Ordinal)) continue;
                    var replacement = AssetDatabase.LoadAssetAtPath<Material>(Art + "Materials/" + original.name + "_Stylized.mat");
                    if (replacement == null) throw new InvalidOperationException("No project material for " + original.name);
                    materials[i] = replacement;
                    replace = true;
                    changed++;
                }
                if (!replace) continue;
                Undo.RecordObject(mesh, "Assign lit environment material");
                mesh.sharedMaterials = materials;
                Record(mesh);
            }
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log($"Stylized setup applied ({changed} material slots updated). Save GamePlay to keep scene changes.");
        }

        [MenuItem(Menu + "Validate Gameplay Visuals")]
        public static void Validate()
        {
            var scene = RequireGameplay();
            var camera = GameplayCamera(scene);
            var data = camera.GetUniversalAdditionalCameraData();
            if (!camera.allowHDR || !data.renderPostProcessing || !data.renderShadows)
                throw new InvalidOperationException("Gameplay camera requires HDR, post processing and shadows.");
            var renderer = Load<UniversalRendererData>(Art + "Stylized_Renderer.asset");
            if (!renderer.rendererFeatures.OfType<ScreenSpaceAmbientOcclusion>().Any(f => f.isActive))
                throw new InvalidOperationException("Stylized renderer is missing active SSAO.");
            foreach (var tier in new[] { "Mobile", "PC" })
            {
                var pipeline = Load<UniversalRenderPipelineAsset>($"Assets/Settings/{tier}_RPAsset.asset");
                var list = new SerializedObject(pipeline).FindProperty("m_RendererDataList");
                if (list.arraySize < 2 || list.GetArrayElementAtIndex(1).objectReferenceValue != renderer || !pipeline.supportsSoftShadows)
                    throw new InvalidOperationException(tier + " pipeline is missing the stylized renderer or soft shadows.");
            }
            var volume = Components<Volume>(scene).Single(v => v.isGlobal && v.enabled);
            if (volume.sharedProfile != Load<VolumeProfile>(Art + "Stylized_Gameplay.asset") || volume.weight <= 0 ||
                (data.volumeLayerMask.value & (1 << volume.gameObject.layer)) == 0)
                throw new InvalidOperationException("The gameplay camera cannot see the stylized global Volume.");
            if (!volume.sharedProfile.TryGet<Tonemapping>(out var tone) || tone.mode.value != TonemappingMode.Neutral)
                throw new InvalidOperationException("Neutral tonemapping is not configured.");
            var meshes = Components<MeshRenderer>(scene).Where(r => r.sharedMaterials.Any(m => m != null &&
                AssetDatabase.GetAssetPath(m).StartsWith(Art + "Materials/", StringComparison.Ordinal))).ToArray();
            if (meshes.Length == 0) throw new InvalidOperationException("No lit environment materials are assigned.");
            foreach (var mesh in meshes)
            {
                if (!mesh.receiveShadows || mesh.shadowCastingMode == ShadowCastingMode.Off)
                    throw new InvalidOperationException(mesh.name + " does not cast/receive shadows.");
                foreach (var material in mesh.sharedMaterials.Where(m => m != null))
                    if (material.shader == null || ShaderUtil.ShaderHasError(material.shader) ||
                        material.shader.name != "Universal Render Pipeline/Lit" || material.GetTexture("_BaseMap") == null)
                        throw new InvalidOperationException("Invalid lit environment material: " + material.name);
            }
            var cameraRenderer = new SerializedObject(data).FindProperty("m_RendererIndex").intValue;
            if (cameraRenderer != 1) throw new InvalidOperationException("Gameplay camera must select the stylized renderer (slot one).");
            if (PlayerSettings.colorSpace != ColorSpace.Linear) throw new InvalidOperationException("Expected Linear color space.");
            Debug.Log($"Visual validation passed: {meshes.Length} lit mesh renderers; palette textures, shadows, camera, Volume and both quality renderers configured. Device GPU profiling and image review are still required.");
        }

        [MenuItem(Menu + "Capture Gameplay Camera")]
        public static void Capture()
        {
            Validate();
            CaptureTo(Path.GetFullPath("Temp/Stylized-Gameplay.png"));
        }

        // For isolated Editor validation: -executeMethod ShrinkToShoot.Editor.StylizedVisualSetup.ValidateAndCaptureBatch
        public static void ValidateAndCaptureBatch()
        {
            EditorSceneManager.OpenScene(Gameplay);
            Validate();
            Directory.CreateDirectory("VisualValidation");
            // Capture the startup quality tier. Switching pipelines during offscreen rendering
            // can invalidate GPU resources before readback; use a fresh process for each tier.
            CaptureTo(Path.GetFullPath("VisualValidation/" + QualitySettings.names[QualitySettings.GetQualityLevel()] + ".png"));
            Validate();
        }

        private static void CaptureTo(string path)
        {
            var camera = GameplayCamera(RequireGameplay());
            // Keep the camera's transform, lens, projection and viewport unchanged.
            // An sRGB output target is needed for PNG readback; HDR still runs in URP's intermediate buffers.
            var target = RenderTexture.GetTemporary(1200, 2000, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            Texture2D pixels = null;
            try
            {
                // Initialize URP offscreen to avoid the batch Editor's tiny backbuffer/UI target.
                camera.targetTexture = target;
                camera.Render();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                pixels = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                Debug.Log("Gameplay camera capture: " + path);
            }
            finally
            {
                RenderTexture.active = previous;
                camera.targetTexture = previousTarget;
                RenderTexture.ReleaseTemporary(target);
                if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
            }
        }

        private static Scene RequireGameplay()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != Gameplay)
                throw new InvalidOperationException("Open Assets/_Scenes/GamePlay.unity in Edit Mode first.");
            return scene;
        }

        private static T[] Components<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true)).ToArray();

        private static Camera GameplayCamera(Scene scene) => Components<Camera>(scene).Single(c => c.CompareTag("MainCamera"));

        private static T Load<T>(string path) where T : UnityEngine.Object =>
            AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing asset: " + path);

        private static void Record(UnityEngine.Object target)
        {
            EditorUtility.SetDirty(target);
            if (PrefabUtility.IsPartOfPrefabInstance(target)) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
    }
}
