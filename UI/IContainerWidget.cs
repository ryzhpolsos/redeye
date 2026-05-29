namespace RedEye.UI {
    public interface IContainerWidget : IShellWidget, IWidgetContainer {
        public object GetWidgetParam(string id, string paramName);
        public void SetWidgetParam(string id, string paramName, object paramValue);
    }
}
