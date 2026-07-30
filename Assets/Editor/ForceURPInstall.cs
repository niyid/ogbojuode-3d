// Assets/Editor/ForceURPInstall.cs
//
// Invoked headless via:
//   Unity -batchmode -nographics -projectPath <proj> -executeMethod ForceURPInstall.Run
//
// NOTE: no -quit on the command line for this one -- this script calls
// EditorApplication.Exit() itself once the (async) package install
// finishes, since Client.Add() doesn't complete synchronously and -quit
// would kill Unity before it's done.
//
// Only installs the package via the stable Package Manager API (lets
// Unity resolve the correct version for this Editor build itself, rather
// than us hardcoding a semver that might not exist). Does NOT attempt to
// auto-create/assign a URP pipeline asset -- that's one quick manual step
// in the Editor afterward (safer than guessing at internal asset-creation
// API surface).

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class ForceURPInstall
{
    private const string PackageName = "com.unity.render-pipelines.universal";
    private static AddRequest _request;

    public static void Run()
    {
        if (IsAlreadyInstalled())
        {
            Debug.Log($"[ForceURPInstall] {PackageName} is already installed. Nothing to do.");
            EditorApplication.Exit(0);
            return;
        }

        Debug.Log($"[ForceURPInstall] Requesting install of {PackageName}...");
        _request = Client.Add(PackageName);
        EditorApplication.update += Progress;
    }

    private static bool IsAlreadyInstalled()
    {
        foreach (var p in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
        {
            if (p.name == PackageName)
                return true;
        }
        return false;
    }

    private static void Progress()
    {
        if (_request == null || !_request.IsCompleted)
            return;

        EditorApplication.update -= Progress;

        if (_request.Status == StatusCode.Success)
        {
            Debug.Log($"[ForceURPInstall] SUCCESS: installed {_request.Result.packageId}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[ForceURPInstall] FAILED: {_request.Error?.message}");
            EditorApplication.Exit(1);
        }
    }
}
