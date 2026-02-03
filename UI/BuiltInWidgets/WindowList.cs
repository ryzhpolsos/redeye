using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using RedEye.Core;

namespace RedEye.UI.BuiltInWidgets {
    public class WindowList : BaseShellWidget {
        FlowLayoutPanel flowLayoutPanel = new();
        ConfigNode elementTemplate = null;
        Dictionary<IntPtr, IShellWidget> widgets = new();

        ILogger logger = null;
        ILayoutLoader layoutLoader = null;
        IResourceManager resourceManager = null;
        IShellEventListener shellEventListener = null;

        public override void Initialize(){
            flowLayoutPanel.FlowDirection = Node.GetAttribute("direction", "horizontal") == "horizontal" ? FlowDirection.LeftToRight : FlowDirection.TopDown;
            Control = flowLayoutPanel;

            elementTemplate = Node.GetNodes().First();
            
            logger = ComponentManager.GetComponent<ILogger>();
            layoutLoader = ComponentManager.GetComponent<ILayoutLoader>();
            resourceManager = ComponentManager.GetComponent<IResourceManager>();
            shellEventListener = ComponentManager.GetComponent<IShellEventListener>();

            shellEventListener.RegisterEventHandler(ShellEvent);
        }

        void PrepareElementNode(ConfigNode node, ShellWindowState windowState){
            node.SetVariable("window.handle", windowState.Handle.ToString());
            node.SetVariable("window.isMinimized", windowState.IsMinimized.ToString());
            node.SetVariable("window.isActive", windowState.IsActive.ToString());
            node.SetVariable("window.showCmd", windowState.ShowCmd.ToString());
            node.SetVariable("window.title", windowState.Title);
            
            try{
                node.SetVariable("window.icon", resourceManager.AddResource(windowState.Icon.ToBitmap()));
            }catch(Exception exc){
                MessageBox.Show($"{exc.GetType().FullName}: {exc.Message} | {windowState.Title}");
            }
        }

        void ShellEvent(ShellWindowEvent windowEvent, ShellWindowState windowState){
            ThreadSafeInvoke(() => {
                // Console.WriteLine($"{windowEvent} on {windowState.Title}");

                try{
                    switch(windowEvent){
                        case ShellWindowEvent.Create: {
                            var node = elementTemplate.Clone();
                            node.SetParentNode(Node);
                            PrepareElementNode(node, windowState);
                            var widget = layoutLoader.CreateWidgetFromNode(node, null, false);
                            widget.UpdateConfig();

                            if(widget.GetControl() is not null) flowLayoutPanel.Controls.Add(widget.GetControl());
                            widgets.Add(windowState.Handle, widget);

                            break;
                        }

                        case ShellWindowEvent.Destroy: {
                            var widget = widgets[windowState.Handle];
                            flowLayoutPanel.Controls.Remove(widget.GetControl());
                            widgets.Remove(windowState.Handle);

                            break;
                        }

                        case ShellWindowEvent.LayoutChange: {
                            break;
                        }

                        default: {
                            var widget = widgets[windowState.Handle];
                            PrepareElementNode(widget.GetNode(), windowState);
                            widget.UpdateConfig();
                            widget.UpdateControl();

                            break;
                        }
                    }
                }catch(Exception ex){
                    logger.LogError($"WindowList [{windowEvent}, {windowState.Title}] Error: {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}\n===");
                }
            });
        }
    }
}
