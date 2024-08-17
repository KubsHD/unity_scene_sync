using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnitySceneSync.Models;

[Overlay(typeof(SceneView), "Users on scene")]
public class SceneUsersOverlay : Overlay
{
    private VisualElement _root;

    Label _userListLabel;
    private Button _connectButton;
    private Button _lockUnlockButton;
    private ListView _userListView;
    
    private bool _isCurrentSceneLocked;
    private bool _isCurrentSceneLockedByMe;
    
    private string _currentScene;
    
    private Dictionary<string, bool> _userMappings = new Dictionary<string, bool>();
    
    public static SceneUsersOverlay Instance;

    public override void OnCreated()
    {
        base.OnCreated();

        SceneSync.instance.TryConnect();
    }
    
    public async void UpdateProjectInfo(Project project)
    {
        //Debug.Log("Updating clients from websocket");
        _userListLabel.text = $"Pobieranie danych...";
            
        //await SceneSync.SendCurrentScene(a);
            
        _userListLabel.text = $"Users on current scene: \n";

        _userListView.itemsSource = project.users;

        foreach (var user in project.users)
        {
            _userMappings[user.name] = project.scenes.Exists(x => x.lockedBy == user.name && x.name == SceneManager.GetActiveScene().name);
        }
        
        _isCurrentSceneLockedByMe = SceneSync.instance.CheckIfUserLockedCurrentScene(SceneSync.instance.GetUsername());
        
        if (_isCurrentSceneLockedByMe)
        {
            _lockUnlockButton.text = "Unlock";
        }
        else
        {
            _lockUnlockButton.text = "Lock";
        }
        
        var inst = SceneSync.instance;
        _userListView.bindItem = (element, i) =>
        {
            // Bind the username to the label
            var nameLabel = element.Q<Label>();
            var lockIcon = element.Q<Image>();


            nameLabel.text = _userMappings.ElementAt(i).Key;
            lockIcon.style.display = _userMappings.ElementAt(i).Value ? DisplayStyle.Flex : DisplayStyle.None;
        };

        _userListView.RefreshItems();
        _userListView.Rebuild();
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

        _root.style.height = new StyleLength(StyleKeyword.Auto);
        _root.style.justifyContent = Justify.SpaceBetween;
        
        _connectButton = new Button();
        _connectButton.text = "Connect";
        
        _lockUnlockButton = new Button();
        _lockUnlockButton.text = "Lock";
        _lockUnlockButton.SetEnabled(true);

        _lockUnlockButton.clicked += () =>
        {
            if (_isCurrentSceneLockedByMe)
            {
                SceneSync.instance.TryUnlockScene();
            }
            else
            {
                SceneSync.instance.TryLockScene();
            }
        };
        
        _userListLabel = new Label("");
        _connectButton.clicked += () =>
        {
            SceneSync.instance.TryConnect();
        };
        
        
        _userListView = new ListView();
        
        _userListView.selectionType = SelectionType.None;
        _userListView.style.flexGrow = 0.0f;
        _userListView.style.maxHeight = 100;
        
        _userListView.makeItem = () =>
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.alignItems = Align.Center;

            // Create a label for the username
            Label nameLabel = new Label();
            nameLabel.style.flexGrow = 1;
            container.Add(nameLabel);

            
            var tex = EditorGUIUtility.IconContent("P4_LockedRemote").image;
            
            // Create the lock icon
            VisualElement lockIcon = new Image()
            {
                image = tex
            };
            lockIcon.style.width = 16;
            lockIcon.style.height = 16;
            container.Add(lockIcon);

            return container;
        };
        
 
        _root.Add(_userListLabel);
        _root.Add(_userListView);
        _root.Add(_lockUnlockButton);
        _root.Add(_connectButton);

        return _root;
    }

    public void ClearList()
    {
        _userListLabel.text = "";
    }
}