using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.Networking;

public class SceneSyncAPI
{
    private string _url;

    public SceneSyncAPI(string url)
    {
        _url = url;
    }
    
    private static void AddAuthHeader(UnityWebRequest uwr)
    {
        uwr.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + EditorPrefs.GetString(SceneSyncSettigns.PREFS_KEY_SECRET))));
    }

    
    public void LogoutFromServer(UserInfo info)
    {
        var uwr = UnityWebRequest.Post(_url + "/logoutUser", JsonUtility.ToJson(info), "application/json");
        AddAuthHeader(uwr);

        var response = uwr.SendWebRequest();
    }


    public async UniTask<List<UserInfo>> GetUsers(string scn)
    {
        var uwr = UnityWebRequest.Post(_url + "/getPeopleOnScene", "{\"scene\": \"" + scn + "\"}", "application/json");
        AddAuthHeader(uwr);

        var response = await uwr.SendWebRequest();
        
        
        //Debug.Log(response.downloadHandler.text);
        
        return JsonConvert.DeserializeObject<List<UserInfo>>(response.downloadHandler.text);
    }

    public async UniTask SendCurrentScene(UserInfo info)
    {
        
        var uwr = UnityWebRequest.Post(_url + "/sendUserInfo", JsonUtility.ToJson(info), "application/json");
        AddAuthHeader(uwr);
        
        
        var response = await uwr.SendWebRequest();
    }

    public bool Ping()
    {
        var uwr = UnityWebRequest.PostWwwForm(_url + "/ping", "");
        AddAuthHeader(uwr);
        
        var response = uwr.SendWebRequest();

        while (!response.isDone) {}

        return response.webRequest.responseCode == 200 ? true : false;
    }
}