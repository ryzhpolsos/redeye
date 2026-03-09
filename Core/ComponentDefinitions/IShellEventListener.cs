using System;
using System.Drawing;
using System.Collections.Generic;

namespace RedEye.Core {
    public interface IShellEventListener : IComponent {
        public void AddIgnoredHandle(IntPtr handle);
        public void RegisterEventHandler(Action<ShellWindowEvent, ShellWindowState> handler);
        public void ToggleWindow(IntPtr handle);
        public void SetWorkArea(int x, int y, int width, int height);
        public void ReSetWorkArea();
        public void SetMinimizedMetrics();
        public void SetDefaultIcon(string fileName, int id);
        public IEnumerable<ShellWindowState> GetWindows();
        public void TriggerEvent(ShellWindowEvent et, IntPtr hWnd);
        public bool HasWindow(IntPtr hWnd);
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
