using System;
using System.IO;
using System.Text;

namespace RedEye {
    public class Loader {
        public static string IterateFiles(string path, string tag, string[] list){
            var builder = new StringBuilder();
            IterateFiles(builder, path, tag, list);
            return builder.ToString();
        }

        static void IterateFiles(StringBuilder builder, string path, string tag, string[] list){
            try{
                if(list == null || list.Length == 0) list = Directory.GetFiles(path);

                foreach(string fileName in list){
                    builder.Append($"<{tag}>\n");

                    if(tag == "script"){
                        builder.Append(Util.ParseJsCode(Util.ReadFile(Path.Combine(path, fileName))));
                    }else{
                        builder.Append(Util.ReadFile(Path.Combine(path, fileName)));
                    }
                    builder.Append($"\n</{tag}>");
                }
            }catch(Exception e){
                Logger.Log(Logger.MessageType.Critical, $"Failed to iterate files in \"{path}\": {e.Message}");
            }
        }

        // начинается ад...
        public static string Load(){
            Logger.Log(Logger.MessageType.Information, "Loader started");
            if(Config.CurrentConfig.core.allowCache){
                if(File.Exists(Util.GetPath("cached.html"))){
                    Logger.Log(Logger.MessageType.Information, "Using cached.html");
                    return Util.ReadFile("cached.html");
                }
            }

            var builder = new StringBuilder();

            builder.Append($"<!DOCTYPE html><html><head><meta charset=\"utf-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"ie={Config.CurrentConfig.core.ieVersion}\">");
            IterateFiles(builder, Util.GetPath("styles"), "style", Config.CurrentConfig.core.loadStyles);
            builder.Append("<script>window.globalConfig = ");
            builder.Append(Config.CurrentConfigText);
            builder.Append(";</script></head><body>");
            builder.Append(Util.ReadFile("res\\content.html"));
            IterateFiles(builder, Util.GetPath("scripts"), "script", Config.CurrentConfig.core.loadScripts);

            foreach(string pluginName in Directory.GetDirectories(Util.GetPath("plugins"))){
                builder.Append(PluginLoader.Load(pluginName));
            }

            builder.Append("</body></html>");

            Logger.Log(Logger.MessageType.Information, "Finished loading");
            var result = builder.ToString();
            if(Config.CurrentConfig.core.debugMode) File.WriteAllText(Util.GetPath("dbgcache.html"), result);
            return result;
        }
    }
}
