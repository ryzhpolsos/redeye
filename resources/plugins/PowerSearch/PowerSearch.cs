using RedEye.PluginAPI;

public class PowerSearchPlugin : Plugin {
    public override string Name {
        get {
            return "powerSearch";
        }
    }

    public override void Main(){
        ExportFunction("open", (args, _) => {
            new PowerSearchWindow().Show(); 
            return string.Empty;
        });
    }
}
