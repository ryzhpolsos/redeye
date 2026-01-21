using System;
using System.Collections.Generic;

namespace RedEye.Core.ScriptEngine {
    public interface IScriptHandler {
        public void EvaluateCode(string code, IDictionary<string, object> nameSpace, IDictionary<string, object> parameters = null);
    }
}