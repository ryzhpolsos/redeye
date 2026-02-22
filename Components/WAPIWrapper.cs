using System;

using RedEye.Core;

namespace RedEye.Components {
    public class WAPIWrapperComponent: IWAPIWrapper {
        ComponentManager manager = null;
        IWindowManager windowManager = null;
        IShellEventListener shellEventListener = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            windowManager = manager.GetComponent<IWindowManager>();
            shellEventListener = manager.GetComponent<IShellEventListener>();
        }

        public void MinimizeWindow(IntPtr hWnd){
            NativeHelper.MinimizeWindow(windowManager.GetWrapper(hWnd));
        }

        public void MaximizeWindow(IntPtr hWnd){
            NativeHelper.MaximizeWindow(windowManager.GetWrapper(hWnd));
        }

        public void RestoreWindow(IntPtr hWnd){
            NativeHelper.RestoreWindow(windowManager.GetWrapper(hWnd));
        }

        public void CloseWindow(IntPtr hWnd){
            NativeHelper.CloseWindow(windowManager.GetWrapper(hWnd));
        }

        public void ToggleWindow(IntPtr hWnd){
            shellEventListener.ToggleWindow(windowManager.GetWrapper(hWnd));
        }

        public void ActivateWindow(IntPtr hWnd){
            NativeHelper.ActivateWindow(windowManager.GetWrapper(hWnd));
        }
    }
}
