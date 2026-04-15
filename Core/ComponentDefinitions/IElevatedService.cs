using System;

namespace RedEye.Core {
    public interface IElevatedService : IComponent {
        public void Start();
        public void Listen();
        public bool GetIsRequired();
        public void SetIsRequired(bool isRequired);
        public bool GetIsRunning();
        public void ExecuteCommand(ElevatedServiceCommand cmd, IntPtr hWnd, IntPtr handleParam = default, long longParam1 = 0, long longParam2 = 0);
    }

    public enum ElevatedServiceCommand {
        Close,
        Activate,
        Minimize,
        Restore,
        Move,
        Resize,
        Wrap
    }
}
