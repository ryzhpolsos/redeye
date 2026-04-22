using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ShellEventListenerComponent : IShellEventListener {
        ComponentManager manager = null;
        IConfig config = null;
        ILogger logger = null;
        IWindowManager windowManager = null;
        IExplorerIntegration explorerIntegration = null;

        Dictionary<IntPtr, IntPtr> windowWrappers = new();
        Dictionary<IntPtr, ShellWindowState> activeWindows = new();
        List<IntPtr> ignoreHandles = new();
        List<Func<ShellWindowEvent, ShellWindowState, bool>> eventHandlers = new();
        int shellMsg = 0;
        bool listenerStarted = false;
        Icon defaultIcon = null;
        RECT workArea = new();
        MINIMIZEDMETRICS mm = new();

        int taskbarCreatedMsg = RegisterWindowMessage("TaskbarCreated");

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            config = manager.GetComponent<IConfig>();
            logger = manager.GetComponent<ILogger>();
            windowManager = manager.GetComponent<IWindowManager>();
            explorerIntegration = manager.GetComponent<IExplorerIntegration>();

            SetDefaultIcon("imageres.dll", 2);
        }

        public void AddIgnoredHandle(IntPtr handle){
            ignoreHandles.Add(handle);
        }

        public void SetWorkArea(int x, int y, int width, int height){
            workArea.left = x;
            workArea.top = y;
            workArea.right = x + width;
            workArea.bottom = y + height;
            SystemParametersInfo(SPI_SETWORKAREA, 0, ref workArea, 0);
        }

        public void ReSetWorkArea(){
            SystemParametersInfo(SPI_SETWORKAREA, 0, ref workArea, 0);
        }

        public void SetMinimizedMetrics(){
            if(mm.cbSize == 0){
                mm.cbSize = Marshal.SizeOf(typeof(MINIMIZEDMETRICS));
                mm.iWidth = 0;
                mm.iHorzGap = 0;
                mm.iVertGap = 0;
                mm.iArrange = ARW_HIDE;
            }

            SystemParametersInfo(SPI_SETMINIMIZEDMETRICS, 0, ref mm, SPIF_SENDCHANGE);
        }

        public void SetDefaultIcon(string fileName, int id){
            if(defaultIcon is not null) DestroyIcon(defaultIcon.Handle);

            var hIcon = ExtractIcon(IntPtr.Zero, fileName, id); 
            defaultIcon = Icon.FromHandle(hIcon);
        }

        public void RegisterEventHandler(Func<ShellWindowEvent, ShellWindowState, bool> handler){
            eventHandlers.Add(handler);

            foreach(var wnd in activeWindows.Values){
                handler.Invoke(ShellWindowEvent.Create, wnd);
            }

            if(!listenerStarted) StartListener();
        }

        public IEnumerable<ShellWindowState> GetWindows(){
            return activeWindows.Values;
        }

        public void TriggerEvent(ShellWindowEvent et, IntPtr hWnd){
            ProcessEvent(et, hWnd);
        }

        public bool HasWindow(IntPtr hWnd){
            return activeWindows.ContainsKey(hWnd);
        }

        public void ToggleWindow(IntPtr hWnd){
            var wp = new WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            GetWindowPlacement(hWnd, ref wp);

            if(activeWindows[hWnd].IsActive){
                windowManager.GetWindow(hWnd).Minimize();
            }else if(wp.showCmd == 2){
                windowManager.GetWindow(hWnd).Restore();
            }else{
                windowManager.GetWindow(hWnd).Activate();
            }
        }

        void ProcessEvent(ShellWindowEvent et, IntPtr hWnd){
            ShellWindowState wnd = null;

            switch(et){
                case ShellWindowEvent.Create: {
                    var wp = new WINDOWPLACEMENT();
                    wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    GetWindowPlacement(hWnd, ref wp);

                    wnd = new ShellWindowState(){ Handle = hWnd, IsMinimized = wp.showCmd == 2, ShowCmd = wp.showCmd, Title = GetWindowText(hWnd), Icon = GetWindowIcon(hWnd), IsActive = GetForegroundWindow() == hWnd };
                    activeWindows.Add(hWnd, wnd);

                    break;
                }

                case ShellWindowEvent.Minimize:
                case ShellWindowEvent.Restore: {
                    wnd = activeWindows[hWnd];
                    var wp = new WINDOWPLACEMENT();
                    wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    GetWindowPlacement(hWnd, ref wp);
                    var evt = wnd.IsMinimized ? ShellWindowEvent.Restore : ShellWindowEvent.Minimize;
                    wnd.IsMinimized = !wnd.IsMinimized;
                    wnd.ShowCmd = wp.showCmd;
                    wnd.IsActive = GetForegroundWindow() == wnd.Handle;

                    break;
                }

                case ShellWindowEvent.Redraw: {
                    var wp = new WINDOWPLACEMENT();
                    wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    GetWindowPlacement(hWnd, ref wp);

                    wnd = activeWindows[hWnd];
                    wnd.Title = GetWindowText(hWnd);
                    wnd.Icon = GetWindowIcon(hWnd);
                    wnd.ShowCmd = wp.showCmd;
                    wnd.IsActive = GetForegroundWindow() == wnd.Handle;

                    break;
                }

                case ShellWindowEvent.Activate: {
                    var wp = new WINDOWPLACEMENT();
                    wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    GetWindowPlacement(hWnd, ref wp);

                    wnd = activeWindows[hWnd];
                    wnd.Title = GetWindowText(hWnd);
                    wnd.Icon = GetWindowIcon(hWnd);
                    wnd.ShowCmd = wp.showCmd;
                    wnd.IsActive = true;
                    //logger.Log(logger.MessageType.Information, "activate wnddow");

                    foreach(var win in activeWindows){
                        if(win.Key != hWnd){
                            win.Value.IsActive = false;
                            ProcessEvent(ShellWindowEvent.Deactivate, win.Value.Handle);
                        }
                    }

                    break;
                }

                default: {
                    wnd = activeWindows[hWnd];
                    break;
                }
            }

            foreach(var handler in eventHandlers){
                if(!handler.Invoke(et, wnd)) break;
            }
        }

        void StartListener(){
            listenerStarted = true;

            logger.LogInformation("Shell event listener started");

            shellMsg = RegisterWindowMessage("SHELLHOOK");

            EnumWindows(EnumWindowsHandler, IntPtr.Zero);

            Thread thread = new(()=>{
                try{
                    var wndClass = new WNDCLASSEX();
                    wndClass.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
                    wndClass.hInstance = GetModuleHandle(IntPtr.Zero);
                    wndClass.lpszClassName = "RedEye_ShellWnd";
                    wndClass.lpfnWndProc = MsgWndProc;

                    if(RegisterClassEx(ref wndClass) == 0){
                        logger.LogFatal("ShellWindow class registration failed, last error: " + Marshal.GetLastWin32Error().ToString());
                    }

                    IntPtr hWnd = CreateWindowEx(0, wndClass.lpszClassName, "UwU", 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);
                    // IntPtr hWnd = CreateWindowEx(0, wndClass.lpszClassName, "UwU", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);

                    if(hWnd == IntPtr.Zero){
                        logger.LogFatal("ShellWindow creation failed, last error: " + Marshal.GetLastWin32Error().ToString());
                    }


                    if(!explorerIntegration.GetIsEnabled()){
                        logger.LogDebug("Explorer integration disabled, activating minimizied metrics");
                        SetTaskmanWindow(hWnd);
                        SetMinimizedMetrics();
                    }

                    // SetShellWindow(hWnd);

                    if(!RegisterShellHookWindow(hWnd)){
                        logger.LogFatal("Shell hook registration failed, last error: " + Marshal.GetLastWin32Error().ToString());
                    }

                    // SetWorkArea(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    
                    var msg = new MSG();
                    while(GetMessage(ref msg, IntPtr.Zero, 0, 0)){
                        DispatchMessage(ref msg);
                    }
                }catch(Exception ex){
                    logger.LogFatal($"ShellEventListener crashed: {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
                }
            });

            thread.Start();
        }

        int MsgWndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam){
            foreach(var handle in activeWindows.Keys.ToArray()){
                if(handle == hWnd || IsWindow(handle)) continue;
                ProcessEvent(ShellWindowEvent.Destroy, handle);
                activeWindows.Remove(handle);
            }

            if(uMsg == shellMsg){
                switch((int)wParam){
                    case HSHELL_WINDOWCREATED: {
                        if(IsWindowNonShell(lParam) && IsWindowTopLevel(lParam)){
                            ProcessEvent(ShellWindowEvent.Create, lParam);
                        }
                        break;
                    }
                    case HSHELL_WINDOWDESTROYED: {
                        if(IsWindowNonShell(lParam) && IsWindowTopLevel(lParam) && activeWindows.ContainsKey(lParam)){
                            //logger.Log(logger.MessageType.Information, "destroy window");
                            ProcessEvent(ShellWindowEvent.Destroy, lParam);
                            activeWindows.Remove(lParam);
                        }
                        break;
                    }
                    case HSHELL_GETMINRECT: {
                        var hookInfo = Marshal.PtrToStructure<SHELLHOOKINFO>(lParam);
                        var hwnd = hookInfo.hwnd;

                        if(IsWindowNonShell(hwnd) && activeWindows.ContainsKey(hwnd)){
                            var evt = activeWindows[hwnd].IsMinimized ? ShellWindowEvent.Restore : ShellWindowEvent.Minimize;
                            //logger.Log(logger.MessageType.Information, "getminrect window");
                            ProcessEvent(evt, hwnd);
                        }
                        break;
                    }
                    case HSHELL_REDRAW: {
                        if(IsWindowNonShell(lParam) && IsWindowTopLevel(lParam) && activeWindows.ContainsKey(lParam)){
                            ProcessEvent(ShellWindowEvent.Redraw, lParam);
                        }
                        break;
                    }
                    case HSHELL_WINDOWACTIVATED:
                    case HSHELL_RUDEAPPACTIVATED: {
                        if(IsWindowNonShell(lParam) && activeWindows.ContainsKey(lParam)){
                            ProcessEvent(ShellWindowEvent.Activate, lParam); 
                        }
                        break;
                    }
                }

                return 1;
            }else{
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
            }
        }

        int EnumWindowsHandler(IntPtr hWnd, IntPtr lParam){
            // if(explorerIntegration.GetIsEnabled()){
            //     GetWindowThreadProcessId(hWnd, out int pid);

            //     if(pid == explorerIntegration.GetExplorerPID()){
            //         return 1;
            //     }
            // }

            if(!IsWindowVisible(hWnd) || !IsWindowTopLevel(hWnd) || !IsWindowNonShell(hWnd)) return 1;

            string txt = GetWindowText(hWnd);
            string className = GetWindowClass(hWnd);
            if(txt != "WorkerW" && className != "ApplicationFrameWindow"){
                ProcessEvent(ShellWindowEvent.Create, hWnd);
            }

            return 1;
        }

        public void SendLayoutChange(int hkl){
            var localeName = new StringBuilder();
            LCIDToLocaleName(hkl, localeName, LOCALE_NAME_MAX_LENGTH, 0);

            var lang = new StringBuilder();
            GetLocaleInfoEx(localeName.ToString(), LOCALE_SISO639LANGNAME2, lang, 8);

            // ProcessEvent(ShellWindowEvent.LayoutChange, new ShellWindowState(){ Data = lang.ToString(), Handle = (IntPtr)hkl });
            //logger.Log(logger.MessageType.Information, "SendLayoutChange called");
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
        
        public Icon GetWindowIcon(IntPtr hWnd){
            SendMessageTimeout(hWnd, WM_GETICON, ICON_BIG, 0, 0, 200, out IntPtr iconHandle);

            if(iconHandle == IntPtr.Zero) SendMessageTimeout(hWnd, WM_GETICON, ICON_SMALL, 0, 0, 200, out iconHandle);
            if(iconHandle == IntPtr.Zero) SendMessageTimeout(hWnd, WM_GETICON, ICON_SMALL2, 0, 0, 200, out iconHandle);
            if(iconHandle == IntPtr.Zero) iconHandle = GetClassPtr(hWnd, GCL_HICON);
            if(iconHandle == IntPtr.Zero) iconHandle = GetClassPtr(hWnd, GCL_HICONSM);

            if(iconHandle == IntPtr.Zero) return defaultIcon;
            return Icon.FromHandle(iconHandle);
        }

        bool IsWindowTopLevel(IntPtr hWnd){
            // if(!IsTopLevelWindow(hWnd)) return false;

            var className = GetWindowClass(hWnd);

            if(className.Length > 0 && className[0] == '#'){
                return className == "#32770";
            }

            long style = GetWindowLongPtr(hWnd, GWL_STYLE);
            long exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE);

            return ((style & WS_POPUP) != 0) || ((style & WS_CAPTION) != 0 && (exStyle & WS_EX_OVERLAPPEDWINDOW) != 0);
        }

        bool IsWindowNonShell(IntPtr hWnd){
            if(ignoreHandles.Contains(hWnd)) return false;

            GetWindowThreadProcessId(hWnd, out int pid);
            if(explorerIntegration.GetIsEnabled() && pid == explorerIntegration.GetExplorerPID() && GetWindowText(hWnd).Length == 0) return false;

            return true;
        }

        bool IsWindowMinimized(IntPtr hWnd){
            return (GetWindowLongPtr(hWnd, GWL_STYLE) & WS_MINIMIZE) != 0;
        }
    }
}
