using System;
using System.Collections.Generic;

using RedEye.UI;

namespace RedEye.Core {
    public interface IPluginManager : IComponent {
        public Type GetExportedWidget(string name);
        public IDictionary<string, Type> GetExportedWidgets();
        public IPluginManager ExportWidget(string name, Type widget);
        public Func<IEnumerable<object>, object> GetExportedFunction(string name);
        public IDictionary<string, Func<IEnumerable<object>, object>> GetExportedFunctions();
        public IPluginManager ExportFunction(string name, Func<IEnumerable<object>, object> func);
        public void LoadPlugins();
    }
}