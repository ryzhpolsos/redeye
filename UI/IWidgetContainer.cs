using System.Collections.Generic;

namespace RedEye.UI {
    public interface IWidgetContainer {
        public IDictionary<string, IShellWidget> GetWidgets();
        public IShellWidget GetWidget(string id);
        public void AddWidget(IShellWidget widget, bool addToControl = true);
        public void RemoveWidget(IShellWidget widget);
    }
}
