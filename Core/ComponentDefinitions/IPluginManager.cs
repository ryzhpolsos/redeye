using System;
using System.Collections.Generic;

namespace RedEye.Core {
    public interface IPluginManager : IComponent {
        public PluginInfo GetPlugin(string id);
        public IEnumerable<PluginInfo> GetPlugins(); 
        public Type GetExportedWidget(string name);
        public IDictionary<string, Type> GetExportedWidgets();
        public IPluginManager ExportWidget(string name, Type widget);
        public Func<IEnumerable<object>, IVariableStorage<string>, object> GetExportedFunction(string name);
        public IDictionary<string, Func<IEnumerable<object>, IVariableStorage<string>, object>> GetExportedFunctions();
        public IPluginManager ExportFunction(string name, Func<IEnumerable<object>, IVariableStorage<string>, object> func);
        public void LoadPlugins();
    }

    public class PluginInfo {
        public string Id = string.Empty;
        public string Name = string.Empty;
        public IEnumerable<string> RequiredAssemblies = null;
        public IEnumerable<string> Dependencies = null;
    }
}
