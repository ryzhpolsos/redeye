using System;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;

namespace RedEye {
    class Program {
        [STAThread]
        public static void Main(string[] args){
            if(args.Length > 0 && args[0] == "--killwmx"){
                var hWnd = Native.FindWindowEx(Native.HWND_MESSAGE, IntPtr.Zero, "RedEye_Wmx64Wnd", IntPtr.Zero);
                
                if(hWnd != IntPtr.Zero){
                    Native.SendMessage(hWnd, Native.WM_CLOSE, 0, 0);
                }

                return;
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Config.Load();

            Application.Run(new DskForm());
        }

        public static void Exit(){
            if(Logger.Writer != null){
                Logger.Writer.Flush();
                Logger.Writer.Close();
            }

            WmxAPI.Exit();
            Environment.Exit(0);
        }
    }
}
