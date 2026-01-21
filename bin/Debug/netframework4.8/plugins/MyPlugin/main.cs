using System;
using System.Drawing;
using System.Windows.Forms;

using RedEye.UI;
using RedEye.Core;
using RedEye.PluginAPI;

class MyWidget : BaseShellWidget {
    Label label;

    public MyWidget() : base(){
        label = new Label();
    }

    public override void Initialize(){
        label.BackColor = ColorTranslator.FromHtml(Node.GetAttribute("color", "black"));
        Control = label;
    }

    protected override void UpdateControlInternal(){
        label.Text = Node.GetAttribute("text");
        base.UpdateControlInternal();
    }
}

class MyPlugin : Plugin {
    public override string Name {
        get {
            return "myPlugin";
        }
    }

    public override void Main(){
        ExportWidget<MyWidget>("myWidget");
    }
}
