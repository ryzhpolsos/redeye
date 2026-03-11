using System.IO;
using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class WebView : BaseShellWidget {
        WebBrowser webBrowser = new();

        public override void Initialize(){
            Control = webBrowser;
            webBrowser.IsWebBrowserContextMenuEnabled = false;
            webBrowser.WebBrowserShortcutsEnabled = false;
            webBrowser.AllowWebBrowserDrop = false;
            
            webBrowser.ScrollBarsEnabled = ParseHelper.ParseBool(Node.GetAttribute("allowScroll", "false"));

            UtilHelper.IfNotEmpty(Node.GetAttribute("url"), url => {
                webBrowser.Navigate(url);
            });

            UtilHelper.IfNotEmpty(Node.GetAttribute("fileName"), fileName => {
                webBrowser.DocumentText = File.ReadAllText(fileName);
            });

            UtilHelper.IfNotEmpty(Node.GetAttribute("content"), content => {
                webBrowser.DocumentText = content;
            });

            UtilHelper.IfNotEmpty(Node.Value, value => {
                webBrowser.DocumentText = value;
            });

            base.Initialize();
        }
    }
}
