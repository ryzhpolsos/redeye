using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ExplorerIntegrationComponent : IExplorerIntegration {
        ComponentManager manager = null;
        IConfig config = null;
        ILogger logger = null;
        IShellWindowManager shellWindowManager = null;
        IShellEventListener shellEventListener = null;

        readonly string trayWndClassName = "Shell_TrayWnd";
        readonly string progManClassName = "Progman";

        bool enabled = false;
        int timeout = 0;
        int pid = 0;

        int taskbarCreatedMsg = RegisterWindowMessage("TaskbarCreated");
        bool gotTaskbarCreated = false;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            config = manager.GetComponent<IConfig>();
            logger = manager.GetComponent<ILogger>();
            shellWindowManager = manager.GetComponent<IShellWindowManager>();
            shellEventListener = manager.GetComponent<IShellEventListener>();
        }

        public bool GetIsEnabled(){
            return enabled;
        }

        public void SetIsEnabled(bool enabled){
#if DEBUG
            bool _enabled = false;
#else
            bool _enabled = enabled;
#endif

            logger.LogInformation($"Explorer integration is {(_enabled ? "enabled" : "disabled")}");
            this.enabled = _enabled;
        }

        public void SetTimeout(int timeout){
            this.timeout = timeout;
        }

        public int GetExplorerPID(){
            return pid;
        }

        void ProcessWindow(IntPtr hWnd){
            // SetWindowPos(hWnd, IntPtr.Zero, -9999, -9999, 1, 1, SWP_NOZORDER);
            ShowWindow(hWnd, 0);
            EnableWindow(hWnd, 0);
            
            foreach(var msg in new int[]{ WM_KEYDOWN, WM_SYSKEYDOWN, WM_KEYUP, WM_SYSKEYUP }){
                ChangeWindowMessageFilterEx(hWnd, msg, MSGFLT_DISALLOW, IntPtr.Zero);
            }
        }

        public void RunHiddenExplorer(){
            Task.Run(async () => {
                if(Process.GetProcessesByName("explorer").Length != 0) return;
               
                Thread thread = new(ListenForTaskbarCreated);
                thread.Start();

                var proc = Process.Start("explorer.exe");
                proc.WaitForInputIdle();
                pid = proc.Id;

                await Task.Delay(timeout);

                if(!gotTaskbarCreated){
                    ProcessWindow(FindWindow(trayWndClassName, IntPtr.Zero));
                    ProcessWindow(FindWindow(progManClassName, IntPtr.Zero));
                }
            });

            Task.Run(async () => {
                while(true){
                    shellEventListener.ReSetWorkArea();
                    await Task.Delay(200);
                }
            });
        }

        void ListenForTaskbarCreated(){
            WNDCLASSEX wc = new();
            wc.cbSize = Marshal.SizeOf<WNDCLASSEX>();
            wc.hInstance = GetModuleHandle(IntPtr.Zero);
            wc.lpszClassName = "RedEye_ExplorerIntegrationListenerWnd";
            wc.lpfnWndProc = WndProc;

            RegisterClassEx(ref wc);

            IntPtr hWnd = CreateWindowEx(0, wc.lpszClassName, "Meow", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, wc.hInstance, IntPtr.Zero);

            MSG msg = new();
            while(GetMessage(ref msg, IntPtr.Zero, 0, 0)){
                DispatchMessage(ref msg);
            }
        }

        int WndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam){
            if(uMsg == taskbarCreatedMsg){
                gotTaskbarCreated = true;

                ProcessWindow(FindWindow(trayWndClassName, IntPtr.Zero));
                ProcessWindow(FindWindow(progManClassName, IntPtr.Zero));
                DestroyWindow(hWnd);

                return 1;
            }

            return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }
}
