using RedEye.UI;

namespace RedEye.Core {
    public interface ILayoutLoader : IComponent {
        public IShellWindow CreateWindowFromNode(ConfigNode node);
        public IShellWidget CreateWidgetFromNode(ConfigNode widgetNode, IShellWindow window = null, bool addToForm = true, bool init = true);
    }
}
