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
    private string _project;

    public SceneSyncAPI(string url, string project)
    {
        _project = project;
        _url = url + "/api/scene/" + _project;
    }
    
    private static void AddAuthHeader(UnityWebRequest uwr)
    {
        uwr.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + EditorPrefs.GetString(SceneSyncSettigns.PREFS_KEY_SECRET))));
    }

    public void LoginToServer(User info)
    {
        var uwr = UnityWebRequest.Post(_url + "/loginUser", JsonUtility.ToJson(info), "application/json");
        AddAuthHeader(uwr);

        var response = uwr.SendWebRequest();
    }
    
    public void LogoutFromServer(User info)
    {
        var uwr = UnityWebRequest.Post(_url + "/logoutUser", JsonUtility.ToJson(info), "application/json");
        AddAuthHeader(uwr);

        var response = uwr.SendWebRequest();
    }


    public async UniTask<List<User>> GetUsers(string scn)
    {
        var uwr = UnityWebRequest.Post(_url + "/getPeopleOnScene", "{\"scene\": \"" + scn + "\"}", "application/json");
        AddAuthHeader(uwr);

        var response = await uwr.SendWebRequest();
        
        
        //Debug.Log(response.downloadHandler.text);
        
        return JsonConvert.DeserializeObject<List<User>>(response.downloadHandler.text);
    }

    public async UniTask SendCurrentScene(User info)
    {
        
        var uwr = UnityWebRequest.Post(_url + "/updateUser", JsonUtility.ToJson(info), "application/json");
        AddAuthHeader(uwr);
        
        
        var response = await uwr.SendWebRequest();
    }

    public bool Ping()
    {
        var uwr = UnityWebRequest.PostWwwForm(_url, "");
        AddAuthHeader(uwr);
        
        var response = uwr.SendWebRequest();

        while (!response.isDone) {}

        return response.webRequest.responseCode != 401;
    }
}