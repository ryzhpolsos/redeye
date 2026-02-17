using System;
using System.Collections.Generic;

using RedEye.Core;
using RedEye.Core.ScriptEngine;

namespace RedEye.Components {
    public class ScriptEngineComponent : IScriptEngine {
        Dictionary<string, IScriptHandler> scriptHandlers = new();

        public void SetManager(ComponentManager manager){}
        public void Initialize(){}

        public IScriptEngine RegisterScriptHandler(string name, IScriptHandler handler){
            scriptHandlers.Add(name, handler);
            return this;
        }

        public void ExecuteScript(string handlerName, string code, IDictionary<string, object> nameSpace, IDictionary<string, object> parameters = null){
            scriptHandlers[handlerName].EvaluateCode(code, nameSpace, parameters);
        }
    }
}