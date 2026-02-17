namespace RedEye.Core {
    public interface IConfig : IComponent {
        public IConfig LoadConfig();
        public string GetAppDirectory();
        public string GetPath(params string[] relativePath);
        public ConfigNode GetRootNode();
        public ConfigNode GetLayoutNode();
        public void LoadFile(string fileName, ConfigNode parentNode);
        public IConfig ExecuteScripts();
    }
}
