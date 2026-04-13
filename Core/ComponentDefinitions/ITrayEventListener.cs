using System;
using System.Drawing;

namespace RedEye.Core {
    public interface ITrayEventListener : IComponent {
        public void RegisterEventHandler(Action<TrayIconEvent, TrayIconState> handler);
        public void SendTaskbarCreated();
    }

    public enum TrayIconEvent {
        Create,
        Modify,
        Delete
    }

    public class TrayIconState {
        public IntPtr Handle = IntPtr.Zero;
        public uint UID = 1;
        public Guid Guid = default;
        public Icon Icon = null;
        public string ToolTip = "";
    }
}
