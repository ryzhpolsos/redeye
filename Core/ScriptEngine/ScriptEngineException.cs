using System;

namespace RedEye.Core.ScriptEngine {
    public class ScriptEngineException : Exception {
        string message = null;
        public override string Message => message;

        public ScriptEngineException(string message){
            this.message = message;
        }
    }
}