using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.UI.BuiltInWidgets {
    public class ExternalProcess : BaseShellWidget {
        System.Windows.Forms.Panel panel = new();
        IShellEventListener shellEventListener = null;

        public override void Initialize(){
            Control = panel;
            shellEventListener = ComponentManager.GetComponent<IShellEventListener>();
            base.Initialize();
        }

        public override void PostInitialize(){
            ProcessStartInfo psi = new();
            psi.FileName = Node.GetAttribute("fileName");
            psi.Arguments = Node.GetAttribute("arguments");
            psi.WindowStyle = ProcessWindowStyle.Minimized;

            var process = Process.Start(psi);
            process.WaitForInputIdle();

            IntPtr hWnd = IntPtr.Zero;

            UtilHelper.IfNotEmpty(Node.GetAttribute("class"), className => {
                hWnd = FindWindow(className, null);
            });
            
            if(hWnd == IntPtr.Zero){
                UtilHelper.IfNotEmpty(Node.GetAttribute("classMatch"), classRegex => {
                    Regex regex = new(classRegex, RegexOptions.Compiled);

                    EnumWindows((hwnd, _) => {
                        if(regex.IsMatch(GetWindowClass(hwnd))){
                            hWnd = hwnd;
                            return false;
                        }

                        return true;
                    }, IntPtr.Zero);
                });
            }

            if(hWnd == IntPtr.Zero){
                UtilHelper.IfNotEmpty(Node.GetAttribute("titleMatch"), titleRegex => {
                    Regex regex = new(titleRegex, RegexOptions.Compiled);

                    EnumWindows((hwnd, _) => {
                        if(regex.IsMatch(GetWindowText(hwnd))){
                            hWnd = hwnd;
                            return false;
                        }

                        return true;
                    }, IntPtr.Zero);
                });
            }

            if(hWnd == IntPtr.Zero){
                try{
                    hWnd = process.MainWindowHandle;
                }catch(Exception){}
            }

            if(hWnd == IntPtr.Zero){
                EnumWindows((hwnd, _) => {
                    GetWindowThreadProcessId(hwnd, out int processId);
                    
                    if(processId == process.Id){
                        hWnd = hwnd;
                        return false;
                    }

                    return true;
                }, IntPtr.Zero);
            }

            // if(shellEventListener.HasWindow(hWnd)){
            //     shellEventListener.TriggerEvent(ShellWindowEvent.Destroy, hWnd);
            // }

            shellEventListener.AddIgnoredHandle(hWnd);

            SetParent(hWnd, panel.Handle);
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, panel.Width, panel.Height, SWP_NOZORDER);
            SetWindowLongPtr(hWnd, GWL_STYLE, WS_VISIBLE);
            SetWindowLongPtr(hWnd, GWL_EXSTYLE, WS_EX_TOOLWINDOW);

            base.PostInitialize();
        }
    }
}
