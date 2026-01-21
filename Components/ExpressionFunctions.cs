using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ExpressionFunctionsComponent : IExpressionFunctions {
        ComponentManager manager = null;

        IShellWindowManager shellWindowManager = null;
        IShellEventListener shellEventListener = null;
        IResourceManager resourceManager = null;
        IPluginManager pluginManager = null;
        IMediaManager mediaManager = null;
        IWmxManager wmxManager = null;
        IConfig config = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            shellWindowManager = manager.GetComponent<IShellWindowManager>();
            shellEventListener = manager.GetComponent<IShellEventListener>();
            resourceManager = manager.GetComponent<IResourceManager>();
            pluginManager = manager.GetComponent<IPluginManager>();
            mediaManager = manager.GetComponent<IMediaManager>();
            wmxManager = manager.GetComponent<IWmxManager>();
            config = manager.GetComponent<IConfig>();

            pluginManager.ExportFunction("eval", args => {
                return EvalHelper.Eval(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("calc", args => {
                return EvalHelper.Eval(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("if", args => {
                if(ParseHelper.ParseBool(EvalHelper.Eval(args.ElementAt(0).ToString()))){
                    return args.ElementAt(1).ToString();
                }else if(args.Count() > 2){
                    return args.ElementAt(2).ToString();
                }

                return string.Empty;
            });

            pluginManager.ExportFunction("concat", args => {
                return string.Join("", args.Select(s => s.ToString()));
            });

            pluginManager.ExportFunction("join", args => {
                return string.Join(args.ElementAt(0).ToString(), args.Skip(1).Select(s => s.ToString()));
            });

            pluginManager.ExportFunction("showMessage", args => {
                Task.Run(() => MessageBox.Show(args.ElementAt(0).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("dateTime", args => {
                return DateTime.Now.ToString(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("expand", args => {
                return Environment.ExpandEnvironmentVariables(args.ElementAt(0).ToString());
            });

            pluginManager.ExportFunction("wmx.setWorkArea", args => {
                wmxManager.SetWorkArea(ParseHelper.ParseInt(args.ElementAt(0).ToString()), ParseHelper.ParseInt(args.ElementAt(1).ToString()), ParseHelper.ParseInt(args.ElementAt(2).ToString()), ParseHelper.ParseInt(args.ElementAt(3).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.minimizeWindow", args => {
                MinimizeWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.restoreWindow", args => {
                RestoreWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.activateWindow", args => {
                ActivateWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.toggleWindow", args => {
                shellEventListener.ToggleWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("wapi.closeWindow", args => {
                CloseWindow(GetIntPtr(args.ElementAt(0)));
                return string.Empty;
            });

            pluginManager.ExportFunction("window.show", args => {
                shellWindowManager.GetWindow(args.ElementAt(0).ToString()).ShowWindow();
                return string.Empty;
            });

            pluginManager.ExportFunction("window.hide", args => {
                shellWindowManager.GetWindow(args.ElementAt(0).ToString()).HideWindow();
                return string.Empty;
            });

            pluginManager.ExportFunction("window.toggle", args => {
                shellWindowManager.GetWindow(args.ElementAt(0).ToString()).ToggleWindow();
                return string.Empty;
            });

            pluginManager.ExportFunction("media.getBrightness", args => {
                return mediaManager.GetBrightness().ToString();
            });

            pluginManager.ExportFunction("media.setBrightness", args => {
                mediaManager.SetBrightness(ParseHelper.ParseInt(args.ElementAt(0).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("media.getVolume", args => {
                return mediaManager.GetVolume().ToString();        
            });

            pluginManager.ExportFunction("media.setVolume", args => {
                mediaManager.SetVolume(ParseHelper.ParseInt(args.ElementAt(0).ToString()));
                return string.Empty;
            });

            pluginManager.ExportFunction("media.getBatteryLevel", args => {
                return mediaManager.GetBatteryLevel().ToString();
            });

            pluginManager.ExportFunction("res.loadImage", args => {
                if(args.Count() == 3){
                    Bitmap bitmap = new(Image.FromFile(config.GetPath(args.ElementAt(0).ToString())), ParseHelper.ParseInt(args.ElementAt(1).ToString()), ParseHelper.ParseInt(args.ElementAt(2).ToString()));
                    return resourceManager.AddResource(bitmap);
                }

                return resourceManager.AddResource(Image.FromFile(config.GetPath(args.ElementAt(0).ToString())));
            });

            pluginManager.ExportFunction("res.loadIcon", args => {
                var index = args.Count() > 1 ? ParseHelper.ParseInt(args.ElementAt(1).ToString()) : 0;
                return resourceManager.AddResource(Icon.FromHandle(ExtractIcon(IntPtr.Zero, args.ElementAt(0).ToString(), index)).ToBitmap());
            });
        }

        IntPtr GetIntPtr(object value){
            return new IntPtr(long.Parse(value.ToString()));
        }
    }
}
