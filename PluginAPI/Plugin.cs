using System;
using System.Collections.Generic;

using RedEye.UI;
using RedEye.Core;

namespace RedEye.PluginAPI {
    public class Plugin {
        protected ComponentManager ComponentManager = null;
        protected IPluginManager PluginManager = null;
        //$$COMPONENTS_DEF

        public virtual string Name {
            get {
                throw new NotImplementedException();
            }
        }

        public void InitPlugin(ComponentManager manager){
            ComponentManager = manager;
            PluginManager = ComponentManager.GetComponent<IPluginManager>();
            //$$COMPONENTS_SET
        }

        public virtual void Main(){}

        protected void ExportWidget<T>(string name) where T : IShellWidget {
            PluginManager.ExportWidget($"{Name}.{name}", typeof(T));
        }

        protected void ExportFunction(string name, Func<IEnumerable<object>, IVariableStorage<string>, object> func){
            PluginManager.ExportFunction($"{Name}.{name}", func);
        }
    }
}
