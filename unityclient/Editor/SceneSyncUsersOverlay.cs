using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
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

    public static SceneUsersOverlay Instance;

    public override void OnCreated()
    {
        base.OnCreated();

        SceneSync.instance.TryConnect();
    }

    private List<User> _filteredUsers = new List<User>();

    public async void UpdateProjectInfo(SceneInfo scene, User[] users, bool isSceneLocked, bool isSceneLockedByMe)
    {
        //Debug.Log("Updating clients from websocket");
        _userListLabel.text = $"Pobieranie danych...";
        _userListLabel.text = $"Users on current scene: \n";

        _filteredUsers = users.ToList();
        _isCurrentSceneLockedByMe = isSceneLockedByMe;
        _isCurrentSceneLocked = isSceneLocked;

        if (_isCurrentSceneLocked && !_isCurrentSceneLockedByMe)
        {
            // scene locked by someone else

            var lockingUser = users.FirstOrDefault(x => x.id == scene.lockedBy);
            if (string.IsNullOrEmpty(lockingUser.id))
            {
                User u = new User();
                u.name = scene.lockedByName;
                u.id = scene.lockedBy;
                u.time = -1;
                _filteredUsers.Add(u);
            }

            _lockUnlockButton.SetEnabled(false);
        }
        else
        {
            _lockUnlockButton.SetEnabled(true);

            if (_isCurrentSceneLockedByMe)
                _lockUnlockButton.text = "Unlock";
            else
                _lockUnlockButton.text = "Lock";
        }

        _userListView.itemsSource = _filteredUsers;

        var inst = SceneSync.instance;
        _userListView.bindItem = (element, i) =>
        {
            // Bind the username to the label
            var nameLabel = element.Q<Label>();
            var lockIcon = element.Q<Image>();

            nameLabel.text = _filteredUsers[i].name;

            if (_filteredUsers[i].time == -1)
                nameLabel.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
            else
                nameLabel.style.color = new StyleColor(new Color(1, 1, 1));

            lockIcon.style.display = _filteredUsers[i].id == scene.lockedBy ? DisplayStyle.Flex : DisplayStyle.None;
        };

        _userListView.RefreshItems();
        _userListView.Rebuild();
    }


    public void SetConnectionButtonVisibility(bool enabled)
    {
        if (enabled)
        {
            _lockUnlockButton.style.display = DisplayStyle.None;
            _root.Add(_connectButton);
        }
        else
        {
            _lockUnlockButton.style.display = DisplayStyle.Flex;
            _connectButton.RemoveFromHierarchy();
        }
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
        _lockUnlockButton.style.display = DisplayStyle.None;

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