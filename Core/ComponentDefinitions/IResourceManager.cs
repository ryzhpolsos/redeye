using System;

namespace RedEye.Core {
    public interface IResourceManager : IComponent {
        public string AddResource(object resource);
        public void RemoveResource(string id);
        public object GetResource(string id);
        public T GetResource<T>(string id);
    }
}