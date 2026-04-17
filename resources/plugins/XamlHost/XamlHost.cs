using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Interop;
using System.Windows.Forms.Integration;

using RedEye.UI;
using RedEye.PluginAPI;

public class XamlHostWidget : BaseShellWidget {
    ElementHost elementHost = new ElementHost();

    public override void Initialize(){
        Control = elementHost;
    }

    public override void PostInitialize(){
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        
        using(var stream = new StringReader(Node.Value)){
            using(var reader = XmlReader.Create(stream)){
                elementHost.Child = (UIElement)XamlReader.Load(reader);
            }
        }

        base.PostInitialize();
    }
}

public class XamlHostPlugin : Plugin {
    public override void Main(){
        ExportWidget<XamlHostWidget>("xamlHost");
    }
}
