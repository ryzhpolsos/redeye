using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ShellEventListenerComponent : IShellEventListener {
        ComponentManager manager = null;
        IConfig config = null;
        ILogger logger = null;
        IWmxManager wmxManager = null;

        Dictionary<IntPtr, ShellWindowState> activeWindows = new();
        List<IntPtr> ignoreHandles = new();
        List<Action<ShellWindowEvent, ShellWindowState>> eventHandlers = new();
        int shellMsg = 0;
        int wmxMsg = 0;
        bool listenerStarted = false;
        Icon defaultIcon = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            config = manager.GetComponent<IConfig>();
            logger = manager.GetComponent<ILogger>();
            wmxManager = manager.GetComponent<IWmxManager>();

            SetDefaultIcon("imageres.dll", 2);
        }

        public void AddIgnoredHandle(IntPtr handle){
            ignoreHandles.Add(handle);
        }

        public void SetDefaultIcon(string fileName, int id){
            if(defaultIcon is not null) DestroyIcon(defaultIcon.Handle);

            var hIcon = ExtractIcon(IntPtr.Zero, fileName, id); 
            defaultIcon = Icon.FromHandle(hIcon);
        }

        public void RegisterEventHandler(Action<ShellWindowEvent, ShellWindowState> handler){
            eventHandlers.Add(handler);

            foreach(var wnd in activeWindows.Values){
                handler.Invoke(ShellWindowEvent.Create, wnd);
            }

            if(!listenerStarted) StartListener();
        }

        public void ToggleWindow(IntPtr handle){
            if(!activeWindows.ContainsKey(handle)) throw new KeyNotFoundException();
            var window = activeWindows[handle];

            if(window.IsActive){
                MinimizeWindow(handle);
            }else if(window.IsMinimized){
                RestoreWindow(handle);
            }else{
                ActivateWindow(handle);
            }
        }

        void ProcessEvent(ShellWindowEvent et, ShellWindowState wnd){
            foreach(var handler in eventHandlers){
                handler.Invoke(et, wnd);
            }
        }

        void StartListener(){
            listenerStarted = true;

            shellMsg = RegisterWindowMessage("SHELLHOOK");
            wmxMsg = RegisterWindowMessage(wmxManager.GetResponseMessage());

            Task.Run(()=>{
                try{
                    EnumWindows(EnumWindowsHandler, IntPtr.Zero);

                    var wndClass = new WNDCLASSEX();
                    wndClass.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
                    wndClass.hInstance = GetModuleHandle(IntPtr.Zero);
                    wndClass.lpszClassName = "RedEye_ShellWnd";
                    wndClass.lpfnWndProc = MsgWndProc;

                    if(RegisterClassEx(ref wndClass) == 0){
                        logger.LogFatal("ShellWindowState class registration failed, last error: " + Marshal.GetLastWin32Error().ToString());
                    }

                    IntPtr hWnd = CreateWindowEx(0, wndClass.lpszClassName, "UwU", 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);

                    if(hWnd == IntPtr.Zero){
                        logger.LogFatal("ShellWindowState creation failed, last error: " + Marshal.GetLastWin32Error().ToString());
                    }

                    var mm = new MINIMIZEDMETRICS();
                    mm.cbSize = Marshal.SizeOf(typeof(MINIMIZEDMETRICS));
                    mm.iWidth = 0;
                    mm.iHorzGap = 0;
                    mm.iVertGap = 0;
                    mm.iArrange = ARW_HIDE;
                    SystemParametersInfo(SPI_SETMINIMIZEDMETRICS, 0, ref mm, SPIF_SENDCHANGE);

                    if(!ParseHelper.ParseBool(config.GetRootNode()["config"]["core"]["useWmxShellHook"].GetValue())){
                        //SetShellWindow(hWnd);
                        SetTaskmanWindow(hWnd);

                        if(!RegisterShellHookWindow(hWnd)){
                            logger.LogFatal("Shell hook registration failed, last error: " + Marshal.GetLastWin32Error().ToString());
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
                }catch(Exception ex){
                    logger.LogFatal($"ShellEventListener crashed: {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        int MsgWndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam){
            foreach(var handle in activeWindows.Keys.ToArray()){
                if(handle == hWnd || IsWindow(handle)) continue;
                ProcessEvent(ShellWindowEvent.Destroy, activeWindows[handle]);
                activeWindows.Remove(handle);
            }

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
                            //logger.Log(logger.MessageType.Information, "destroy window");
                            ProcessEvent(ShellWindowEvent.Destroy, activeWindows[lParam]);
                            activeWindows.Remove(lParam);
                        }
                        break;
                    }
                    case HSHELL_GETMINRECT: {
                        IntPtr hwnd;
                        if(config.GetRootNode()["config"]["core"]["useWmxShellHook"].GetValue() == "true"){
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
                            var evt = win.IsMinimized ? ShellWindowEvent.Restore : ShellWindowEvent.Minimize;
                            win.IsMinimized = !win.IsMinimized;
                            win.ShowCmd = wp.showCmd;
                            win.IsActive = GetForegroundWindow() == win.Handle;
                            //logger.Log(logger.MessageType.Information, "getminrect window");
                            ProcessEvent(evt, win);
                        }
                        break;
                    }
                    case HSHELL_REDRAW: {
                        if(IsWindowNonShell(lParam) && IsWindowTopLevel(lParam) && activeWindows.ContainsKey(lParam)){
                            var wp = new WINDOWPLACEMENT();
                            wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                            GetWindowPlacement(lParam, ref wp);

                            var win = activeWindows[lParam];
                            win.Title = GetWindowText(lParam);
                            win.Icon = GetWindowIcon(lParam);
                            win.ShowCmd = wp.showCmd;
                            win.IsActive = GetForegroundWindow() == win.Handle;
                            //logger.Log(logger.MessageType.Information, "redraw window");
                            ProcessEvent(ShellWindowEvent.Redraw, win);
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
                            win.Title = GetWindowText(lParam);
                            win.Icon = GetWindowIcon(lParam);
                            win.ShowCmd = wp.showCmd;
                            win.IsActive = true;
                            //logger.Log(logger.MessageType.Information, "activate window");
                            ProcessEvent(ShellWindowEvent.Activate, win);

                            foreach(var wnd in activeWindows){
                                if(wnd.Key != lParam){
                                    wnd.Value.IsActive = false;
                                    ProcessEvent(ShellWindowEvent.Deactivate, wnd.Value);
                                }
                            }
                        }
                        break;
                    }
                }

                return 1;
            }else if(uMsg == wmxMsg){
                switch((WmxResponse)wParam){
                    case WmxResponse.Lang: {
                        SendLayoutChange((int)lParam);
                        break;
                    }
                }
            }else{
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
            }

            return 0;
        }

        bool EnumWindowsHandler(IntPtr hWnd, IntPtr lParam){
            string txt = GetWindowText(hWnd);
            if(IsWindowVisible(hWnd) && IsWindowTopLevel(hWnd) && txt.Length != 0 && txt != "WorkerW"){
                AddWindow(hWnd);
            }

            return true;
        }

        void AddWindow(IntPtr hWnd){
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            GetWindowPlacement(hWnd, ref wp);

            var wnd = new ShellWindowState(){ Handle = hWnd, IsMinimized = wp.showCmd == 2, ShowCmd = wp.showCmd, Title = GetWindowText(hWnd), Icon = GetWindowIcon(hWnd), IsActive = GetForegroundWindow() == hWnd };
            activeWindows.Add(hWnd, wnd);
            //logger.Log(logger.MessageType.Information, "create window");
            ProcessEvent(ShellWindowEvent.Create, wnd);
        }

        public void SendLayoutChange(int hkl){
            var localeName = new StringBuilder();
            LCIDToLocaleName(hkl, localeName, LOCALE_NAME_MAX_LENGTH, 0);

            var lang = new StringBuilder();
            GetLocaleInfoEx(localeName.ToString(), LOCALE_SISO639LANGNAME2, lang, 8);

            ProcessEvent(ShellWindowEvent.LayoutChange, new ShellWindowState(){ Data = lang.ToString(), Handle = (IntPtr)hkl });
            //logger.Log(logger.MessageType.Information, "SendLayoutChange called");
        }

        Icon GetWindowIcon(IntPtr hWnd){
            IntPtr iconHandle = SendMessage(hWnd, WM_GETICON, ICON_BIG, 0);
            if(iconHandle == IntPtr.Zero) iconHandle = SendMessage(hWnd, WM_GETICON, ICON_SMALL, 0);
            if(iconHandle == IntPtr.Zero) iconHandle = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, 0);
            if(iconHandle == IntPtr.Zero) iconHandle = GetClassPtr(hWnd, GCL_HICON);
            if(iconHandle == IntPtr.Zero) iconHandle = GetClassPtr(hWnd, GCL_HICONSM);

            if(iconHandle == IntPtr.Zero) return defaultIcon;
            return Icon.FromHandle(iconHandle);
        }

        string GetWindowClass(IntPtr h){
            var buff = new StringBuilder(255);
            GetClassName(h, buff, buff.Capacity);
            return buff.ToString();
        }

        public IntPtr GetClassPtr(IntPtr hWnd, int nIndex){
            if(IntPtr.Size > 4) return GetClassLongPtr(hWnd, nIndex);
            return new IntPtr(GetClassLong(hWnd, nIndex));
        }

        string GetWindowText(IntPtr h){
            int len = SendMessage(h, 0xE, 0L, 0L)+1;
            StringBuilder buff = new StringBuilder(len);
            SendMessage(h, 0xD, len, buff);
            return buff.ToString();
        }

        bool IsWindowTopLevel(IntPtr hWnd){
            if(hWnd == GetAncestor(hWnd, GA_ROOT)){
                long style = GetWindowLongPtr(hWnd, GWL_EXSTYLE);
                return (style & WS_EX_OVERLAPPEDWINDOW) != 0;
            }else{
                return false;
            }
        }

        bool IsWindowNonShell(IntPtr hWnd){
            return !ignoreHandles.Contains(hWnd);
        }

        bool IsWindowMinimized(IntPtr hWnd){
            return (GetWindowLongPtr(hWnd, GWL_STYLE) & WS_MINIMIZE) != 0;
        }
    }
}
