using System;

namespace RedEye.Core {
    public interface IWAPIWrapper: IComponent {
        public void MinimizeWindow(IntPtr hWnd);
        public void MaximizeWindow(IntPtr hWnd);
        public void RestoreWindow(IntPtr hWnd);
        public void CloseWindow(IntPtr hWnd);
        public void ToggleWindow(IntPtr hWnd);
        public void ActivateWindow(IntPtr hWnd);
    }
}
