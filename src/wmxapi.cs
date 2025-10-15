using System;
using System.IO;
using System.Diagnostics;
using static RedEye.Native;

namespace RedEye {
    public class WmxAPI {
        static readonly string msg64Name = "RedEye_Wmx64Msg";
        static readonly string msg32Name = "RedEye_Wmx32Msg";
        static readonly string wnd64Name = "RedEye_Wmx64Wnd";
        static readonly string wnd32Name = "RedEye_Wmx32Wnd";
        public static readonly string resMsgName = "RedEye_WmxResMsg";

        enum WmxParam {
            X,
            Y,
            Width,
            Height
        }

        public enum WmxResponse {
            Lang
        }

        static int msgNumber64 = -1;
        static int pid64 = -1;
        static IntPtr hWnd64 = IntPtr.Zero;

        static int msgNumber32 = -1;
        static int pid32 = -1;
        static IntPtr hWnd32 = IntPtr.Zero;

        public static void SetDesktopSize(int x, int y, int width, int height){
            if(pid64 == -1){
                var wmx = new ProcessStartInfo();
                wmx.FileName = Util.GetPath("wmx\\wmx64.exe");
                wmx.Arguments = $"{x} {y} {width} {height} {(Config.CurrentConfig.core.useWmxShellHook?1:0)}";
                wmx.WorkingDirectory = Util.GetPath("wmx");

                try{
                    var proc = Process.Start(wmx);
                    pid64 = proc.Id;
                }catch(Exception ex){
                    Logger.Log(Logger.MessageType.Critical, "Failed to start WMX64: " + ex.Message);
                }

                msgNumber64 = RegisterWindowMessage(msg64Name);
            }else{
                if(hWnd64 == IntPtr.Zero){
                    hWnd64 = FindWindowEx(HWND_MESSAGE, IntPtr.Zero, wnd64Name, IntPtr.Zero);
                }

                SendMessage(hWnd64, msgNumber64, (int)WmxParam.X, x);
                SendMessage(hWnd64, msgNumber64, (int)WmxParam.Y, y);
                SendMessage(hWnd64, msgNumber64, (int)WmxParam.Width, width);
                SendMessage(hWnd64, msgNumber64, (int)WmxParam.Height, height);

                Native.EnumWindows((hWnd, lParam)=>{
                    SendMessage(hWnd, msgNumber64, (int)WmxParam.X, x);
                    SendMessage(hWnd, msgNumber64, (int)WmxParam.Y, y);
                    SendMessage(hWnd, msgNumber64, (int)WmxParam.Width, width);
                    SendMessage(hWnd, msgNumber64, (int)WmxParam.Height, height);
                    return true;
                }, IntPtr.Zero);
            }

            if(!File.Exists(Util.GetPath("wmx\\wmx32.exe"))){
                return;
            }

            if(pid32 == -1){
                var wmx = new ProcessStartInfo();
                wmx.FileName = Util.GetPath("wmx\\wmx32.exe");
                wmx.Arguments = $"{x} {y} {width} {height}";
                wmx.WorkingDirectory = Util.GetPath("wmx");

                try{
                    var proc = Process.Start(wmx);
                    pid32 = proc.Id;
                }catch(Exception ex){
                    Logger.Log(Logger.MessageType.Critical, "Failed to start WMX32: " + ex.Message);
                }

                msgNumber32 = RegisterWindowMessage(msg32Name);
            }else{
                if(hWnd32 == IntPtr.Zero){
                    hWnd32 = FindWindowEx(HWND_MESSAGE, IntPtr.Zero, wnd32Name, IntPtr.Zero);
                }

                SendMessage(hWnd32, msgNumber32, (int)WmxParam.X, x);
                SendMessage(hWnd32, msgNumber32, (int)WmxParam.Y, y);
                SendMessage(hWnd32, msgNumber32, (int)WmxParam.Width, width);
                SendMessage(hWnd32, msgNumber32, (int)WmxParam.Height, height);

                Native.EnumWindows((hWnd, lParam)=>{
                    SendMessage(hWnd, msgNumber32, (int)WmxParam.X, x);
                    SendMessage(hWnd, msgNumber32, (int)WmxParam.Y, y);
                    SendMessage(hWnd, msgNumber32, (int)WmxParam.Width, width);
                    SendMessage(hWnd, msgNumber32, (int)WmxParam.Height, height);
                    return true;
                }, IntPtr.Zero);
            }
        }

        public static void Exit(){
            if(hWnd64 != IntPtr.Zero) SendMessage(hWnd64, WM_CLOSE, 0, 0);
            if(hWnd32 != IntPtr.Zero) SendMessage(hWnd32, WM_CLOSE, 0, 0);
        }
    }
}
