using System;
using System.Collections.Generic;

using RedEye.Core.ScriptEngine;

namespace RedEye.Core {
    public interface IScriptEngine : IComponent {
        public IScriptEngine RegisterScriptHandler(string name, IScriptHandler handler);
        public void ExecuteScript(string handlerName, string code, IDictionary<string, object> nameSpace, IDictionary<string, object> parameters = null);
    }
}
