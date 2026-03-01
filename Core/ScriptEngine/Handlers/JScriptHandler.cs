#pragma warning disable 0618

using System;
using System.Collections.Generic;

using Microsoft.JScript;
using Microsoft.JScript.Vsa;
using System.Linq;

namespace RedEye.Core.ScriptEngine {
    internal class JScriptConverter {
        public Delegate CreateDelegateFromClosure(string type, Closure closure){
            var delType = Type.GetType(type);

            return DynamicDelegateHelper.GetDelegate((data, args) => {
                ((Closure)data).Invoke(null, args);
            }, closure, delType);
        } 
    }

    public class JScriptHandler : IScriptHandler {
        VsaEngine engine = VsaEngine.CreateEngine();

        string preDef = @"
            function callback(type, func){
                return Convert.GetDelegateFromClosure(type, func);
            }
        ";

        public void EvaluateCode(string code, IDictionary<string, object> nameSpace, IDictionary<string, object> parameters = null){
            nameSpace.Add("Convert", new JScriptConverter());

            try{
                var argList = string.Join(", ", nameSpace.Keys);
                code = $"(function({argList}){{ {preDef}\n{code} }})";
                Console.WriteLine("---\n"+code);
      
                var func = (Closure)Eval.JScriptEvaluate(code, engine);
                func.Invoke(func, nameSpace.Values.ToArray());
            }catch(Exception ex){
                throw new ScriptEngineException($"JS runtime error: {ex.GetType().FullName}: {ex.Message}");
            }
        }
    }
}
