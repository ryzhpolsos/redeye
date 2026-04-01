using System.Runtime.InteropServices;

namespace RedEye.Core {
    [ComVisible(true)]
    public interface IConfig : IComponent {
        public IConfig LoadConfig();
        public string GetAppDirectory();
        public string GetPath(params string[] relativePath);
        public ConfigNode GetRootNode();
        public ConfigNode GetLayoutNode();
        public void LoadFile(string fileName, ConfigNode parentNode);
        public void LoadString(string data, ConfigNode parentNode);
        public ConfigNode CreateNode(string name);
        public ConfigNode CreateNodeFromString(string data);
    }
}
