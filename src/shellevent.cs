using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using static RedEye.Native;

namespace RedEye {
    class ShellEventListener {
        public enum EventType {
            Create,
            Destroy,
            Minimize,
            Restore,
            Redraw,
            Activate,
            LayoutChange
        }

        public class ShellWnd {
            public IntPtr handle;
            public bool isMinimized = false;
            public bool isActive = false;
            public int showCmd = 1;
            public string title = "";
            public string icon = "";
            public string data = "";
        }

        public static List<IntPtr> IgnoreHandles = new List<IntPtr>();

        public delegate void ShellEventHandler(EventType et, ShellWnd wnd);

        static Dictionary<IntPtr, ShellWnd> activeWindows = new Dictionary<IntPtr, ShellWnd>();
        static int shellMsg = 0;
        static int wmxMsg = 0;
        static ShellEventHandler handler = null;

        public static void Start(ShellEventHandler hnd){
            handler = hnd;

            shellMsg = RegisterWindowMessage("SHELLHOOK");
            wmxMsg = RegisterWindowMessage(WmxAPI.resMsgName);

            Task.Run(()=>{
                EnumWindows(EnumWindowsHandler, IntPtr.Zero);

                var wndClass = new WNDCLASSEX();
                wndClass.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
                wndClass.hInstance = GetModuleHandle(IntPtr.Zero);
                wndClass.lpszClassName = "RedEye_ShellWnd";
                wndClass.lpfnWndProc = MsgWndProc;

                if(RegisterClassEx(ref wndClass) == 0){
                    Logger.Log(Logger.MessageType.Critical, "ShellWnd class registration failed, last error: " + Marshal.GetLastWin32Error().ToString());
                }

                IntPtr hWnd = CreateWindowEx(0, wndClass.lpszClassName, "UwU", 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);

                if(hWnd == IntPtr.Zero){
                    Logger.Log(Logger.MessageType.Critical, "ShellWnd creation failed, last error: " + Marshal.GetLastWin32Error().ToString());
                }

                var mm = new MINIMIZEDMETRICS();
                mm.cbSize = Marshal.SizeOf(typeof(MINIMIZEDMETRICS));
                mm.iWidth = 0;
                mm.iHorzGap = 0;
                mm.iVertGap = 0;
                mm.iArrange = ARW_HIDE;
                SystemParametersInfo(SPI_SETMINIMIZEDMETRICS, 0, ref mm, SPIF_SENDCHANGE);

                if(!Config.CurrentConfig.core.useWmxShellHook){
                    //SetShellWindow(hWnd);
                    SetTaskmanWindow(hWnd);

                    if(!RegisterShellHookWindow(hWnd)){
                        Logger.Log(Logger.MessageType.Critical, "Shell hook registration failed, last error: " + Marshal.GetLastWin32Error().ToString());
                    }
                }

                var rcFull = new RECT();
                rcFull.left = 0;
                rcFull.top = 0;
                rcFull.right = Screen.PrimaryScreen.Bounds.Width;
                rcFull.right = Screen.PrimaryScreen.Bounds.Height;
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref rcFull, SPIF_SENDCHANGE);

                var msg = new MSG();
                while(GetMessage(ref msg, IntPtr.Zero, 0, 0)){
                    DispatchMessage(ref msg);
                }
            });
        }

