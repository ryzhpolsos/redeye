using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using RedEye.UI;
using RedEye.Core;
using RedEye.PluginAPI;
using System.Reflection;

namespace RedEye.Components {
    public class PluginManagerComponent : IPluginManager {
        IConfig config = null;
        ILogger logger = null;
        IScriptEngine scriptEngine = null;

        ComponentManager manager = null;

        Dictionary<string, Type> exportedWidgets = new();
        Dictionary<string, Func<IEnumerable<object>, object>> exportedFunctions = new();

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            config = manager.GetComponent<IConfig>();
            logger = manager.GetComponent<ILogger>();
            scriptEngine = manager.GetComponent<IScriptEngine>();
        }

        public Type GetExportedWidget(string name){
            return exportedWidgets[name];
        }

        public IDictionary<string, Type> GetExportedWidgets(){
            return exportedWidgets;
        }

        public IPluginManager ExportWidget(string name, Type widget){
            if(!typeof(IShellWidget).IsAssignableFrom(widget)) throw new ArgumentException("Type must implement IShellWidget interface");
            exportedWidgets.Add(name, widget);
            return this;
        }

        public IPluginManager ExportFunction(string name, Func<IEnumerable<object>, object> func){
            exportedFunctions.Add(name, func);
            return this;
        }

        public Func<IEnumerable<object>, object> GetExportedFunction(string name){
            return exportedFunctions[name];
        }

        public IDictionary<string, Func<IEnumerable<object>, object>> GetExportedFunctions(){
            return exportedFunctions;
        }

        public void LoadPlugins(){
            logger.LogInformation("Plugin loader started");

            var pluginsDir = Path.Combine(config.GetAppDirectory(), "plugins");

            if(!Directory.Exists(pluginsDir)){
                Directory.CreateDirectory(pluginsDir);
                logger.LogInformation("Plugin loader completed");
                return;
            }

            foreach(var dir in Directory.GetDirectories(pluginsDir)){
                if(File.Exists(Path.Combine(dir, "plugin.json"))){
                    logger.LogInformation($"Loading plugin: {Path.GetFileName(dir)}");

                    var pluginConfig = ParseHelper.ParseJson<Dictionary<string, object>>(File.ReadAllText(Path.Combine(dir, "plugin.json")));
                    if(pluginConfig.ContainsKey("requiredAssemblies") && pluginConfig["requiredAssemblies"] is not null && pluginConfig["requiredAssemblies"] is IEnumerable<string>){
                        foreach(var asm in (IEnumerable<string>)pluginConfig["requiredAssemblies"]){
                            CSharpHelper.AddAssembly(Assembly.LoadFile(asm));
                        }
                    }

                    List<string> codes = new();
                    foreach(var file in Directory.GetFiles(dir)){
                        if(file.EndsWith(".cs")) codes.Add(File.ReadAllText(file));
                    }

                    var result = CSharpHelper.CompileCode(codes.ToArray());

                    if(!result.Success){
                        var message = string.Empty;
                        foreach(CompilerError err in result.Errors){
                            message += $"C# error in plugin \"{Path.GetFileName(dir)}\": {err.ErrorText} on line {err.Line}; ";
                        }

                        logger.LogFatal(message);
                        return;
                    }

                    foreach(var type in result.Assembly.GetTypes()){
                        if(type.IsSubclassOf(typeof(Plugin))){
                            var plugin = (Plugin)Activator.CreateInstance(type);
                            plugin.InitPlugin(manager);
                            plugin.Main();
                        }
                    }
                }
            }

            logger.LogInformation("Plugin loader completed");
        }
    }
}
