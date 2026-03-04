using System;
using System.Diagnostics;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.UI.BuiltInWidgets {
    public class ExternalWindow : BaseShellWidget {
        System.Windows.Forms.Panel panel = new();

        public override void Initialize(){
            Control = panel;
            base.Initialize();
        }

        public override void PostInitialize(){
            ProcessStartInfo psi = new();
            psi.FileName = Node.GetAttribute("fileName");
            psi.Arguments = Node.GetAttribute("arguments");
            psi.WindowStyle = ProcessWindowStyle.Minimized;

            var process = Process.Start(psi);
            process.WaitForInputIdle();

            SetParent(process.MainWindowHandle, panel.Handle);
            SetWindowPos(process.MainWindowHandle, IntPtr.Zero, 0, 0, panel.Width, panel.Height, SWP_NOZORDER);
            SetWindowLongPtr(process.MainWindowHandle, GWL_STYLE, WS_VISIBLE);
            SetWindowLongPtr(process.MainWindowHandle, GWL_EXSTYLE, WS_EX_TOOLWINDOW);

            base.PostInitialize();
        }
    }
}
