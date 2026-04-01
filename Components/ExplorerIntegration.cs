using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

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
#if DEBUG
            return false;
#else
            return enabled;
#endif
        }

        public void SetIsEnabled(bool enabled){
            logger.LogInformation($"Explorer integration is {(enabled ? "enabled" : "disabled")}");
            this.enabled = enabled;
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
            ShellWindowConfig cfg = new(){
                Type = ShellWindowType.TopMost,
                X = 0,
                Y = 0,
                Width = ParseHelper.ParseInt(config.GetRootNode().GetVariable("screenWidth")),
                Height = ParseHelper.ParseInt(config.GetRootNode().GetVariable("screenHeight")),
                BackgroundColor = "#000000"
            };

            var wnd = shellWindowManager.CreateWindow(cfg);
            wnd.ShowWindow();

            foreach(var explorer in Process.GetProcessesByName("explorer")){
                try{
                    explorer.Kill();
                    logger.LogInformation($"Killed Explorer process with PID {explorer.Id}");
                }catch(Exception ex){
                    logger.LogWarning($"Failed to kill Explorer process with PID {explorer.Id}: {ex.Message}");
                }
            }

            var proc = Process.Start("explorer.exe");
            proc.WaitForInputIdle();
            pid = proc.Id;

            Task.Run(() => {
                Thread.Sleep(timeout);

                ProcessWindow(FindWindow(trayWndClassName, IntPtr.Zero));
                ProcessWindow(FindWindow(progManClassName, IntPtr.Zero));

                // wnd.CloseWindow();
                
                while(true){
                    shellEventListener.ReSetWorkArea();
                    Thread.Sleep(250);
                }
                // shellEventListener.SetMinimizedMetrics();
            });
        }
    }
}
