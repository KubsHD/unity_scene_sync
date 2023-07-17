using System;
using UnityEditor;
using UnityEngine;

public class SceneSyncSettigns : EditorWindow
{
    private String _url;
    private bool _enabled;
    private String _secret;

    public static string PREFS_KEY_URL = "scene_sync_url";
    public static string PREFS_KEY_SECRET = "scene_sync_secret";
    public static string PREFS_KEY_ENABLED = "scene_sync_enabled";

    
    [MenuItem("Storbeed/Scene Sync")]
    public static void Open()
    {
        GetWindow<SceneSyncSettigns>();
    }

    private void OnEnable()
    {
        _url = EditorPrefs.GetString(PREFS_KEY_URL);
        _enabled = EditorPrefs.GetBool(PREFS_KEY_ENABLED);
        _secret = EditorPrefs.GetString(PREFS_KEY_SECRET);
    }

    private void OnGUI()
    {
        _url = EditorGUILayout.TextField("Server URL", _url);
        _enabled = EditorGUILayout.Toggle("Enabled", _enabled);
        _secret = EditorGUILayout.TextField("Secret", _secret);

        
        if (GUILayout.Button("Save"))
            Save();
    }

    public void Save()
    {
        EditorPrefs.SetString(PREFS_KEY_URL, _url);
        EditorPrefs.SetBool(PREFS_KEY_ENABLED, _enabled);
        EditorPrefs.SetString(PREFS_KEY_SECRET, _secret);
    }
}