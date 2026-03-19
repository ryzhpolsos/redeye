using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class TextBox : BaseShellWidget {
        System.Windows.Forms.TextBox textBox = new();
        ConfigNode rootNode = null;

        void UpdateVariable(){
            rootNode.SetVariable(Node.GetAttribute("output"), textBox.Text);
        }

        public override void Initialize(){
            rootNode = GetNode().RootNode;
            Control = textBox;
            
            textBox.TextChanged += (_, _) => {
                UpdateVariable();
            };

            textBox.Text = Node.GetAttribute("text");

            base.Initialize();
        }

        protected override void UpdateControlInternal(){
            UpdateVariable();
            textBox.Multiline = ParseHelper.ParseBool(Node.GetAttribute("multiLine", "false"));
            base.UpdateControlInternal();
        }
    }
}
