using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;

namespace DllCallerLib{
    public class Argument{
        public Argument(string type, object value){
            this.Type = type;
            this.Value = value;
        }
        public string Type;
        public object Value;
    }

    public static class DllCaller{

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string funcName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, IntPtr funcOrdinal);

        private static Type CreateDelegate<T>(string[] argumentTypes){
            Type delegateReturnType = typeof(T);

            Type[] delegateArgumentTypes = new Type[argumentTypes.Length];
            for(int i = 0; i < argumentTypes.Length; i++){
                delegateArgumentTypes[i] = Type.GetType(argumentTypes[i]);
            }

            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

            TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicDelegate", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class, typeof(MulticastDelegate));

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(IntPtr) });
            constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            MethodBuilder methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, delegateReturnType, delegateArgumentTypes);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            Type delegateType = typeBuilder.CreateType();
            return delegateType;
        }

        private static Type CreateDelegate(string returnType, string[] argumentTypes){
            Type delegateReturnType = Type.GetType(returnType);

            Type[] delegateArgumentTypes = new Type[argumentTypes.Length];
            for(int i = 0; i < argumentTypes.Length; i++){
                delegateArgumentTypes[i] = Type.GetType(argumentTypes[i]);
            }

            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

            TypeBuilder typeBuilder = moduleBuilder.DefineType("DynamicDelegate", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class, typeof(MulticastDelegate));

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(IntPtr) });
            constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            MethodBuilder methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, delegateReturnType, delegateArgumentTypes);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            Type delegateType = typeBuilder.CreateType();
            return delegateType;
        }

        public static T CallFunction<T>(string dllName, string funcName, List<Argument> args){
            return CallFunction<T>(dllName, funcName, args.ToArray());
        }

        [HandleProcessCorruptedStateExceptions]
        public static T CallFunction<T>(string dllName, string funcName, Argument[] args){
            IntPtr mod = LoadLibrary(dllName);
            IntPtr fn = IntPtr.Zero;

            if(funcName.StartsWith("#")){
                fn = GetProcAddress(mod, new IntPtr(int.Parse(funcName.Substring(1))));
            }else{
                fn = GetProcAddress(mod, funcName);
            }

            List<string> argTypes = new List<string>();
            List<object> argValues = new List<object>();

            foreach(Argument arg in args){
                argTypes.Add(arg.Type);
                argValues.Add(arg.Value);
            }

            Type del = CreateDelegate<T>(argTypes.ToArray());

            return (T)(Marshal.GetDelegateForFunctionPointer(fn, del).DynamicInvoke(argValues.ToArray()));
        }

        public static object CallFunction(string dllName, string funcName, string type, List<Argument> args){
            return CallFunction(dllName, funcName, type, args.ToArray());
        }

        [HandleProcessCorruptedStateExceptions]
        public static object CallFunction(string dllName, string funcName, string type, Argument[] args){
            IntPtr mod = LoadLibrary(dllName);
            IntPtr fn = IntPtr.Zero;

            if(funcName.StartsWith("#")){
                fn = GetProcAddress(mod, new IntPtr(int.Parse(funcName.Substring(1))));
            }else{
                fn = GetProcAddress(mod, funcName);
            }

            List<string> argTypes = new List<string>();
            List<object> argValues = new List<object>();

            foreach(Argument arg in args){
                argTypes.Add(arg.Type);
                argValues.Add(arg.Value);
            }

            Type del = CreateDelegate(type, argTypes.ToArray());

            try{
                return Marshal.GetDelegateForFunctionPointer(fn, del).DynamicInvoke(argValues.ToArray());
            }catch(Exception e){
                throw new Exception(e.Message);
            }
        }
    }
}