        static int MsgWndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam){
            if(uMsg == WM_QUIT){
                DestroyWindow(hWnd);
                return 1;
            }else if(uMsg == shellMsg){
                switch((int)wParam){
                    case HSHELL_WINDOWCREATED: {
                        if(IsWindowNonShell(lParam) && IsWindowTopLevel(lParam)){
                            AddWindow(lParam);
                        }
                        break;
                    }
                    case HSHELL_WINDOWDESTROYED: {
                        if(IsWindowNonShell(lParam) && IsWindowTopLevel(lParam) && activeWindows.ContainsKey(lParam)){
                            //Logger.Log(Logger.MessageType.Information, "destroy window");
                            handler(EventType.Destroy, activeWindows[lParam]);
                            activeWindows.Remove(lParam);
                        }
                        break;
                    }
                    case HSHELL_GETMINRECT: {
                        IntPtr hwnd;
                        if(Config.CurrentConfig.core.useWmxShellHook){
                            hwnd = lParam;
                        }else{
                            var hookInfo = Marshal.PtrToStructure<SHELLHOOKINFO>(lParam);
                            hwnd = hookInfo.hwnd;
                        }

                        if(IsWindowNonShell(hwnd) && activeWindows.ContainsKey(hwnd)){
                            var win = activeWindows[hwnd];
                            var wp = new WINDOWPLACEMENT();
                            wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                            GetWindowPlacement(hwnd, ref wp);
                            var evt = win.isMinimized ? EventType.Restore : EventType.Minimize;
                            win.isMinimized = !win.isMinimized;
                            win.showCmd = wp.showCmd;
                            win.isActive = GetForegroundWindow() == win.handle;
                            Logger.Log(Logger.MessageType.Information, "getminrect window");
                            handler(evt, win);
                        }
                        break;
                    }
                    case HSHELL_REDRAW: {
                        if(IsWindowNonShell(lParam) && IsWindowTopLevel(lParam) && activeWindows.ContainsKey(lParam)){
                            var wp = new WINDOWPLACEMENT();
                            wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                            GetWindowPlacement(lParam, ref wp);

                            var win = activeWindows[lParam];
                            win.title = GetWindowText(lParam);
                            win.icon = GetWindowIcon(lParam);
                            win.showCmd = wp.showCmd;
                            win.isActive = GetForegroundWindow() == win.handle;
                            //Logger.Log(Logger.MessageType.Information, "redraw window");
                            handler(EventType.Redraw, win);
                        }
                        break;
                    }
                    case HSHELL_WINDOWACTIVATED:
                    case HSHELL_RUDEAPPACTIVATED: {
                        if(IsWindowNonShell(lParam) && activeWindows.ContainsKey(lParam)){
                            var wp = new WINDOWPLACEMENT();
                            wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                            GetWindowPlacement(lParam, ref wp);

                            var win = activeWindows[lParam];
                            win.title = GetWindowText(lParam);
                            win.icon = GetWindowIcon(lParam);
                            win.showCmd = wp.showCmd;
                            win.isActive = true;
                            //Logger.Log(Logger.MessageType.Information, "activate window");
                            handler(EventType.Activate, win);
                        }
                        break;
                    }
                }

                return 1;
            }else if(uMsg == wmxMsg){
                switch((WmxAPI.WmxResponse)wParam){
                    case WmxAPI.WmxResponse.Lang: {
                        SendLayoutChange((int)lParam);
                        break;
                    }
                }
            }else{
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
            }

            return 0;
        }

        static bool EnumWindowsHandler(IntPtr hWnd, IntPtr lParam){
            string txt = GetWindowText(hWnd);
            if(IsWindowVisible(hWnd) && IsWindowTopLevel(hWnd) && txt.Length != 0 && txt != "WorkerW"){
                AddWindow(hWnd);
            }

            return true;
        }

        static void AddWindow(IntPtr hWnd){
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            GetWindowPlacement(hWnd, ref wp);

            var wnd = new ShellWnd(){ handle = hWnd, isMinimized = wp.showCmd == 2, showCmd = wp.showCmd, title = GetWindowText(hWnd), icon = GetWindowIcon(hWnd), isActive = GetForegroundWindow() == hWnd };
            activeWindows.Add(hWnd, wnd);
            //Logger.Log(Logger.MessageType.Information, "create window");
            handler(EventType.Create, wnd);
        }

        public static void SendLayoutChange(int hkl){
            var localeName = new StringBuilder();
            LCIDToLocaleName(hkl, localeName, LOCALE_NAME_MAX_LENGTH, 0);

            var lang = new StringBuilder();
            GetLocaleInfoEx(localeName.ToString(), LOCALE_SISO639LANGNAME2, lang, 8);

            if(handler != null) handler(EventType.LayoutChange, new ShellWnd(){ data = lang.ToString(), handle = (IntPtr)hkl });
            Logger.Log(Logger.MessageType.Information, "SendLayoutChange called");
        }

        public static void CreateAllKnownWindows(){
            foreach(var kvp in activeWindows){
                handler(EventType.Create, kvp.Value);
            }
        }

        static string GetWindowIcon(IntPtr hWnd){
            IntPtr iconHandle = SendMessage(hWnd, WM_GETICON, ICON_BIG, 0);
            if(iconHandle == IntPtr.Zero) iconHandle = SendMessage(hWnd, WM_GETICON, ICON_SMALL, 0);
            if(iconHandle == IntPtr.Zero) iconHandle = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, 0);
            if(iconHandle == IntPtr.Zero) iconHandle = GetClassPtr(hWnd, GCL_HICON);
            if(iconHandle == IntPtr.Zero) iconHandle = GetClassPtr(hWnd, GCL_HICONSM);

            if(iconHandle == IntPtr.Zero) return "";
            
            var icn = Icon.FromHandle(iconHandle);
            var bitmap = icn.ToBitmap();
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        }

        static string GetWindowClass(IntPtr h){
            var buff = new StringBuilder(255);
            GetClassName(h, buff, buff.Capacity);
            return buff.ToString();
        }

        public static IntPtr GetClassPtr(IntPtr hWnd, int nIndex){
            if(IntPtr.Size > 4) return GetClassLongPtr(hWnd, nIndex);
            return new IntPtr(GetClassLong(hWnd, nIndex));
        }

        static string GetWindowText(IntPtr h){
            int len = SendMessage(h, 0xE, 0L, 0L)+1;
            StringBuilder buff = new StringBuilder(len);
            SendMessage(h, 0xD, len, buff);
            return buff.ToString();
        }

        static bool IsWindowTopLevel(IntPtr hWnd){
            if(hWnd == GetAncestor(hWnd, GA_ROOT)){
                long style = GetWindowLongPtr(hWnd, GWL_EXSTYLE);
                return (style & WS_EX_OVERLAPPEDWINDOW) != 0;
            }else{
                return false;
            }
        }

        static bool IsWindowNonShell(IntPtr hWnd){
            return !IgnoreHandles.Contains(hWnd);
        }

        static bool IsWindowMinimized(IntPtr hWnd){
            return (GetWindowLongPtr(hWnd, GWL_STYLE) & WS_MINIMIZE) != 0;
        }
    }
}