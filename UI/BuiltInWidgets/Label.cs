using System.Drawing;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class Label : BaseShellWidget {
        System.Windows.Forms.Label label;

        public Label() : base(){
            label = new();
        }

        public override void Initialize(){
            Control = label;

            label.TextAlign = ParseHelper.ParseEnum<ContentAlignment>(Node.GetAttribute("align", "topLeft"));
        }

        protected override void UpdateControlInternal(){
            label.Text = Node.GetAttribute("text");
            base.UpdateControlInternal();
        }
    }
}
