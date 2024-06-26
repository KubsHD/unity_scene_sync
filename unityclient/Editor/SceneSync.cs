using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
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


[UnityEditor.FilePath("STRB/SceneSync.state", FilePathAttribute.Location.PreferencesFolder)]
[InitializeOnLoad]
public class SceneSync : ScriptableSingleton<SceneSync>
{
    public static string URL = "http://localhost:3001";

    private static UserInfo _info;
    private static Task _wsTask;
    private static WebSocket _ws;

    private static SceneSyncAPI _api;
    
    public static List<UserInfo> PeopleOnCurrentScene = new List<UserInfo>();


    private void OnEnable()
    {
        TryConnect();
    }

    private void OnDestroy()
    {
        if (_ws != null)
            _ws.Close();
    }

    public void TryConnect()
    {
        URL = EditorPrefs.GetString(SceneSyncSettigns.PREFS_KEY_URL);

        _api = new SceneSyncAPI(URL, PlayerSettings.productName);

    
        
        if (!_api.Ping())
        {
            Debug.LogError("Couldn't connect to server!");
            return;
        }

        _info.name = Environment.UserName;
        _info.id = SystemInfo.deviceUniqueIdentifier;
        _info.scene = SceneManager.GetActiveScene().name;
        
        // setup unity callbacks
        EditorSceneManager.sceneOpened += async (arg0, mode) =>
        {
            _info.scene = arg0.name;
            await _api.SendCurrentScene(_info);
        };
        
        EditorApplication.quitting += () =>
        {
            _api.LogoutFromServer(_info);
        };
        
        WebSocketThread();
        
        Debug.Log("Scene sync initalized! ");

        initialDataSend();
    }
    
    private async void initialDataSend()
    {
        await UniTask.WaitUntil(() => _ws.State == WebSocketState.Open);

        _info.scene = SceneManager.GetActiveScene().name;
        _ = _api.SendCurrentScene(_info);
    }

    async void WebsocketHeartbeat()
    {
        HeartbeatMessage msg;
        msg.type = "heartbeat";
        msg.content = DateTime.Now.ToUniversalTime().Ticks.ToString();

        var bytes = System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg).ToString());

        while (_ws.State == WebSocketState.Open)
        {
            await UniTask.WaitForSeconds(20);
            await _ws.Send(bytes);
        }
    }

    private async void WebSocketThread()
    {
        var websocketUrl = URL.Replace("http", "ws") + "/api/scene/ws";
        Debug.Log(websocketUrl);

        _ws = new WebSocket(websocketUrl, new Dictionary<string, string>
        {
            {"key", EditorPrefs.GetString(SceneSyncSettigns.PREFS_KEY_SECRET)}
        });

        _ws.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            SceneUsersOverlay.Instance.SetConnectionButtonVisibility(false);
            WebsocketHeartbeat();
        };

        
        _ws.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };
        
        _ws.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            
            
            SceneUsersOverlay.Instance.ClearList();
            SceneUsersOverlay.Instance.SetConnectionButtonVisibility(true);
        };

        
        _ws.OnMessage += (bytes) =>
        {
            var userList = SceneSync.DeserializeUserInfos(Encoding.ASCII.GetString(bytes, 0, bytes.Length));
            PeopleOnCurrentScene = userList;
            SceneUsersOverlay.Instance.UpdateUserList(userList);
        };
        
        EditorApplication.update += () => SceneSync.updateWebSocket();
        
        await _ws.Connect();

    }

    
    
    static void updateWebSocket()
    {
        var pos = SceneView.lastActiveSceneView.camera.transform.position;
        
        _ws.DispatchMessageQueue();
    }
    


    public static List<UserInfo> DeserializeUserInfos(string jsonString)
    {
        var list = JsonConvert.DeserializeObject<List<UserInfo>>(jsonString);
        return list.Where(u => u.scene == _info.scene).ToList();
    }
}



