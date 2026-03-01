using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RedEye.Core {
    public static class DynamicDelegateHelper {
        static AssemblyBuilder dynamicAssembly;
        static ModuleBuilder dynamicModule;
        
        static DynamicDelegateHelper(){
            dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynAsm0"), AssemblyBuilderAccess.RunAndSave);
            dynamicModule = dynamicAssembly.DefineDynamicModule("DynMod0");
        }

        public static Delegate GetDelegate(Action<object, object[]> callback, object data, Type delegateType){
            var delInvokeMethod = delegateType.GetMethod("Invoke");

            var typeBuilder = dynamicModule.DefineType("_" + Guid.NewGuid().ToString().Replace("-", "_"));

            var dataField = typeBuilder.DefineField("Data", typeof(object), FieldAttributes.Public);
            var callbackField = typeBuilder.DefineField("Callback", typeof(Action<object, object[]>), FieldAttributes.Public);

            var argumentTypes = delInvokeMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            var method = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public, delInvokeMethod.ReturnType, argumentTypes);
            var il = method.GetILGenerator();
            var local0 = il.DeclareLocal(typeof(object).MakeArrayType());
            var local1 = il.DeclareLocal(typeof(object).MakeArrayType());

            il.Emit(OpCodes.Ldc_I4, argumentTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, local0);

            for(int i = 0; i < argumentTypes.Length; i++){
                il.Emit(OpCodes.Ldloc, local0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Stelem, typeof(object));
            }

            il.Emit(OpCodes.Ldc_I4, 2);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, local1);

            il.Emit(OpCodes.Ldloc, local1);
            il.Emit(OpCodes.Ldc_I4, 0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, dataField);
            il.Emit(OpCodes.Stelem, typeof(object));

            il.Emit(OpCodes.Ldloc, local1);
            il.Emit(OpCodes.Ldc_I4, 1);
            il.Emit(OpCodes.Ldloc, local0);
            il.Emit(OpCodes.Stelem, typeof(object).MakeArrayType());

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, callbackField);
            il.Emit(OpCodes.Ldloc, local1);
            il.Emit(OpCodes.Callvirt, callback.GetType().GetMethod("DynamicInvoke"));
            
            if(delInvokeMethod.ReturnType == typeof(void)) il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateType();
            var inst = Activator.CreateInstance(type);

            type.GetField("Callback").SetValue(inst, callback);
            type.GetField("Data").SetValue(inst, data);

            return Delegate.CreateDelegate(delegateType, inst, type.GetMethod("Invoke"));
        }
    }
}
