using System;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ElevatedServiceComponent : IElevatedService {
        ComponentManager manager = null;

        bool isRequired = false;
        IntPtr hWnd = IntPtr.Zero;

        readonly string className = "RedEye_ElevatedServiceWnd";
        readonly int message = RegisterWindowMessage("RedEye_ElevatedServiceMsg");

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
        }

        public bool GetIsRequired(){
            return isRequired;
        }

        public void SetIsRequired(bool isRequired){
            this.isRequired = isRequired;
        }

        public bool GetIsRunning(){
            return FindWindow(className, IntPtr.Zero) != IntPtr.Zero;
        }

        public void Start(){
            ProcessStartInfo psi = new();
            psi.FileName = "Schtasks.exe"; 
            psi.Arguments = "/Run /TN RedEyeElevatedService";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            Process.Start(psi).WaitForExit();
        }

        public void Listen(){
            WNDCLASSEX wc = new();
            wc.cbSize = Marshal.SizeOf<WNDCLASSEX>();
            wc.hInstance = GetModuleHandle(IntPtr.Zero);
            wc.lpszClassName = className;
            wc.lpfnWndProc = WndProc;

            if(RegisterClassEx(ref wc) == 0){
                ShowError("Class registration failed: " + Marshal.GetLastWin32Error().ToString());
            }

            IntPtr hWnd = CreateWindowEx(0, wc.lpszClassName, "Nya~", 0, 0, 0, 0, 0, HWND_MESSAGE, IntPtr.Zero, wc.hInstance, IntPtr.Zero);

            if(hWnd == IntPtr.Zero){
                ShowError("Window creation failed: " + Marshal.GetLastWin32Error().ToString());
            }

            MSG msg = new();
            while(GetMessage(ref msg, IntPtr.Zero, 0, 0)){
                DispatchMessage(ref msg);
            }
        }

        public void ExecuteCommand(ElevatedServiceCommand cmd, IntPtr hWnd){
            if(this.hWnd == IntPtr.Zero){
                this.hWnd = FindWindow(className, IntPtr.Zero);
            }

            SendMessage(this.hWnd, message, (int)cmd, hWnd);
        }

        void ShowError(string message){
            MessageBox.Show(message, "RedEye Elevated Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        int WndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam){
            if(uMsg == message){
                var command = (ElevatedServiceCommand)wParam;
                var target = lParam;

                switch(command){
                    case ElevatedServiceCommand.Close: {
                        CloseWindow(target);
                        break;
                    }

                    case ElevatedServiceCommand.Activate: {
                        ActivateWindow(target);
                        break;
                    }

                    case ElevatedServiceCommand.Minimize: {
                        MinimizeWindow(target);
                        break;
                    }

                    case ElevatedServiceCommand.Restore: {
                        RestoreWindow(target);
                        break;
                    }
                }

                return 1;
            }else{
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
            }
        }
    }
}
