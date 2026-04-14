using System;
using System.Drawing;

namespace RedEye.Core {
    public interface IWindowManager : IComponent {
        public INativeWindow GetWindow(IntPtr hWnd);
    }

    public interface INativeWindow {
        public IntPtr GetHwnd();
        public string GetText();
        public Icon GetIcon();
        public void Close();
        public void Activate();
        public void Minimize();
        public void Restore();
        public void Toggle();
    }
}
