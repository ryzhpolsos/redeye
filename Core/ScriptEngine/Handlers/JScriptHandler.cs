#pragma warning disable 0618

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.JScript;
using Microsoft.JScript.Vsa;
using System.Linq;

namespace RedEye.Core.ScriptEngine {
    public class JScriptConverter {
        struct CachedConverter {
            public CachedConverter(){}
            public int ArgCount = 0;
            public bool NeedResult = false;
            public object Delegate = null;
        }

        List<CachedConverter> converters = new();
        AssemblyBuilder assemblyBuilder = null;
        ModuleBuilder moduleBuilder = null;

        public object GetDelegateFromClosure(Closure func, int argCount, bool needResult){
            var conv = converters.Where(cvt => cvt.NeedResult == needResult && cvt.ArgCount == argCount);
            
            if(conv.Any()){
                return conv.ElementAt(0).Delegate;
            }

            if(assemblyBuilder is null){
                assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("JScriptConverterDynAsm"), AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule("JScriptConverterModule");
            }

            Type delegateType = null;

            if(needResult){
                delegateType = Type.GetType($"System.Func`{argCount+1}").MakeGenericType(new Type[argCount + 1].Select(_ => typeof(object)).ToArray());
            }else{
                delegateType = Type.GetType($"System.Action`{argCount}").MakeGenericType(new Type[argCount].Select(_ => typeof(object)).ToArray());
            }

            var typeBuilder = moduleBuilder.DefineType($"JScriptConverterType_{argCount}_{needResult}", TypeAttributes.Public);

            var field = typeBuilder.DefineField("_w_Closure", typeof(Closure), FieldAttributes.Public);

            var callMethod = typeBuilder.DefineMethod("_w_CallClosure", MethodAttributes.Public, needResult ? typeof(object) : typeof(void), new Type[argCount].Select(_ => typeof(object)).ToArray());
            var callIL = callMethod.GetILGenerator();

            var cmArgArray = callIL.DeclareLocal(typeof(object[]));

            callIL.Emit(OpCodes.Ldc_I4, argCount);
            callIL.Emit(OpCodes.Newarr, typeof(object));
            callIL.Emit(OpCodes.Stloc, cmArgArray);

            for(int i = 0; i < argCount; i++){
                callIL.Emit(OpCodes.Ldloc, cmArgArray);
                callIL.Emit(OpCodes.Ldc_I4, i);
                callIL.Emit(OpCodes.Ldarg, i + 1);
                callIL.Emit(OpCodes.Stelem_Ref, typeof(object));
            }

            callIL.Emit(OpCodes.Ldarg_0);
            callIL.Emit(OpCodes.Ldfld, field);
            callIL.Emit(OpCodes.Ldarg_0);
            callIL.Emit(OpCodes.Ldfld, field);
            callIL.Emit(OpCodes.Ldloc, cmArgArray);
            callIL.Emit(OpCodes.Callvirt, typeof(Closure).GetMethod("Invoke"));
            if(!needResult) callIL.Emit(OpCodes.Pop);
            callIL.Emit(OpCodes.Ret);

            var convertMethod = typeBuilder.DefineMethod("_w_ConvertDelegate", MethodAttributes.Public, delegateType, new Type[]{ typeof(object) });
            var convertIL = convertMethod.GetILGenerator();

            convertIL.Emit(OpCodes.Ldarg_0);
            convertIL.Emit(OpCodes.Ldarg_1);
            convertIL.Emit(OpCodes.Stfld, field);
            convertIL.Emit(OpCodes.Ldtoken, delegateType);
            convertIL.Emit(OpCodes.Ldarg_0);
            convertIL.Emit(OpCodes.Ldstr, "_w_CallClosure");
            convertIL.Emit(OpCodes.Call, typeof(Delegate).GetMethod("CreateDelegate", new Type[]{ typeof(Type), typeof(object), typeof(string) }));
            convertIL.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(type);
            var del = type.GetMethod("_w_ConvertDelegate").Invoke(instance, new object[]{ func });
        
            var cvt = new CachedConverter();
            cvt.ArgCount = argCount;
            cvt.NeedResult = needResult;
            cvt.Delegate = del;
            converters.Add(cvt);

            return del;
        }
    }

    public class JScriptHandler : IScriptHandler {
        VsaEngine engine = VsaEngine.CreateEngine();

        string preDef = @"
            function callback(func, needResult){
                return Convert.GetDelegateFromClosure(func, func.length, !!needResult);
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