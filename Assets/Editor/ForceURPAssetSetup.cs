// Assets/Editor/ForceURPAssetSetup.cs
//
// Invoked headless via:
//   Unity -batchmode -nographics -projectPath <proj> -executeMethod ForceURPAssetSetup.Run -quit
//
// Run this AFTER ForceURPInstall.cs has installed the URP package.
// (This one can use -quit since AssetDatabase.SaveAssets() is synchronous --
// unlike Client.Add() in ForceURPInstall.cs, there's no async request to wait on.)
//
// What this replaces: the manual "right-click > Create > Rendering > URP >
// Pipeline Asset (Forward Renderer)" step, then manually dragging the result
// into Edit > Project Settings > Graphics and Quality.
//
// Verified public API used (not guessed):
//   - UniversalRendererData        : ScriptableObject.CreateInstance<T>() -- standard SO creation
//   - UniversalRenderPipelineAsset.Create(ScriptableRendererData)
//       -> documented static factory that wires the asset to the renderer data
//          correctly (this is what the Editor's own "Create" menu item calls
//          under the hood -- confirmed via Unity Discussions thread showing
//          working headless usage of this exact call)
//   - GraphicsSettings.defaultRenderPipeline  : stable, non-deprecated static property
//   - QualitySettings.renderPipeline          : per-quality-level override, same pattern
//
// Deliberately NOT touched: individual URP asset tuning (shadow distance,
// MSAA, HDR, etc.) -- those are 'get'-only on UniversalRenderPipelineAsset
// itself and only reachable via SerializedObject + private field names
// (m_SupportsTerrainHoles, m_MainLightRenderingMode, ...), which is rewriting
// private serialized fields rather than a public API. That's a real
// automation gap, not an oversight -- if you want non-default quality
// settings later, that's the next thing worth doing by hand in the Editor
// GUI rather than scripting around private fields.

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ForceURPAssetSetup
{
    private const string SettingsFolder = "Assets/Settings";
    private const string RendererDataPath = SettingsFolder + "/URP-HighFidelity-Renderer.asset";
    private const string PipelineAssetPath = SettingsFolder + "/URP-HighFidelity.asset";

    public static void Run()
    {
        if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)
        {
            Debug.Log("[ForceURPAssetSetup] A URP asset is already assigned as the default render pipeline. Nothing to do.");
            return;
        }

        if (!Directory.Exists(SettingsFolder))
        {
            Directory.CreateDirectory(SettingsFolder);
            AssetDatabase.Refresh();
        }

        // 1. Create the renderer data (the "Forward Renderer" companion asset).
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        CreateOrReplaceAsset(rendererData, RendererDataPath);

        // 2. Create the pipeline asset via the documented factory, wired to that renderer.
        var pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
        CreateOrReplaceAsset(pipelineAsset, PipelineAssetPath);

        // 3. Assign it as the project's default pipeline (Graphics settings)
        //    and as the pipeline for the active quality level (Quality settings) --
        //    the two things the manual Editor steps do.
        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ForceURPAssetSetup] SUCCESS: created {PipelineAssetPath} and {RendererDataPath}, " +
                  "assigned as default render pipeline and active quality-level pipeline.");
    }

    private static void CreateOrReplaceAsset(Object asset, string path)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);

        AssetDatabase.CreateAsset(asset, path);
        EditorUtility.SetDirty(asset);
    }
}
