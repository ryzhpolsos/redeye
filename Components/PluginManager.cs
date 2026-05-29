using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using RedEye.UI;
using RedEye.Core;
using RedEye.PluginAPI;
using System.Reflection;

namespace RedEye.Components {
    public class PluginManagerComponent : IPluginManager {
        class PluginConfig {
            public string id = null;
            public string name = null;
            public List<string> requiredAssemblies = null;
            public List<string> dependencies = null;
        }

        struct PluginLoaderInfo {
            public string DirectoryName;
            public PluginConfig Config;
        }

        IConfig config = null;
        ILogger logger = null;
        IScriptEngine scriptEngine = null;

        ComponentManager manager = null;

        List<PluginLoaderInfo> plugins = new();
        Dictionary<string, Type> exportedWidgets = new();
        Dictionary<string, Func<IEnumerable<object>, IVariableStorage<string>, object>> exportedFunctions = new();

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            config = manager.GetComponent<IConfig>();
            logger = manager.GetComponent<ILogger>();
            scriptEngine = manager.GetComponent<IScriptEngine>();
        }

        public PluginInfo GetPlugin(string id){
            var conf = plugins.First(x => x.Config.id == id).Config;

            return new(){
                Id = conf.id,
                Name = conf.name,
                RequiredAssemblies = conf.requiredAssemblies,
                Dependencies = conf.dependencies
            };
        }
        
        public IEnumerable<PluginInfo> GetPlugins(){
            return plugins.Select(x => x.Config).Cast<PluginInfo>();
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

        public IPluginManager ExportFunction(string name, Func<IEnumerable<object>, IVariableStorage<string>, object> func){
            exportedFunctions.Add(name, func);
            return this;
        }

        public Func<IEnumerable<object>, IVariableStorage<string>, object> GetExportedFunction(string name){
            return exportedFunctions[name];
        }

        public IDictionary<string, Func<IEnumerable<object>, IVariableStorage<string>, object>> GetExportedFunctions(){
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

            List<string> loadedPlugins = new();

            foreach(var dir in Directory.GetDirectories(pluginsDir)){
                plugins.Add(new(){
                    DirectoryName = dir,
                    Config = ParseHelper.ParseJson<PluginConfig>(File.ReadAllText(Path.Combine(dir, "plugin.json"))) 
                });
            }

            for(int i = 0; i < plugins.Count; i++){
                var plugin = plugins[i];
                var needContinue = false;
                
                logger.LogInformation($"Loading plugin: {plugin.Config.id}");

                if(plugin.Config.dependencies is not null){
                    foreach(var dep in plugin.Config.dependencies){
                        if(!plugins.Any(x => x.Config.id == dep)){
                            logger.LogFatal($"Failed to load plugin \"{plugin.Config.id}\": required dependency \"{dep}\" not found");
                            return;
                        }

                        if(!loadedPlugins.Contains(dep)){
                            plugins.RemoveAt(i);
                            plugins.Add(plugin);

                            needContinue = true;
                            break;
                        }
                    }
                }

                if(needContinue) continue;

                if(plugin.Config.requiredAssemblies is not null){
                    foreach(var asm in plugin.Config.requiredAssemblies){
                        CSharpHelper.AddAssembly(Assembly.Load(asm.ToString()));
                    }
                }

                List<string> codes = new();
                foreach(var file in Directory.GetFiles(plugin.DirectoryName)){
                    if(file.EndsWith(".cs")) codes.Add(File.ReadAllText(file));
                }

                var result = CSharpHelper.CompileCode(true, codes.ToArray());

                if(!result.Success){
                    var message = string.Empty;
                    foreach(CompilerError err in result.Errors){
                        message += $"C# error in plugin \"{plugin.Config.id}\": {err.ErrorText} on line {err.Line}; ";
                    }

                    logger.LogFatal(message);
                    return;
                }

                CSharpHelper.AddAssembly(result.FullName);

                foreach(var type in result.Assembly.GetTypes()){
                    if(type.IsSubclassOf(typeof(Plugin))){
                        var plug = (Plugin)Activator.CreateInstance(type);
                        plug.InitPlugin(manager, plugin.Config.id);
                        plug.Main();
                    }
                }

                loadedPlugins.Add(plugin.Config.id);
                logger.LogInformation("Loaded plugin: " + plugin.Config.id);
            }

            logger.LogInformation("Plugin loader completed");
        }
    }
}
