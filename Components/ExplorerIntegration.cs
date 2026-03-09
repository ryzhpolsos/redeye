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
            shellEventListener = manager.GetComponent<IShellEventListener>();
        }

        public bool GetIsEnabled(){
            if(!enabledStateInit){
                enabledStateInit = true;
                node = manager.GetComponent<IConfig>().GetRootNode()["config"]["core"]["explorerIntegration"];
                enabled = ParseHelper.ParseBool(node["enable"].Value);
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
        }

        public void RunHiddenExplorer(){
            if(!GetIsEnabled()) return;

            var proc = Process.Start("explorer.exe");
            proc.WaitForInputIdle();
            pid = proc.Id;

            Task.Run(() => {
                Thread.Sleep(ParseHelper.ParseInt(node["timeout"].Value));

                ProcessWindow(FindWindow(trayWndClassName, IntPtr.Zero));
                ProcessWindow(FindWindow(progManClassName, IntPtr.Zero));
                shellEventListener.ReSetWorkArea();
                // shellEventListener.SetMinimizedMetrics();
            });
        }
    }
}
