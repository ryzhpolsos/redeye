using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class TextBox : BaseShellWidget {
        System.Windows.Forms.TextBox textBox = new();

        public override void Initialize(){
            Control = textBox;
            
            textBox.TextChanged += (_, _) => {
                if(Container is not null){
                    Container.GetNode().SetVariable(Node.GetAttribute("output"), textBox.Text);
                }else{
                    Node.RootNode.SetVariable(Node.GetAttribute("output"), textBox.Text);
                }
            };

            base.Initialize();
        }

        protected override void UpdateControlInternal(){
            textBox.Multiline = ParseHelper.ParseBool(Node.GetAttribute("multiLine", "false"));
            base.UpdateControlInternal();
        }
    }
}
