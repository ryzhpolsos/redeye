using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RedEye {
    public class PluginLoader {
        public static string Load(string dirName){
            Logger.Log(Logger.MessageType.Information, "Loading plugin: " + dirName);
            try{
                string pluginPath = Path.Combine("plugins", dirName);
                var configText = Util.ReadFile(Path.Combine(pluginPath, "plugin.json"), false);
                var config = Util.FromJson<PluginConfigFile>(configText);
                var builder = new StringBuilder();

                foreach(string fileName in Directory.GetFiles(Util.GetPath(Path.Combine(pluginPath, "styles")))){
                    builder.Append("var s = document.createElement('style');s.innerHTML=");
                    builder.Append(Util.ToJsString(Util.ReadFile(fileName)));
                    builder.Append(";document.head.appendChild(s);\n");
                }

                var loadAllFiles = config.loadFiles == null;
                foreach(string fileName in loadAllFiles ? (IEnumerable<string>)(Directory.GetFiles(Util.GetPath(Path.Combine(pluginPath, "scripts")))) : (IEnumerable<string>)config.loadFiles){
                    builder.Append(Util.ParseJsCode(Util.ReadFile(loadAllFiles ? fileName : Path.Combine(pluginPath, "scripts", fileName))));
                    builder.Append("\n");
                }

                if(Directory.Exists(Path.Combine(pluginPath, "locales"))){
                    foreach(string fileName in Directory.GetFiles(Util.GetPath(Path.Combine(pluginPath, "locales")))){
                        var langName = Util.ToJsString(Path.GetFileNameWithoutExtension(fileName));
                        builder.Append($"if(!redeye.locale.store[{langName}]) redeye.locale.store[{langName}] = {{}};\nobjectAssign(redeye.locale.store[{langName}], ");
                        builder.Append(Util.ReadFile(fileName));
                        builder.Append(");\n");
                    }
                }

                var result = Util.ReplaceTemplate(
                    Util.ReadFile("res\\pluginTemplate.html"),
                    "PLUGIN_CONFIG", configText,
                    "PLUGIN_DIR", Util.ToJsString(Util.GetPath(pluginPath)),
                    "PLUGIN_STORAGE", Util.ReadFile(Path.Combine(pluginPath, "storage.json")),
                    "PLUGIN_FILES", builder.ToString()
                );

                Logger.Log(Logger.MessageType.Information, "Loaded plugin: " + dirName);
                return result;
            }catch(Exception e){
                Logger.Log(Logger.MessageType.Error, $"Failed to load plugin \"{dirName}\": {e.Message}");
                return null;
            }
        }
    }

    public class PluginConfigFile {
        public string name;
        public string displayName;
        public string version;
        public Dictionary<string, string> developer;
        public List<string> excludeFiles;
        public List<string> loadFiles;
    }
}