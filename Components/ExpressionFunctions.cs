using System;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ExpressionFunctionsComponent : IExpressionFunctions {
        ComponentManager manager = null;

        IShellWindowManager shellWindowManager = null;
        IShellEventListener shellEventListener = null;
        IExpressionParser expressionParser = null;
        IResourceManager resourceManager = null;
        IPluginManager pluginManager = null;
        IMediaManager mediaManager = null;
        IConfig config = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            shellWindowManager = manager.GetComponent<IShellWindowManager>();
            shellEventListener = manager.GetComponent<IShellEventListener>();
            expressionParser = manager.GetComponent<IExpressionParser>();
            resourceManager = manager.GetComponent<IResourceManager>();
            pluginManager = manager.GetComponent<IPluginManager>();
            mediaManager = manager.GetComponent<IMediaManager>();
            config = manager.GetComponent<IConfig>();

            pluginManager.ExportFunction("eval", (args, _) => {
                return EvalHelper.Eval(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("calc", (args, _) => {
                return EvalHelper.Eval(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("run", (_, _) => {
                return string.Empty;
            });

            pluginManager.ExportFunction("if", (args, _) => {
                if(ParseHelper.ParseBool(EvalHelper.Eval(args.ElementAt(0).ToString()))){
                    return args.ElementAt(1).ToString();
                }else if(args.Count() > 2){
                    return args.ElementAt(2).ToString();
                }

                return string.Empty;
            });
            
            pluginManager.ExportFunction("ife", (args, vars) => {
                if(ParseHelper.ParseBool(EvalHelper.Eval(args.ElementAt(0).ToString()))){
                    return expressionParser.EvaluateExpression(args.ElementAt(1).ToString(), vars);
                }else if(args.Count() > 2){
                    return expressionParser.EvaluateExpression(args.ElementAt(2).ToString(), vars);
                }

                return string.Empty;
            });

            pluginManager.ExportFunction("get", (args, _) => {
                var obj = resourceManager.GetResource(args.ElementAt(0).ToString());
                var type = obj.GetType();
                var name = args.ElementAt(1).ToString();

                if(type.GetField(name) is var field && field is not null){
                    return field.GetValue(obj);
                }
                
                if(type.GetProperty(name) is var prop && prop is not null){
                    return prop.GetValue(obj);
                }

                return string.Empty;
            });

            pluginManager.ExportFunction("eq", (args, _) => {
                Console.WriteLine(args.ElementAt(0).ToString());
                Console.WriteLine(args.ElementAt(1).ToString());
                return (args.ElementAt(0).ToString() == args.ElementAt(1).ToString()) ? "true" : "false"; 
            });

            pluginManager.ExportFunction("concat", (args, _) => {
                return string.Join("", args.Select(s => s.ToString()));
            });

            pluginManager.ExportFunction("join", (args, _) => {
                return string.Join(args.ElementAt(0).ToString(), args.Skip(1).Select(s => s.ToString()));
            });

            pluginManager.ExportFunction("showMessage", (args, _) => {
                Task.Run(() => MessageBox.Show(args.ElementAt(0).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("dateTime", (args, _) => {
                return DateTime.Now.ToString(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("expand", (args, _) => {
                return Environment.ExpandEnvironmentVariables(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("shellExecute", (args, _) => {
                try{
                    var arg0 = args.ElementAt(0).ToString();

                    ProcessStartInfo psi = new();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/c start \"{arg0.Replace("\"", "")}\" {arg0}";
                    psi.UseShellExecute = false;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(psi);
                }catch(Exception){}
                return string.Empty;
            });

            pluginManager.ExportFunction("setWorkArea", (args, _) => {
                shellEventListener.SetWorkArea(ParseHelper.ParseInt(args.ElementAt(0).ToString()), ParseHelper.ParseInt(args.ElementAt(1).ToString()), ParseHelper.ParseInt(args.ElementAt(2).ToString()), ParseHelper.ParseInt(args.ElementAt(3).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.minimizeWindow", (args, _) => {
                MinimizeWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.restoreWindow", (args, _) => {
                RestoreWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.activateWindow", (args, _) => {
                ActivateWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.toggleWindow", (args, _) => {
                shellEventListener.ToggleWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.closeWindow", (args, _) => {
                CloseWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("window.show", (args, _) => {
                shellWindowManager.GetWindow(args.ElementAt(0).ToString()).ShowWindow();
                return string.Empty;
            });

            pluginManager.ExportFunction("window.hide", (args, _) => {
                shellWindowManager.GetWindow(args.ElementAt(0).ToString()).HideWindow();
                return string.Empty;
            });
            
            pluginManager.ExportFunction("window.close", (args, _) => {
                shellWindowManager.GetWindow(args.ElementAt(0).ToString()).CloseWindow();
                return string.Empty;
            });


            pluginManager.ExportFunction("window.toggle", (args, _) => {
                shellWindowManager.GetWindow(args.ElementAt(0).ToString()).ToggleWindow();
                return string.Empty;
            });

            pluginManager.ExportFunction("media.getBrightness", (args, _) => {
                return mediaManager.GetBrightness().ToString();
            });

            pluginManager.ExportFunction("media.setBrightness", (args, _) => {
                mediaManager.SetBrightness(ParseHelper.ParseInt(args.ElementAt(0).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("media.increaseBrightness", (args, _) => {
                mediaManager.IncreaseBrightness(args.Count() > 0 ? ParseHelper.ParseInt(args.ElementAt(0).ToString()) : 10);
                return string.Empty;     
            });
            
            pluginManager.ExportFunction("media.decreaseBrightness", (args, _) => {
                mediaManager.DecreaseBrightness(args.Count() > 0 ? ParseHelper.ParseInt(args.ElementAt(0).ToString()) : 10);
                return string.Empty;     
            });

            pluginManager.ExportFunction("media.getVolume", (args, _) => {
                return mediaManager.GetVolume().ToString();        
            });

            pluginManager.ExportFunction("media.setVolume", (args, _) => {
                mediaManager.SetVolume(ParseHelper.ParseInt(args.ElementAt(0).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("media.increaseVolume", (args, _) => {
                mediaManager.IncreaseVolume(args.Count() > 0 ? ParseHelper.ParseInt(args.ElementAt(0).ToString()) : 10);
                return string.Empty;
            });

            pluginManager.ExportFunction("media.decreaseVolume", (args, _) => {
                mediaManager.DecreaseVolume(args.Count() > 0 ? ParseHelper.ParseInt(args.ElementAt(0).ToString()) : 10);
                return string.Empty;     
            });

            pluginManager.ExportFunction("media.getBatteryLevel", (args, _) => {
                return mediaManager.GetBatteryLevel().ToString();
            });

            pluginManager.ExportFunction("res.loadImage", (args, _) => {
                if(args.Count() == 3){
                    Bitmap bitmap = new(Image.FromFile(config.GetPath(args.ElementAt(0).ToString())), ParseHelper.ParseInt(args.ElementAt(1).ToString()), ParseHelper.ParseInt(args.ElementAt(2).ToString()));
                    return resourceManager.AddResource(bitmap);
                }

                return resourceManager.AddResource(Image.FromFile(config.GetPath(args.ElementAt(0).ToString())));
            });

            pluginManager.ExportFunction("res.loadIcon", (args, _) => {
                var index = args.Count() > 1 ? ParseHelper.ParseInt(args.ElementAt(1).ToString()) : 0;
                return resourceManager.AddResource(Icon.FromHandle(ExtractIcon(IntPtr.Zero, args.ElementAt(0).ToString(), index)).ToBitmap());
            });
        }

        IntPtr GetIntPtr(object value){
            return new IntPtr(long.Parse(value.ToString()));
        }
    }
}
