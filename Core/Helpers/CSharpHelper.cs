using System;
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Collections.Generic;

namespace RedEye.Core {
    public static class CSharpHelper {
        static CSharpCodeProvider provider = new();
        static CompilerParameters compilerParameters = new();

        static string tempDir;

        static CSharpHelper(){
            // AppContext.SetSwitch("Switch.System.DisableTempFileCollectionDirectoryFeature", true);

            tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RedEye", "Temp");
            Directory.CreateDirectory(tempDir);

            foreach(var file in Directory.GetFiles(tempDir)){
                File.Delete(file);
            }

            compilerParameters.GenerateInMemory = true;
            compilerParameters.GenerateExecutable = false;
            compilerParameters.TempFiles = new(tempDir, false);
            compilerParameters.OutputAssembly = Path.Combine(tempDir, Guid.NewGuid() + ".dll");

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
            return CompileCode(false, codes);
        }

        public static CSharpCompilerResult CompileCode(bool exportDll, params string[] codes){
            compilerParameters.OutputAssembly = Path.Combine(tempDir, Guid.NewGuid() + ".dll");
            compilerParameters.GenerateInMemory = !exportDll;
            
            var compiled = provider.CompileAssemblyFromSource(compilerParameters, codes);

            if(compiled.Errors.HasErrors){
                return new CSharpCompilerResult(){ Success = false, Errors = compiled.Errors };
            }

            return new CSharpCompilerResult(){ Success = true, Assembly = compiled.CompiledAssembly, FullName = compilerParameters.OutputAssembly };
        }
    }

    public class CSharpCompilerResult {
        public bool Success = false;
        public Assembly Assembly = null;
        public CompilerErrorCollection Errors = null;
        public string FullName = null;
    }
}
