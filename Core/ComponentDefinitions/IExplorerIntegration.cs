
namespace RedEye.Core {
    public interface IExplorerIntegration : IComponent {
        public void RunHiddenExplorer();
        public bool GetIsEnabled();
        public int GetExplorerPID();
    }
}
