using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ElevatedServiceComponent : IElevatedService {
        [StructLayout(LayoutKind.Sequential)]
        struct ElevatedServiceMessage {
            public ElevatedServiceCommand Command;
            public IntPtr Target;
            public IntPtr HandleParam;
            public long LongParam1;
            public long LongParam2;
        }

        ComponentManager manager = null;

        bool isRequired = false;
        IntPtr hWnd = IntPtr.Zero;

        readonly string className = "RedEye_ElevatedServiceWnd";

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
        }

        public bool GetIsRequired(){
#if TRUE//DEBUG
            return false;
#else
            return isRequired;
#endif
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

            ChangeWindowMessageFilterEx(hWnd, WM_COPYDATA, MSGFLT_ALLOW, IntPtr.Zero);

            MSG msg = new();
            while(GetMessage(ref msg, IntPtr.Zero, 0, 0)){
                DispatchMessage(ref msg);
            }
        }

        public void ExecuteCommand(ElevatedServiceCommand cmd, IntPtr hWnd, IntPtr handleParam = default, long longParam1 = 0, long longParam2 = 0){
            if(this.hWnd == IntPtr.Zero){
                this.hWnd = FindWindow(className, IntPtr.Zero);
            }

            if(this.hWnd == IntPtr.Zero){
                ShowError("Failed to find ElevatedService window");
            }

            COPYDATASTRUCT cds = new();
            cds.dwData = 0;
            cds.lpData = Marshal.AllocHGlobal(Marshal.SizeOf<ElevatedServiceMessage>());
            cds.cbData = Marshal.SizeOf<ElevatedServiceMessage>();

            ElevatedServiceMessage msg = new();
            msg.Command = cmd;
            msg.Target = hWnd;
            msg.HandleParam = handleParam;
            msg.LongParam1 = longParam1;
            msg.LongParam2 = longParam2;
            Marshal.StructureToPtr(msg, cds.lpData, false);

            SendMessage(this.hWnd, WM_COPYDATA, this.hWnd, ref cds);

            Marshal.FreeHGlobal(cds.lpData);
        }

        void ShowError(string message){
            MessageBox.Show(message, "RedEye Elevated Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        int WndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam){
            if(uMsg == WM_COPYDATA){
                var cds = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);
                var msg = Marshal.PtrToStructure<ElevatedServiceMessage>(cds.lpData);

                var command = msg.Command;
                var target = msg.Target;

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

                    case ElevatedServiceCommand.Move: {
                        SetWindowPos(target, IntPtr.Zero, (int)msg.LongParam1, (int)msg.LongParam2, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                        break;
                    }

                    case ElevatedServiceCommand.Resize: {
                        SetWindowPos(target, IntPtr.Zero, 0, 0, (int)msg.LongParam1, (int)msg.LongParam2, SWP_NOMOVE | SWP_NOZORDER);
                        break;
                    }

                    case ElevatedServiceCommand.Wrap: {
                        WrapWindow(target, msg.HandleParam);
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
