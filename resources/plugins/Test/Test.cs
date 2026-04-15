using RedEye.UI;
using RedEye.Core;
using RedEye.PluginAPI;

class Test : Plugin {
    public override string Name {
        get {
            return "TestPlugin";
        }
    }

    public override void Main(){
        ExportFunction("test", (_, __) => {
            var node = Config.CreateNodeFromString("<window x=\"100\" y=\"100\" width=\"300\" height=\"300\"/>");
            var window = LayoutLoader.CreateWindowFromNode(node);
            window.ShowWindow();

            return string.Empty;
        });
    }
}
