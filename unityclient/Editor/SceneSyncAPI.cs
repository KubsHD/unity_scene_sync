using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
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
        uwr.SetRequestHeader("Authorization", "Basic c3RyYjp4d3deMkw4YSRCSDZVQ0VjWiFIJDNCYzcjTnBvYyV4ZjUzQnRLQyFwaEppN1FXYVdWaGo0VF5L");
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