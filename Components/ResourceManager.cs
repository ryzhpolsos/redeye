using System;
using System.Collections.Generic;

using RedEye.Core;

namespace RedEye.Components {
    public class ResourceManagerComponent : IResourceManager {
        Dictionary<string, object> resources = new();

        public void SetManager(ComponentManager manager){}
        public void Initialize(){}

        public string AddResource(object resource){
            var guid = Guid.NewGuid().ToString();
            resources.Add(guid, resource);
            return guid;
        }

        public void RemoveResource(string id){
            resources.Remove(id);
        }

        public object GetResource(string id){
            return resources[id];
        }

        public T GetResource<T>(string id){
            return (T)GetResource(id);
        }
    }
}
