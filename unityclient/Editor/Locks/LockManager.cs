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
                if (SceneSync.instance.GetLockedScenes().Contains(sceneName))
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
