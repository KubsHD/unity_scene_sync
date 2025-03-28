using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SaveFailReason
{
    OK,
    LOCKED_SCENE,
    SCENE_NOT_CHECKED_OUT,
    PENDING_CHANGES_FROM_VCS
}

public class LockManager : AssetModificationProcessor
{
    private static string[] OnWillSaveAssets(string[] paths)
    {
        SaveFailReason reason = SaveFailReason.OK;
        
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
                    reason = SaveFailReason.LOCKED_SCENE;
                    continue;
                }

                if (!SceneSync.instance.CheckIfUserLockedCurrentScene(SceneSync.instance.GetUserInfo()))
                {
                    reason = SaveFailReason.SCENE_NOT_CHECKED_OUT;
                    continue;
                }
            }
            
            okPaths.Add(path);
        }

        switch (reason)
        {
            case SaveFailReason.OK:
                break;
            case SaveFailReason.LOCKED_SCENE:
                SceneView.lastActiveSceneView.ShowNotification(
                    new GUIContent("Scena zablokowana, zmiany nie zostały zapisane!"));
                break;
            case SaveFailReason.SCENE_NOT_CHECKED_OUT:
                SceneView.lastActiveSceneView.ShowNotification(
                    new GUIContent("Musisz najpierw zablokować scene aby zapisać zmiany!"));
                break;
        }
        
        return okPaths.ToArray();
    }
}
