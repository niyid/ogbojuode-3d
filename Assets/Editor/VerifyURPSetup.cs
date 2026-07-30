// Assets/Editor/VerifyURPSetup.cs
//
// Invoked headless via:
//   Unity -batchmode -nographics -projectPath <proj> -executeMethod VerifyURPSetup.Run -quit
//
// Purpose: substitute for "open the Editor GUI and eyeball the scene for pink
// materials" when GUI usage isn't an option (e.g. memory constraints). This
// can't literally render a frame and screenshot it in -nographics mode, but
// it checks the two things that actually cause the pink/unlit symptom:
//
//   1. Is a UniversalRenderPipelineAsset actually assigned as the default
//      render pipeline AND the active quality level's pipeline? (Both must
//      be set -- a project can have one set and not the other, which still
//      renders pink in some views.)
//   2. For every scene in Build Settings (or every .unity scene under
//      Assets/ if Build Settings is empty), open it and scan all renderers'
//      materials for the actual Unity "error material" shader
//      (Hidden/InternalErrorShader) -- that pink checkerboard is not a URP
//      thing, it's Unity's fallback for "this material's shader failed to
//      compile or is missing," and it's directly detectable without
//      rendering a frame.
//
// This is a diagnostic script, not a fix -- it logs PASS/FAIL per check and
// exits with a summary. If a scene shows a real compile-error material,
// that's a separate problem from the pipeline assignment and gets called
// out with a scene+object path so it doesn't need to be found by eye.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class VerifyURPSetup
{
    public static void Run()
    {
        bool allPass = true;

        // --- Check 1: pipeline assignment ---
        var defaultPipeline = GraphicsSettings.defaultRenderPipeline;
        var qualityPipeline = QualitySettings.renderPipeline;

        if (defaultPipeline is UniversalRenderPipelineAsset)
        {
            Debug.Log($"[VerifyURPSetup] PASS: GraphicsSettings.defaultRenderPipeline is a UniversalRenderPipelineAsset ({defaultPipeline.name}).");
        }
        else
        {
            Debug.LogError($"[VerifyURPSetup] FAIL: GraphicsSettings.defaultRenderPipeline is {(defaultPipeline == null ? "null" : defaultPipeline.GetType().Name)}, expected UniversalRenderPipelineAsset.");
            allPass = false;
        }

        if (qualityPipeline is UniversalRenderPipelineAsset)
        {
            Debug.Log($"[VerifyURPSetup] PASS: QualitySettings.renderPipeline (active quality level '{QualitySettings.names[QualitySettings.GetQualityLevel()]}') is a UniversalRenderPipelineAsset ({qualityPipeline.name}).");
        }
        else if (qualityPipeline == null && defaultPipeline is UniversalRenderPipelineAsset)
        {
            Debug.Log("[VerifyURPSetup] PASS: QualitySettings.renderPipeline is null (inherits default pipeline) -- OK since default is URP.");
        }
        else
        {
            Debug.LogError($"[VerifyURPSetup] FAIL: QualitySettings.renderPipeline is {(qualityPipeline == null ? "null" : qualityPipeline.GetType().Name)} and default pipeline isn't URP either.");
            allPass = false;
        }

        // --- Check 2: scan scenes for the error-shader material ---
        var scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToList();

        if (scenePaths.Count == 0)
        {
            Debug.LogWarning("[VerifyURPSetup] No scenes in Build Settings -- falling back to scanning all .unity files under Assets/.");
            scenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .ToList();
        }

        if (scenePaths.Count == 0)
        {
            Debug.LogWarning("[VerifyURPSetup] No scenes found at all -- skipping material scan.");
        }

        foreach (var scenePath in scenePaths)
        {
            Scene scene;
            try
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VerifyURPSetup] FAIL: could not open scene '{scenePath}': {e.Message}");
                allPass = false;
                continue;
            }

            var badMaterialsInScene = new List<string>();

            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null)
                    {
                        badMaterialsInScene.Add($"{GetPath(renderer.transform)} -- missing material reference (renders pink/magenta)");
                        continue;
                    }

                    if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
                    {
                        badMaterialsInScene.Add($"{GetPath(renderer.transform)} -- material '{mat.name}' has a broken/error shader (renders pink checkerboard)");
                    }
                }
            }

            if (badMaterialsInScene.Count == 0)
            {
                Debug.Log($"[VerifyURPSetup] PASS: scene '{scenePath}' -- no error-shader or missing materials found.");
            }
            else
            {
                allPass = false;
                Debug.LogError($"[VerifyURPSetup] FAIL: scene '{scenePath}' has {badMaterialsInScene.Count} problem material(s):");
                foreach (var line in badMaterialsInScene)
                    Debug.LogError($"[VerifyURPSetup]   - {line}");
            }
        }

        // --- Summary ---
        if (allPass)
        {
            Debug.Log("[VerifyURPSetup] ===== OVERALL: PASS -- URP is assigned and no pink/error materials detected. =====");
        }
        else
        {
            Debug.LogError("[VerifyURPSetup] ===== OVERALL: FAIL -- see errors above. Do not commit until resolved. =====");
        }
    }

    private static string GetPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = $"{t.name}/{path}";
        }
        return path;
    }
}
