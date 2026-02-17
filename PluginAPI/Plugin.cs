using System;
using System.Collections.Generic;

using RedEye.UI;
using RedEye.Core;

namespace RedEye.PluginAPI {
    public class Plugin {
        protected ComponentManager ComponentManager = null;
        protected IConfig Config = null;
        protected ILayoutLoader LayoutLoader = null;
        protected ILogger Logger = null;
        protected IPluginManager PluginManager = null;
        protected IScriptEngine ScriptEngine = null;
        protected IShellWindowManager ShellWindowManager = null;
        protected IShellEventListener ShellEventListener = null;
        protected IWmxManager WmxManager = null;

        public virtual string Name {
            get {
                throw new NotImplementedException();
            }
        }

        public void InitPlugin(ComponentManager manager){
            ComponentManager = manager;
            Config = manager.GetComponent<IConfig>();
            LayoutLoader = manager.GetComponent<ILayoutLoader>();
            Logger = manager.GetComponent<ILogger>();
            PluginManager = manager.GetComponent<IPluginManager>();
            ScriptEngine = manager.GetComponent<IScriptEngine>();
            ShellWindowManager = manager.GetComponent<IShellWindowManager>();
            ShellEventListener = manager.GetComponent<IShellEventListener>();
            WmxManager = manager.GetComponent<IWmxManager>();
        }

        public virtual void Main(){}

        protected void ExportWidget<T>(string name) where T : IShellWidget {
            PluginManager.ExportWidget($"{Name}.{name}", typeof(T));
        }

        protected void ExportFunction(string name, Func<IEnumerable<object>, object> func){
            PluginManager.ExportFunction($"{Name}.{name}", func);
        }
    }
}
