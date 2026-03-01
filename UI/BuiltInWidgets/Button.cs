using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class Button : BaseShellWidget {
        System.Windows.Forms.Button button = new();

        public override void Initialize(){
            Control = button;
        }

        protected override void UpdateControlInternal(){
            button.Text = Node.GetAttribute("text");
            base.UpdateControlInternal();
        }
    }
}
