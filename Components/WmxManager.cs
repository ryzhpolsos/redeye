using System;
using System.IO;
using System.Diagnostics;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class WmxManagerComponent : IWmxManager {
        ComponentManager manager = null;
        ILogger logger = null;
        IConfig config = null;

        readonly string msg64Name = "RedEye_Wmx64Msg";
        readonly string wnd64Name = "RedEye_Wmx64Wnd";
        readonly string resMsgName = "RedEye_WmxResMsg";

        int msgNumber64 = -1;
        IntPtr hWnd64 = IntPtr.Zero;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            logger = manager.GetComponent<ILogger>();
            config = manager.GetComponent<IConfig>();
        }

        public void SetWorkArea(int x, int y, int width, int height){
            if(config.GetRootNode()["config"]["core"].TryGetNode("disableWmx", out _)){
                logger.LogDebug($"[FakeWMX] SetDesktopBounds({x}, {y}, {width}, {height})");
                return;
            }

            if(hWnd64 == IntPtr.Zero){
                hWnd64 = FindWindowEx(HWND_MESSAGE, IntPtr.Zero, wnd64Name, IntPtr.Zero);

                if(hWnd64 == IntPtr.Zero){
                    var wmx = new ProcessStartInfo();
                    wmx.FileName = Path.Combine(config.GetAppDirectory(), "wmx", "wmx64.exe");
                    wmx.Arguments = $"{x} {y} {width} {height} {((config.GetRootNode()["config"]["core"]["useWmxShellHook"].GetValue() == "true") ? 1 : 0)}";
                    wmx.WorkingDirectory = Path.Combine(config.GetAppDirectory(), "wmx");

                    try{
                        Process.Start(wmx);
                    }catch(Exception ex){
                        logger.LogFatal("Failed to start WMX: " + ex.Message);
                        return;
                    }

                    msgNumber64 = RegisterWindowMessage(msg64Name);
                    return;
                }
            }
            
            SendMessage(hWnd64, msgNumber64, (int)WmxParam.X, x);
            SendMessage(hWnd64, msgNumber64, (int)WmxParam.Y, y);
            SendMessage(hWnd64, msgNumber64, (int)WmxParam.Width, width);
            SendMessage(hWnd64, msgNumber64, (int)WmxParam.Height, height);

            EnumWindows((hWnd, lParam)=>{
                SendMessage(hWnd, msgNumber64, (int)WmxParam.X, x);
                SendMessage(hWnd, msgNumber64, (int)WmxParam.Y, y);
                SendMessage(hWnd, msgNumber64, (int)WmxParam.Width, width);
                SendMessage(hWnd, msgNumber64, (int)WmxParam.Height, height);
                return true;
            }, IntPtr.Zero);
        }

        public void Exit(){
            if(hWnd64 != IntPtr.Zero) SendMessage(hWnd64, WM_CLOSE, 0, 0);
        }

        public string GetResponseMessage(){
            return resMsgName;
        }
    }
}
