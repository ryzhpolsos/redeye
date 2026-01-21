using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class FlowPanel : BaseContainerWidget {
        FlowLayoutPanel flowLayoutPanel = null;

        public FlowPanel() : base() {
            flowLayoutPanel = new();
        }

        protected override void UpdateControlInternal(){
            Control = flowLayoutPanel;

            flowLayoutPanel.WrapContents = ParseHelper.ParseBool(Node.GetAttribute("wrap", "false"));
            flowLayoutPanel.FlowDirection = ParseHelper.ParseEnum<FlowDirection>(Node.GetAttribute("direction", "leftToRight"));

            base.UpdateControlInternal();
        }
    }
}
