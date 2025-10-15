using System;
using System.IO;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace RedEye {
    public class ConfigFile {
        public CoreConfig core;
    }

    public class CoreConfig {
        public bool debugMode;
        public bool useEdgeRuntime;
        public bool useWmxShellHook;
        public string ieVersion;
        public string edgeArguments;
        public bool allowCache;
        public bool enableCustomWindows;
        public string[] loadScripts;
        public string[] loadStyles;
        public string[] loadHTWScripts;
        public string[] parseJsSyntax;
    }

    public class Config {
        public static readonly string AppDir = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Directory.FullName;
        public static ConfigFile CurrentConfig;
        public static string CurrentConfigText;

        public static ConfigFile Get(){
            string fileData = File.ReadAllText(Path.Combine(AppDir, "config.json"));
            CurrentConfigText = fileData;
            return Util.FromJson<ConfigFile>(fileData);
        }

        public static void Set(ConfigFile config){
            string text = Util.ToJson(config);
            CurrentConfigText = text;
            File.WriteAllText(Path.Combine(AppDir, "config.json"), text);
        }

        public static void Load(){
            try{
                CurrentConfig = Get();
            }catch(Exception e){
                Logger.Log(Logger.MessageType.Critical, "Failed to load config: " + e.Message);
            }
        }

        public static void Save(){
            try{
                Set(CurrentConfig);
            }catch(Exception e){
                Logger.Log(Logger.MessageType.Critical, "Failed to save config: " + e.Message);
            }
        }
    }
}
