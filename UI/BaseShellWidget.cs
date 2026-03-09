using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using RedEye.Core;

namespace RedEye.UI {
    public class BaseShellWidget : IShellWidget {
        protected ShellWidgetConfig Config = null;
        protected Dictionary<string, List<Action<ShellWidgetEvent>>> EventMap = new();

        protected Control Control = null;
        protected ConfigNode Node = null;
        protected IShellWindow Window = null;
        protected IContainerWidget Container = null;
        protected ComponentManager ComponentManager = null;

        private System.Windows.Forms.ToolTip toolTip = null;
        private IResourceManager resourceManager = null;
        private List<string> processedEvents = new();

        public void SetManager(ComponentManager manager){
            ComponentManager = manager;
        }

        public virtual void Initialize(){
            toolTip = new();
            toolTip.ShowAlways = true;
            toolTip.AutomaticDelay = 500;

            if(Config.UpdateInterval > 0){
                Task.Run(() => {
                    while(true){
                        UpdateConfig();
                        UpdateControl();
                        Thread.Sleep(Config.UpdateInterval);
                    }
                });
            }
        }

        public virtual void PostInitialize(){
            if(Control is null) return;

            if(!string.IsNullOrEmpty(Config.ToolTip)){
                Control.MouseHover += (_, _) => {
                    toolTip.Show(Config.ToolTip, Control, Control.Width / 2, Control.Height / 2);
                };

                Control.MouseLeave += (_, _) => {
                    toolTip.Hide(Control);
                };
            }
        }

        public ConfigNode GetNode(){
            return Node;
        }

        public void SetNode(ConfigNode node){
            Node = node;
        }

        public Control GetControl(bool update = true){
            if(update) UpdateControl();
            return Control;
        }

        protected virtual void UpdateControlInternal(){
            if(Control is null) return;

            if(resourceManager is null){
                resourceManager = ComponentManager.GetComponent<IResourceManager>();
            }
 
            Control.Location = new Point(Config.X, Config.Y);
            Control.Size = new Size(Config.Width, Config.Height);
            Control.AutoSize = Config.AutoSize;
            Control.Padding = ParseHelper.ParsePadding(Config.Padding);
            Control.Margin = ParseHelper.ParsePadding(Config.Margin);

            UtilHelper.IfNotEmpty(Config.Dock, dock => {
                Control.Dock = ParseHelper.ParseEnum<DockStyle>(Config.Dock, DockStyle.None);
            });

            UtilHelper.IfNotEmpty(Config.Anchor, anchor => {
                Control.Anchor = ParseHelper.ParseEnum<AnchorStyles>(Config.Anchor, AnchorStyles.None);
            });

            UtilHelper.IfNotEmpty(Config.Color, (color) => {
                Control.ForeColor = ColorTranslator.FromHtml(color);
            });

            UtilHelper.IfNotEmpty(Config.BackgroundColor, (color) => {
                Control.BackColor = ColorTranslator.FromHtml(color);
            });

            UtilHelper.IfNotEmpty(Config.Font, (font) => {
                Control.Font = ParseHelper.ParseFont(font);
            });

            var controlType = Control.GetType();

            foreach(var attr in Node.GetAttributes()){
                if(!attr.StartsWith("on") || processedEvents.Contains(attr)) continue;
                processedEvents.Add(attr);
                var eventName = attr.Substring(2);

                if(controlType.GetEvent(eventName) is var @event && @event is not null){
                    try{
                        @event.GetAddMethod().Invoke(Control, new object[]{
                            DynamicDelegateHelper.GetDelegate((data, args) => {
                                var resIds = new string[args.Length];

                                for(int i = 0; i < args.Length; i++){
                                    var id = resourceManager.AddResource(args[i]);
                                    Node.SetVariable("arg" + i.ToString(), id);
                                    resIds[i] = id;
                                }

                                Node.GetAttribute(attr);

                                foreach(var id in resIds){
                                    resourceManager.RemoveResource(id);
                                }
                            }, attr, @event.EventHandlerType)
                        });
                    }catch(Exception ex){
                        ComponentManager.GetComponent<ILogger>().LogError($"{ex.GetType().FullName}: {ex.Message} | {ex.StackTrace}");
                    }
                }
            }

            Control.Invalidate(true);
        }

        public void UpdateControl(){
            ThreadSafeInvoke(()=>{
                UpdateControlInternal();
            });
        }

        public ShellWidgetConfig GetConfig(){
            return Config;
        }

        public void SetConfig(ShellWidgetConfig newConfig){
            Config = newConfig;

            if(string.IsNullOrEmpty(Config.Id)){
                Config.Id = Guid.NewGuid().ToString();
            }
        }

        public virtual void UpdateConfig(){
            Config.Id = Node.GetAttribute("id", Guid.NewGuid().ToString());
            Config.Dock = Node.GetAttribute("dock");
            Config.Anchor = Node.GetAttribute("anchor");
            Config.X = ParseHelper.ParseInt(Node.GetAttribute("x"));
            Config.Y = ParseHelper.ParseInt(Node.GetAttribute("y"));
            Config.Width = ParseHelper.ParseInt(Node.GetAttribute("width"));
            Config.Height = ParseHelper.ParseInt(Node.GetAttribute("height"));
            Config.AutoSize = ParseHelper.ParseBool(Node.GetAttribute("autoSize"));
            Config.Color = Node.GetAttribute("color");
            Config.BackgroundColor = Node.GetAttribute("backgroundColor");
            Config.Padding = Node.GetAttribute("padding");
            Config.Margin = Node.GetAttribute("margin");
            Config.UpdateInterval = ParseHelper.ParseInt(Node.GetAttribute("updateInterval"));
            Config.Font = Node.GetAttribute("font");
            Config.ToolTip = Node.GetAttribute("toolTip");
            Config.Layer = ParseHelper.ParseInt(Node.GetAttribute("layer", "-1"));
        }

        public IContainerWidget GetContainer(){
            return Container;
        }

        public void SetContainer(IContainerWidget container){
            Container = container;
        }

        public IShellWindow GetWindow(){
            return Window;
        }

        public void SetWindow(IShellWindow window){
            Window = window;
        }

        void HandleEvent(string eventName, object sender, EventArgs ea){
            foreach(var handler in EventMap[eventName]){
                handler.Invoke(new ShellWidgetEvent());
            }
        }

        protected virtual void RegisterEventHandlerInternal(string name, Action<ShellWidgetEvent> handler){
            if(!EventMap.ContainsKey(name)){
                EventMap.Add(name, new());
                Control.GetType().GetMethod("add_" + name).Invoke(Control, new object[]{ new EventHandler((sender, args) => {
                    HandleEvent(name, sender, args);
                }) });
            }

            EventMap[name].Add(handler);
        }

        public void RegisterEventHandler(string name, Action<ShellWidgetEvent> handler){
            ThreadSafeInvoke(()=>{
                RegisterEventHandlerInternal(name, handler);
            });
        }

        protected void ThreadSafeInvoke(Action action){
            if(Control is not null && Control.InvokeRequired){
                Control.Invoke(action);
            }else{
                action.Invoke();
            }
        }
    }
}
