using System;
using System.Collections.Generic;

namespace RedEye.Core {
    public interface IPluginManager : IComponent {
        public Type GetExportedWidget(string name);
        public IDictionary<string, Type> GetExportedWidgets();
        public IPluginManager ExportWidget(string name, Type widget);
        public Func<IEnumerable<object>, IVariableStorage<string>, object> GetExportedFunction(string name);
        public IDictionary<string, Func<IEnumerable<object>, IVariableStorage<string>, object>> GetExportedFunctions();
        public IPluginManager ExportFunction(string name, Func<IEnumerable<object>, IVariableStorage<string>, object> func);
        public void LoadPlugins();
    }
}
