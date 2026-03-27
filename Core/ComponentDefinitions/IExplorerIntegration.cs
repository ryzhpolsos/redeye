
namespace RedEye.Core {
    public interface IExplorerIntegration : IComponent {
        public void RunHiddenExplorer();
        public bool GetIsEnabled();
        public void SetIsEnabled(bool enabled);
        public void SetTimeout(int timeout);
        public int GetExplorerPID();
    }
}
