using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RedEye.UI {
    public class BaseContainerWidget : BaseShellWidget, IContainerWidget {
        protected Dictionary<string, IShellWidget> widgets = new();
        protected Dictionary<string, Dictionary<string, object>> widgetParams = new();

        public BaseContainerWidget() : base(){}

        protected virtual Dictionary<string, object> GetDefaultWidgetParams(IShellWidget widget){
            return new();
        }

        protected override void UpdateControlInternal(){
            foreach(var widget in widgets.Values){
                widget.UpdateControl();
            }

            base.UpdateControlInternal();
        }

        protected virtual void AddWidgetInternal(IShellWidget widget){
            widgets.Add(widget.GetConfig().Id, widget);
            widgetParams.Add(widget.GetConfig().Id, GetDefaultWidgetParams(widget));
            widget.SetContainer(this);
            if(widget.GetControl() is not null) Control.Controls.Add(widget.GetControl());

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
            return widgets[id];
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
