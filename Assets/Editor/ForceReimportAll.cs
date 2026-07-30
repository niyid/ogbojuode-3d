// Assets/Editor/ForceReimportAll.cs
//
// Invoked headless via:
//   Unity -batchmode -nographics -quit -projectPath <proj> -executeMethod ForceReimportAll.Run
//
// Forces Unity to (re)import every asset under Assets/ — this is what
// actually pulls in models/textures/etc. that exist on disk but don't have
// a .meta file yet, and regenerates import artefacts for the current
// render pipeline / Editor version. Also reports anything that's still
// missing or failed to load after the reimport, and flags any model paths
// the SceneSetupWizard scripts expect but can't find.

using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ForceReimportAll
{
    // Paths the two sibling projects' SceneSetupWizard scripts look for.
    // Harmless to check both lists in either project — missing ones just
    // get skipped/reported, they don't block anything.
    private static readonly string[] ExpectedCharacterFolders =
    {
        "Assets/Models/Characters/tribal_warrior_3d_model",
        "Assets/Models/Characters/agbako_creature_3d_model",
        "Assets/Models/Characters/brute_giant_3d_model",
        "Assets/Models/Characters/creature_eru_3d_model",
        "Assets/Models/Characters/ostrich_king_3d_model",
        "Assets/Models/Characters/forest_spirit_3d_model",
        "Assets/Models/Characters/musket",
        "Assets/Models/Characters/Player_Ireke_Onibudo",
        "Assets/Models/Characters/Flying_Snake_Ejo_Fifo",
        "Assets/Models/Characters/Wrestler_Cat_Ologbo_Ijakadi",
        "Assets/Models/Characters/Warrior_Fish_Eja_Jagunjagun",
        "Assets/Models/Characters/Arogidigba_Mermaid_Queen",
        "Assets/Models/Characters/Mother_Spirit_Guide_Iya_Ireke",
    };

    private static readonly string[] ExpectedProps =
    {
        "Assets/Models/Props/pillar-wood.fbx",
        "Assets/Models/Props/blade.fbx",
        "Assets/Models/Props/wall-wood.fbx",
        "Assets/Models/Props/roof-high-point.fbx",
        "Assets/Models/Props/tree_oak_dark.fbx",
        "Assets/Models/Props/mushroom_redGroup.fbx",
        "Assets/Models/Props/campfire-pit.fbx",
        "Assets/Models/Props/fence-fortified.fbx",
        "Assets/Models/Props/Spear_Oko_Eja",
    };

    public static void Run()
    {
        Debug.Log("[ForceReimportAll] Editor version: " + Application.unityVersion);
        Debug.Log("[ForceReimportAll] Forcing full AssetDatabase reimport...");

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();

        Debug.Log("[ForceReimportAll] Reimport pass complete.");

        // --- Report every model Unity actually has, and whether it loads.
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models" });
        Debug.Log($"[ForceReimportAll] {modelGuids.Length} model asset(s) found under Assets/Models.");

        int failedLoads = 0;
        foreach (var guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj == null)
            {
                Debug.LogWarning($"[ForceReimportAll] MODEL FAILED TO LOAD after reimport: {path}");
                failedLoads++;
            }
        }

        // --- Report expected folders/files that are simply absent.
        int missingCharacters = 0;
        foreach (var folder in ExpectedCharacterFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                continue; // not part of this project, ignore silently

            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[ForceReimportAll] MISSING ARTEFACT: no model found in {folder}");
                missingCharacters++;
            }
        }

        int missingProps = 0;
        foreach (var propPath in ExpectedProps)
        {
            bool exists = AssetDatabase.IsValidFolder(propPath)
                ? AssetDatabase.FindAssets("t:Model", new[] { propPath }).Length > 0
                : AssetDatabase.LoadAssetAtPath<GameObject>(propPath) != null;

            if (!exists)
            {
                Debug.LogWarning($"[ForceReimportAll] MISSING ARTEFACT: {propPath}");
                missingProps++;
            }
        }

        Debug.Log($"[ForceReimportAll] SUMMARY: {failedLoads} failed load(s), " +
                  $"{missingCharacters} missing character folder(s), " +
                  $"{missingProps} missing prop(s).");
        Debug.Log("[ForceReimportAll] Done.");
    }
}
