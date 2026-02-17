using System;
using System.Linq;
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace RedEye.Core {
    public static class CSharpHelper {
        static CSharpCodeProvider provider = new();
        static CompilerParameters compilerParameters = new();

        static CSharpHelper(){
            compilerParameters.GenerateInMemory = true;
            compilerParameters.GenerateExecutable = false;

            List<string> assemblies = new();
            assemblies.Add(Assembly.GetExecutingAssembly().Location);

            foreach(var asm in Assembly.GetExecutingAssembly().GetReferencedAssemblies()){
                assemblies.Add(Assembly.Load(asm.FullName).Location);
            }

            compilerParameters.ReferencedAssemblies.AddRange(assemblies.ToArray());
        }

        public static void AddAssembly(string asmName){
            compilerParameters.ReferencedAssemblies.Add(asmName);
        }

        public static void AddAssembly(Assembly asm){
            AddAssembly(asm.Location);
        }

        public static CSharpCompilerResult CompileCode(params string[] codes){
            var compiled = provider.CompileAssemblyFromSource(compilerParameters, codes);

            if(compiled.Errors.HasErrors){
                return new CSharpCompilerResult(){ Success = false, Errors = compiled.Errors };
            }

            return new CSharpCompilerResult(){ Success = true, Assembly = compiled.CompiledAssembly };
        }
    }

    public class CSharpCompilerResult {
        public bool Success = false;
        public Assembly Assembly = null;
        public CompilerErrorCollection Errors = null;
    }
}