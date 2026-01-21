using System;
using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI {
    public interface IShellWidget : IComponent {
        public void PostInitialize();
        public ConfigNode GetNode();
        public void SetNode(ConfigNode node);
        public ShellWidgetConfig GetConfig();
        public void SetConfig(ShellWidgetConfig config);
        public void UpdateConfig();
        public IContainerWidget GetContainer();
        public void SetContainer(IContainerWidget container);
        public IShellWindow GetWindow();
        public void SetWindow(IShellWindow window);
        public Control GetControl();
        public void UpdateControl();
        public void RegisterEventHandler(string name, Action<ShellWidgetEvent> handler);
    }
}
