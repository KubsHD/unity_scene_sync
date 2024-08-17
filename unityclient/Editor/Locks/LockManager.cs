using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LockManager : UnityEditor.AssetModificationProcessor
{
    private static string[] OnWillSaveAssets(string[] paths)
    {
        List<string> okPaths = new List<string>();
        foreach (var path in paths)
        {
            if (path.EndsWith(".unity"))
            {
                var sceneName = path.Substring(path.LastIndexOf('/') + 1);
                sceneName = sceneName.Substring(0, sceneName.Length - 6);
                var lockedScenes = SceneSync.instance.GetLockedScenes();
                
                if (lockedScenes.Contains(sceneName))
                {
                    SceneView.lastActiveSceneView.ShowNotification(
                        new GUIContent("Scena zablokowana, zmiany nie zostały zapisane!"));
                    continue;
                }
            }
            
            okPaths.Add(path);
        }

        
        return okPaths.ToArray();
    }
}
