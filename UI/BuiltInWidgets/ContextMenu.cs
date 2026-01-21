using System;
using System.Windows.Forms;

namespace RedEye.UI.BuiltInWidgets {
    public class ContextMenu : BaseShellWidget {
        ContextMenuStrip contextMenuStrip = new();

        public override void PostInitialize(){
            Control = null;

            foreach(var itemNode in Node.GetNodes("item")){
                contextMenuStrip.Items.Add(itemNode.Value).Click += (s, e) => {
                    itemNode.GetAttribute("action");
                };
            }

            var parent = (Container is null ? (IWidgetContainer)Window : (IWidgetContainer)Container);

            foreach(var wid in parent.GetWidgets()){
                // Console.WriteLine(wid.Key);
            }

            parent.GetWidget(Node.GetAttribute("for")).GetControl().ContextMenuStrip = contextMenuStrip;

            base.PostInitialize();
        }
    }
}
