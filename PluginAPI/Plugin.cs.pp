using System;
using System.Collections.Generic;

using RedEye.UI;
using RedEye.Core;

namespace RedEye.PluginAPI {
    public class Plugin {
        protected ComponentManager ComponentManager = null;
        protected IPluginManager PluginManager = null;
        protected IConfig Config = null;
        protected IExpressionFunctions ExpressionFunctions = null;
        protected IHotKeyManager HotKeyManager = null;
        protected ILayoutLoader LayoutLoader = null;
        protected ILogger Logger = null;
        protected IMediaManager MediaManager = null;
        protected IResourceManager ResourceManager = null;
        protected IScriptEngine ScriptEngine = null;
        protected IShellEventListener ShellEventListener = null;
        protected IShellWindowManager ShellWindowManager = null;
        protected ISpecialFolderWrapper SpecialFolderWrapper = null;
        protected IWindowManager WindowManager = null;
        protected IWmxManager WmxManager = null;


        public virtual string Name {
            get {
                throw new NotImplementedException();
            }
        }

        public void InitPlugin(ComponentManager manager){
            ComponentManager = manager;
            PluginManager = ComponentManager.GetComponent<IPluginManager>();
            Config = ComponentManager.GetComponent<IConfig>();
            ExpressionFunctions = ComponentManager.GetComponent<IExpressionFunctions>();
            HotKeyManager = ComponentManager.GetComponent<IHotKeyManager>();
            LayoutLoader = ComponentManager.GetComponent<ILayoutLoader>();
            Logger = ComponentManager.GetComponent<ILogger>();
            MediaManager = ComponentManager.GetComponent<IMediaManager>();
            ResourceManager = ComponentManager.GetComponent<IResourceManager>();
            ScriptEngine = ComponentManager.GetComponent<IScriptEngine>();
            ShellEventListener = ComponentManager.GetComponent<IShellEventListener>();
            ShellWindowManager = ComponentManager.GetComponent<IShellWindowManager>();
            SpecialFolderWrapper = ComponentManager.GetComponent<ISpecialFolderWrapper>();
            WindowManager = ComponentManager.GetComponent<IWindowManager>();
            WmxManager = ComponentManager.GetComponent<IWmxManager>();

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

