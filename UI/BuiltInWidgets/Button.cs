using System.Drawing;
using System.Windows.Forms;

namespace RedEye.UI.BuiltInWidgets {
    public class Button : BaseShellWidget {
        System.Windows.Forms.Button button = new();

        public override void Initialize(){
            Control = button;
        }

        protected override void UpdateControlInternal(){
            button.Text = Node.GetAttribute("text");

            if(Node.GetAttribute("border") == "none"){
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
                
                button.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml(Node.GetAttribute("hoverColor"));
                button.FlatAppearance.MouseDownBackColor = ColorTranslator.FromHtml(Node.GetAttribute("pressedColor"));
            }

            base.UpdateControlInternal();
        }
    }
}
