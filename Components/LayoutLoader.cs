using System;
using RedEye.Core;
using RedEye.UI;

namespace RedEye.Components {
    public class LayoutLoaderComponent : ILayoutLoader {
        ComponentManager manager = null;

        ILogger logger = null;
        IPluginManager pluginManager = null;
        IShellWindowManager windowManager = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            logger = manager.GetComponent<ILogger>();
            pluginManager = manager.GetComponent<IPluginManager>();
            windowManager = manager.GetComponent<IShellWindowManager>();
        }

        public IShellWindow CreateWindowFromNode(ConfigNode node){
            ShellWindowConfig config = new();
            config.AutoShow = ParseHelper.ParseBool(node.GetAttribute("autoShow", "true"));
            config.Id = node.GetAttribute("id");
            config.Title = node.GetAttribute("title");
            config.X = ParseHelper.ParseInt(node.GetAttribute("x"));
            config.Y = ParseHelper.ParseInt(node.GetAttribute("y"));
            config.Width = ParseHelper.ParseInt(node.GetAttribute("width"));
            config.Height = ParseHelper.ParseInt(node.GetAttribute("height"));
            config.IsTransparent = ParseHelper.ParseBool(node.GetAttribute("transparent"));
            config.Type = ParseHelper.ParseEnum<ShellWindowType>(node.GetAttribute("type"), ShellWindowType.Normal);
            config.BorderType = ParseHelper.ParseEnum<ShellWindowBorderType>(node.GetAttribute("border"), ShellWindowBorderType.Normal);
            config.MinimizeButton = ParseHelper.ParseBool(node.GetAttribute("minimizeButton", "true"));
            config.MaximizeButton = ParseHelper.ParseBool(node.GetAttribute("maximizeButton", "true"));
            config.AllowClose = ParseHelper.ParseBool(node.GetAttribute("allowClose", "true"));
            config.AutoSize = ParseHelper.ParseBool(node.GetAttribute("autoSize"));
            config.Padding = node.GetAttribute("padding");
            config.Color = node.GetAttribute("color");
            config.BackgroundColor = node.GetAttribute("backgroundColor");

            var window = windowManager.CreateWindow(config);

            foreach(var widgetNode in node.GetNodes()){
                CreateWidgetFromNode(widgetNode, window);
            }

            return window;
        }

        public IShellWidget CreateWidgetFromNode(ConfigNode widgetNode, IShellWindow window = null, bool addToForm = true, bool init = true){
            var exportedWidgets = pluginManager.GetExportedWidgets();

            if(!exportedWidgets.ContainsKey(widgetNode.Name)){
                logger.LogFatal($"No widget with name \"{widgetNode.Name}\" was found");
                return null;
            }

            var widget = (IShellWidget)Activator.CreateInstance(exportedWidgets[widgetNode.Name]);
            widget.SetManager(manager);
            widget.SetConfig(new());
            widget.SetNode(widgetNode);
            widget.UpdateConfig();
            widget.Initialize();
            widget.UpdateControl();
            BuildContainerTree(widgetNode, window, widget);
            if(window is not null) window.AddWidget(widget, addToForm);

            if(init) widget.PostInitialize();
            return widget;
        }

        void BuildContainerTree(ConfigNode node, IShellWindow window, IShellWidget parent){
            if(parent is not IContainerWidget) return;
            var container = (IContainerWidget)parent;

            foreach(var widgetNode in node.GetNodes()){
                var widget = CreateWidgetFromNode(widgetNode, window, false, false);
                container.AddWidget(widget);

                var id = widgetNode.GetAttribute("id");

                foreach(var attr in widgetNode.GetAttributes()){
                    if(!attr.StartsWith("control.")) continue;
                    var paramName = attr.Substring("control.".Length);

                    container.SetWidgetParam(id, paramName, widgetNode.GetAttribute(attr));
                }

                widget.PostInitialize();
            }
        }

        void ApplyNETProperties(ConfigNode node, IShellWidget widget){
            var type = widget.GetControl().GetType();

            // foreach(var attr in node.GetAttributes()){
            //     if(!attr.StartsWith("control.")) continue;
            //     attr = attr.Substring("control.".Length);


            // }
        }
    }
}
