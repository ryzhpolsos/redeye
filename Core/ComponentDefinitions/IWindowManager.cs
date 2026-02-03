using System;

namespace RedEye.Core {
    public interface IWindowManager : IComponent {
        public IntPtr CreateWindowWrapper(IntPtr hWnd);
        public void ProcessWrapperEvent(ShellWindowEvent evt, ShellWindowState state);
    }
}
