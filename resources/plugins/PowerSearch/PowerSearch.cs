using System.Linq;
using System.Collections.Generic;

using RedEye.PluginAPI;

namespace PowerSearch {
    static class PowerSearchWindowState {
        public static bool Opened = false;
    }

    public class PowerSearchPlugin : Plugin {
        Dictionary<string, string> psConfig = new Dictionary<string, string>();

        public override void Main(){
            ExportFunction("open", (args, _) => {
                if(!PowerSearchWindowState.Opened){
                    PowerSearchWindowState.Opened = true;
                    new PowerSearchWindow(ComponentManager, psConfig).Show(); 
                }

                return string.Empty;
            });

            ExportFunction("config", (args, _) => {
                var name = args.ElementAt(0).ToString();
                var value = args.ElementAt(1).ToString();
                
                if(psConfig.ContainsKey(name)){
                    psConfig[name] = value;
                }else{
                    psConfig.Add(name, value);
                }

                return string.Empty; 
            });

            SuggestionHandlers.Register(ComponentManager);
        }
    }
}
