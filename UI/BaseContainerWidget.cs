using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RedEye.UI {
    public class BaseContainerWidget : BaseShellWidget, IContainerWidget {
        protected Dictionary<string, IShellWidget> widgets = new();
        protected Dictionary<string, Dictionary<string, object>> widgetParams = new();

        protected virtual Dictionary<string, object> GetDefaultWidgetParams(IShellWidget widget){
            return new();
        }

        protected override void UpdateControlInternal(){
            foreach(var widget in widgets.Values){
                widget.UpdateControl();
            }

            base.UpdateControlInternal();
        }

        public override void UpdateConfig(){
            foreach(var widget in widgets.Values) widget.UpdateConfig();
            base.UpdateConfig();
        }

        protected virtual void AddWidgetInternal(IShellWidget widget){
            var config = widget.GetConfig();
            widgets.Add(config.Id, widget);
            widgetParams.Add(widget.GetConfig().Id, GetDefaultWidgetParams(widget));
            widget.SetContainer(this);

            var control = widget.GetControl();

            if(control is not null){
                Control.Controls.Add(control);

                if(config.Layer >= 0){
                    Control.Controls.SetChildIndex(control, config.Layer);
                }
            }

            foreach(var attr in Node.GetAttributes().Where(a => a.StartsWith("on"))){
                widget.GetNode().SetAttribute(attr, Node.GetRawAttribute(attr));
            }
        }


        public void AddWidget(IShellWidget widget, bool _ = true){
            ThreadSafeInvoke(()=>{
                AddWidgetInternal(widget);
            });
        }

        public IShellWidget GetWidget(string id){
            if(widgets.ContainsKey(id)) return widgets[id];

            foreach(var widget in widgets.Values){
                if(widget is IContainerWidget container && container is not null){
                    if(container.GetWidget(id) is var wid && wid is not null){
                        return wid;
                    }
                }
            }

            return null;
        }

        public IDictionary<string, IShellWidget> GetWidgets(){
            return widgets;
        }

        protected virtual void RemoveWidgetInternal(IShellWidget widget){
            Control.Controls.Remove(widget.GetControl());
            widgetParams.Remove(widget.GetConfig().Id);
            widgets.Remove(widget.GetConfig().Id);
            widget.SetContainer(null);
        }

        public void RemoveWidget(IShellWidget widget){
            ThreadSafeInvoke(()=>{
                RemoveWidgetInternal(widget);
            });
        }

        public object GetWidgetParam(string id, string paramName){
            return widgetParams[id][paramName];
        }

        public void SetWidgetParam(string id, string paramName, object paramValue){
            widgetParams[id][paramName] = paramValue;
        }
    }
}
