using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Scene sync")]
public class SceneUsersOverlay : Overlay
{
    Label _label;
    private Button _connectButton;
    private VisualElement _root;
    private string _currentScene;
    
    public static SceneUsersOverlay Instance;
    
    public async void UpdateOverlayData(string a)
    {
        if (!EditorPrefs.GetBool(SceneSyncSettigns.PREFS_KEY_ENABLED))
        {
            _label.text = $"Disabled";
            return;
        }

        SceneSync.URL = EditorPrefs.GetString(SceneSyncSettigns.PREFS_KEY_URL);
            
        _label.text = $"Pobieranie danych...";
            
        // await SceneSync.SendCurrentScene(a);
        // SceneSync.PeopleOnCurrentScene = await SceneSync.GetUsers(a);
            
        _label.text = $"People on scene: {SceneSync.PeopleOnCurrentScene.Count} \n";

        foreach (var info in SceneSync.PeopleOnCurrentScene)
        {
            _label.text += info.name + "\n";
        }
    }
    
    public async void UpdateUserList(List<UserInfo> users)
    {
        //Debug.Log("Updating clients from websocket");
        _label.text = $"Pobieranie danych...";
            
        //await SceneSync.SendCurrentScene(a);
            
        _label.text = $"People on scene: {users.Count} \n";

        foreach (var info in users)
        {
            //                                        don't add newline for the last user
            _label.text += info.name + (users.IndexOf(info) == users.Count - 1 ? "" : "\n");
        }
    }


    public void SetConnectionButtonVisibility(bool enabled)
    {
        if (enabled)
            _root.Add(_connectButton);
        else
            _connectButton.RemoveFromHierarchy();
    }
    
    public override VisualElement CreatePanelContent()
    {
        
        Instance = this;
        
        
        _root = new VisualElement();
        
        _connectButton = new Button();
        _connectButton.text = "Connect";
        
        _label = new Label("");

        _connectButton.clicked += () =>
        {
            SceneSync.instance.TryConnect();
        };
        
        _root.Add(_label);
        _root.Add(_connectButton);

        return _root;
    }

    public void ClearList()
    {
        _label.text = "";
    }
}