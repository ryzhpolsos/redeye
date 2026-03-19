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
        IShellWindowManager shellWindowManager = null;
        IShellEventListener shellEventListener = null;

        readonly string trayWndClassName = "Shell_TrayWnd";
        readonly string progManClassName = "Progman";

        bool enabled = false;
        bool enabledStateInit = false;
        int pid = 0;
        ConfigNode node = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            config = manager.GetComponent<IConfig>();
            shellWindowManager = manager.GetComponent<IShellWindowManager>();
            shellEventListener = manager.GetComponent<IShellEventListener>();
        }

        public bool GetIsEnabled(){
            if(!enabledStateInit){
                enabledStateInit = true;

#if DEBUG
                enabled = false;
#else
                var rootNode = manager.GetComponent<IConfig>().GetRootNode();

                if(rootNode is null){
                    manager.GetComponent<ILogger>().LogFatal("root node is null somehow");
                }

                node = rootNode["config"]["core"]["explorerIntegration"];
                enabled = ParseHelper.ParseBool(node["enable"].Value);
#endif
            }

            return enabled;
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
            if(!GetIsEnabled()) return;

            ShellWindowConfig cfg = new(){
                Type = ShellWindowType.TopMost,
                X = 0,
                Y = 0,
                Width = ParseHelper.ParseInt(config.GetRootNode().GetVariable("screenWidth")),
                Height = ParseHelper.ParseInt(config.GetRootNode().GetVariable("screenHeight")),
                BackgroundColor = "#000000"
            };

            // var wnd = shellWindowManager.CreateWindow(cfg);
            // wnd.ShowWindow();

            var proc = Process.Start("explorer.exe");
            proc.WaitForInputIdle();
            pid = proc.Id;

            Task.Run(() => {
                Thread.Sleep(ParseHelper.ParseInt(node["timeout"].Value));

                ProcessWindow(FindWindow(trayWndClassName, IntPtr.Zero));
                ProcessWindow(FindWindow(progManClassName, IntPtr.Zero));

                // wnd.CloseWindow();
                
                while(true){
                    shellEventListener.ReSetWorkArea();
                    Thread.Sleep(1000);
                }
                // shellEventListener.SetMinimizedMetrics();
            });
        }
    }
}
