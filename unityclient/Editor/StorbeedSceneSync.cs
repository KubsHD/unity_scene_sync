using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine.SceneManagement;
using FilePathAttribute = UnityEditor.FilePathAttribute;

public struct UserInfo
{
    public String name;
    public String id;
    public String scene;
    public int date;
}

[Overlay(typeof(SceneView), "Scene sync")]
public class SceneUsersOverlay : Overlay
{
    Label _label;
    private Button _refreshButton;
    private VisualElement _root;
    private string _currentScene;
    
    public static SceneUsersOverlay Instance;
    
    public async void UpdateData(string a)
    {
        if (!EditorPrefs.GetBool(SceneSyncSettigns.PREFS_KEY_ENABLED))
        {
            _label.text = $"Disabled";
            return;
        }

        SceneSync.URL = EditorPrefs.GetString(SceneSyncSettigns.PREFS_KEY_URL);
            
        _label.text = $"Pobieranie danych...";
            
        await SceneSync.SendCurrentScene(a);
        SceneSync.PeopleOnCurrentScene = await SceneSync.GetUsers(a);
            
        _label.text = $"People on scene: {SceneSync.PeopleOnCurrentScene.Count} \n";

        foreach (var info in SceneSync.PeopleOnCurrentScene)
        {
            _label.text += info.name + "\n";
        }
    }
    
    public async void UpdateDataWs(string a)
    {
        //Debug.Log("Updating clients from websocket");
        _label.text = $"Pobieranie danych...";
            
        //await SceneSync.SendCurrentScene(a);
        SceneSync.PeopleOnCurrentScene = SceneSync.GetUsersWs(a);
            
        _label.text = $"People on scene: {SceneSync.PeopleOnCurrentScene.Count} \n";

        foreach (var info in SceneSync.PeopleOnCurrentScene)
        {
            _label.text += info.name + "\n";
        }
    }


    public void SetConnectionButtonVisibility(bool enabled)
    {
        if (enabled)
            _root.Add(_refreshButton);
        else
            _refreshButton.RemoveFromHierarchy();
    }
    
    public override VisualElement CreatePanelContent()
    {
        
        Instance = this;
        
        
        _root = new VisualElement();
        
        _refreshButton = new Button();
        _refreshButton.text = "Connect";
        
        _label = new Label("");

        _refreshButton.clicked += () =>
        {
            SceneSync.instance.Init();
            UpdateData(SceneManager.GetActiveScene().name);
            //
            // if (String.IsNullOrEmpty(_currentScene))
            // {
            //     // get active scene
            //     _currentScene = EditorSceneManager.GetActiveScene().name;
            // }
            //
            // UpdateData(_currentScene);
        };
        
        
        _root.Add(_label);
        _root.Add(_refreshButton);

        UpdateData(SceneManager.GetActiveScene().name);
        return _root;
    }
}


[UnityEditor.FilePath("SomeSubFolder/StateFile.foo", FilePathAttribute.Location.PreferencesFolder)]
public class SceneSync : ScriptableSingleton<SceneSync>
{
    public static string URL = "http://localhost:3001";

    private static UserInfo _info;
    private static Task _wsTask;
    private static WebSocket _ws;
    
    public static List<UserInfo> PeopleOnCurrentScene = new List<UserInfo>();

    private void OnEnable()
    {
        Init();
    }

    private void OnDestroy()
    {
        if (_ws != null)
            _ws.Close();
    }


    
    public void Init()
    {
        URL = EditorPrefs.GetString(SceneSyncSettigns.PREFS_KEY_URL);
        
        _info.name = Environment.UserName;
        _info.id = SystemInfo.deviceUniqueIdentifier;
        _info.scene = SceneManager.GetActiveScene().name;
        
        EditorSceneManager.sceneOpened += async (arg0, mode) =>
        {
            await SendCurrentScene(arg0.name);
        };
        
        WebSocketThread();
        
        EditorApplication.quitting += () =>
        {
            LogoutFromServer();
        };
        
        Debug.Log("Scene server initalized! " + GetFilePath());
        
    }
    
    private async void WebSocketThread()
    {
        _ws = new WebSocket(URL.Replace("http", "ws") + "/rt");

        _ws.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            SceneUsersOverlay.Instance.SetConnectionButtonVisibility(false);
        };

        
        _ws.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };
        
        _ws.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            SceneUsersOverlay.Instance.SetConnectionButtonVisibility(true);
        };

        
        _ws.OnMessage += (bytes) =>
        {
            //Debug.Log("OnMessage!");
            //Debug.Log(bytes);

            SceneUsersOverlay.Instance.UpdateDataWs(Encoding.ASCII.GetString(bytes, 0, bytes.Length));
        };
        EditorApplication.update += () => SceneSync.updateWebSocket();
        
        await _ws.Connect();

    }

    
    
    static void updateWebSocket()
    {
        var pos = SceneView.lastActiveSceneView.camera.transform.position;
        
        _ws.DispatchMessageQueue();
    }
    
    private static void LogoutFromServer()
    {
        var uwr = UnityWebRequest.Post(URL + "/logoutUser", JsonUtility.ToJson(_info), "application/json");
        AddAuthHeader(uwr);

        var response = uwr.SendWebRequest();
    }


    public async static UniTask<List<UserInfo>> GetUsers(string scn)
    {
        var uwr = UnityWebRequest.Post(URL + "/getPeopleOnScene", "{\"scene\": \"" + scn + "\"}", "application/json");
        AddAuthHeader(uwr);

        var response = await uwr.SendWebRequest();
        
        
        //Debug.Log(response.downloadHandler.text);
        
        return JsonConvert.DeserializeObject<List<UserInfo>>(response.downloadHandler.text);
    }

    public static async UniTask SendCurrentScene(string scn)
    {
        _info.scene = scn;
        
        var uwr = UnityWebRequest.Post(URL + "/sendUserInfo", JsonUtility.ToJson(_info), "application/json");
        AddAuthHeader(uwr);
        
        
        var response = await uwr.SendWebRequest();
    }

    private static void AddAuthHeader(UnityWebRequest uwr)
    {
        uwr.SetRequestHeader("Authorization", "Basic c3RyYjp4d3deMkw4YSRCSDZVQ0VjWiFIJDNCYzcjTnBvYyV4ZjUzQnRLQyFwaEppN1FXYVdWaGo0VF5L");
    }

    public static List<UserInfo> GetUsersWs(string s)
    {
        var list = JsonConvert.DeserializeObject<List<UserInfo>>(s);
        return list.Where(u => u.scene == _info.scene).ToList();
    }
}

public class SceneSyncSettigns : EditorWindow
{
    private String _url;
    private bool _enabled;

    public static string PREFS_KEY_URL = "scene_sync_url";
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
    }

    private void OnGUI()
    {
        _url = EditorGUILayout.TextField("Server URL", _url);
        _enabled = EditorGUILayout.Toggle("Enabled", _enabled);
        
        
        if (GUILayout.Button("Save"))
            Save();
    }

    public void Save()
    {
        EditorPrefs.SetString(PREFS_KEY_URL, _url);
        EditorPrefs.SetBool(PREFS_KEY_ENABLED, _enabled);
    }
}
