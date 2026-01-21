using System;
using System.Drawing;

namespace RedEye.Core {
    public interface IShellEventListener : IComponent {
        public void AddIgnoredHandle(IntPtr handle);
        public void RegisterEventHandler(Action<ShellWindowEvent, ShellWindowState> handler);
        public void ToggleWindow(IntPtr handle);
    }

    public enum ShellWindowEvent {
        Create,
        Destroy,
        Minimize,
        Restore,
        Redraw,
        Activate,
        Deactivate,
        LayoutChange
    }

    public class ShellWindowState {
        public IntPtr Handle;
        public bool IsMinimized = false;
        public bool IsActive = false;
        public int ShowCmd = 1;
        public string Title = "";
        public Icon Icon = null;
        public string Data = "";
    }
}
