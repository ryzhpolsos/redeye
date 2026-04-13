using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class WindowManagerComponent : IWindowManager {
        ComponentManager manager = null;
        IElevatedService elevatedService = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            elevatedService = manager.GetComponent<IElevatedService>();
        }

        public INativeWindow GetWindow(IntPtr hWnd){
            return new NativeWindowImpl(manager, hWnd);
        }
    }

    class NativeWindowImpl : INativeWindow {
        IntPtr hWnd;
        IElevatedService elevatedService;
        IShellEventListener shellEventListener;

        public NativeWindowImpl(ComponentManager manager, IntPtr hWnd){
            this.elevatedService = manager.GetComponent<IElevatedService>();
            this.shellEventListener = manager.GetComponent<IShellEventListener>();
            this.hWnd = hWnd;
        }

        public IntPtr GetHwnd(){
            return hWnd;
        }

        public string GetText(){
            return GetWindowText(hWnd);
        }

        public Icon GetIcon(){
            return shellEventListener.GetWindowIcon(hWnd); 
        }

        public void Close(){
            if(elevatedService.GetIsRequired()){
                elevatedService.ExecuteCommand(ElevatedServiceCommand.Close, hWnd);
            }else{
                CloseWindow(hWnd);
            }
        }
        
        public void Activate(){
            if(elevatedService.GetIsRequired()){
                elevatedService.ExecuteCommand(ElevatedServiceCommand.Activate, hWnd);
            }else{
                ActivateWindow(hWnd);
            }
        }
        
        public void Minimize(){
            if(elevatedService.GetIsRequired()){
                elevatedService.ExecuteCommand(ElevatedServiceCommand.Minimize, hWnd);
            }else{
                MinimizeWindow(hWnd);
            }
        }
        
        public void Restore(){
            if(elevatedService.GetIsRequired()){
                elevatedService.ExecuteCommand(ElevatedServiceCommand.Restore, hWnd);
            }else{
                RestoreWindow(hWnd);
            }
        }
    }
}
