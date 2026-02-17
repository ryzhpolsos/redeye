using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class FlowPanel : BaseContainerWidget {
        FlowLayoutPanel flowLayoutPanel = new();

//         public FlowPanel() : base() {
//             flowLayoutPanel = new();
//         }

        public FlowPanel() : base(){
            Control = flowLayoutPanel;
        }

        protected override void UpdateControlInternal(){
            flowLayoutPanel.WrapContents = ParseHelper.ParseBool(Node.GetAttribute("wrap", "false"));
            flowLayoutPanel.FlowDirection = ParseHelper.ParseEnum<FlowDirection>(Node.GetAttribute("direction", "leftToRight"));

            base.UpdateControlInternal();
        }
    }
}
