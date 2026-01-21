
namespace RedEye.Core {
    public interface IWmxManager : IComponent {
        public void SetWorkArea(int x, int y, int width, int height);
        public string GetResponseMessage();
        public void Exit();
    }

    public enum WmxParam {
        X,
        Y,
        Width,
        Height
    }

    public enum WmxResponse {
        Lang
    }
}
