using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class AppList : BaseShellWidget {
        TableLayoutPanel tableLayoutPanel = new();

        ILayoutLoader layoutLoader = null;
        IResourceManager resourceManager = null;
        ISpecialFolderWrapper specialFolderWrapper = null;

        public override void Initialize(){
            base.Initialize();

            layoutLoader = ComponentManager.GetComponent<ILayoutLoader>();
            resourceManager = ComponentManager.GetComponent<IResourceManager>();
            specialFolderWrapper = ComponentManager.GetComponent<ISpecialFolderWrapper>();

            Control = tableLayoutPanel;
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.AutoScroll = true;
            tableLayoutPanel.ColumnCount = 1;
            
            var elementTemplate = Node.GetNodes().First();

            int i = 0;
            foreach(var app in specialFolderWrapper.GetApplicationList()){
                var node = elementTemplate.Clone();
                node.SetParentNode(Node);
                node.SetVariable("app.name", app.GetName());
                node.SetVariable("app.command", app.GetCommand());
                node.SetVariable("app.icon", resourceManager.AddResource(app.GetIcon().ToBitmap()));

                var widget = layoutLoader.CreateWidgetFromNode(node, null, false);
                widget.UpdateConfig();

                if(widget.GetControl() is var control){
                    tableLayoutPanel.Controls.Add(control);
                    tableLayoutPanel.SetCellPosition(control, new(0, i));
                }

                i++;
            }

            tableLayoutPanel.ResumeLayout();
        }
    }
}
