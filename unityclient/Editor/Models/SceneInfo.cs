using System;
using System.Collections.Generic;

namespace UnitySceneSync.Models
{

    public struct MsgWrapper
    {
        public string type;
        public byte[] content;
    }

    public struct Project
    {
        public string id;
        public List<User> users;
        public List<SceneInfo> scenes;
    }

    public struct SceneInfo
    {
        public String name;
        public string lockedBy;
        public bool isLocked;
    }

}