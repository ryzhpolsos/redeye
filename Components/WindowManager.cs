using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class WindowManagerComponent : IWindowManager {
        ComponentManager manager = null;

        IConfig config = null;
        ILayoutLoader layoutLoader = null;
        IElevatedService elevatedService = null;
        IResourceManager resourceManager = null;
        IShellEventListener shellEventListener = null;

        bool enabled = false;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            config = manager.GetComponent<IConfig>();
            layoutLoader = manager.GetComponent<ILayoutLoader>();
            elevatedService = manager.GetComponent<IElevatedService>();
            resourceManager = manager.GetComponent<IResourceManager>();
            shellEventListener = manager.GetComponent<IShellEventListener>();
        }

        public INativeWindow GetWindow(IntPtr hWnd){
            return new NativeWindowImpl(manager, hWnd);
        }

        public bool GetIsEnabled(){
            return enabled;
        }

        public void SetIsEnabled(bool enabled){
            this.enabled = enabled;
        }

        public void Start(){
            Dictionary<IntPtr, IntPtr> wrappers = new();

            shellEventListener.RegisterEventHandler((evt, state) => {
                if(wrappers.ContainsKey(state.Handle)){
                    switch(evt){
                        case ShellWindowEvent.Destroy: {
                            GetWindow(wrappers[state.Handle]).Close();
                            break;
                        }

                        case ShellWindowEvent.Activate: {
                            RedrawWindow(wrappers[state.Handle], IntPtr.Zero, IntPtr.Zero, 0);
                            break;
                        }
                    }

                    return false;
                }

                if(evt == ShellWindowEvent.Create){
                    if(wrappers.ContainsValue(state.Handle)){
                        var wnd = GetWindow(wrappers.First(kvp => kvp.Value == state.Handle).Key);
                        state.Title = wnd.GetText();
                        state.Icon = wnd.GetIcon();
                        return true;
                    }

                    RECT wndRect = new();
                    GetWindowRect(state.Handle, ref wndRect);

                    RECT clientRect = new();
                    GetClientRect(state.Handle, ref clientRect);

                    var window = GetWindow(state.Handle);

                    var node = config.GetRootNode()["config"]["core"]["windowManager"]["window"];
                    node.SetVariable("window.x", wndRect.left.ToString());
                    node.SetVariable("window.y", wndRect.top.ToString());
                    node.SetVariable("window.width", (clientRect.right - clientRect.left).ToString());
                    node.SetVariable("window.height", (clientRect.bottom - clientRect.top).ToString());
                    node.SetVariable("window.title", window.GetText());
                    node.SetVariable("window.icon", resourceManager.AddResource(window.GetIcon()));

                    var wrapperWindow = layoutLoader.CreateWindowFromNode(node);
                    node.SetVariable("window.handle", wrapperWindow.GetHwnd().ToString());
                    wrappers.Add(state.Handle, wrapperWindow.GetHwnd());

                    wrapperWindow.SetTitle("W: " + window.GetText());
                    wrapperWindow.SetIcon(window.GetIcon());
                    wrapperWindow.ShowWindow();

                    window.Wrap(wrapperWindow.GetWidget("content").GetControl().Handle);
                    window.Move(ParseHelper.ParseInt(node.GetVariable("window.offset.x")), ParseHelper.ParseInt(node.GetVariable("window.offset.y")));
                    window.Resize(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top);

                    shellEventListener.TriggerEvent(ShellWindowEvent.Create, wrapperWindow.GetHwnd());
                    return false;
                }

                return true;
            });
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

        public void Toggle(){
            shellEventListener.ToggleWindow(hWnd);
        }

        public void Move(int x, int y){
            if(elevatedService.GetIsRequired()){
                elevatedService.ExecuteCommand(ElevatedServiceCommand.Move, hWnd, longParam1: x, longParam2: y);
            }else{
                SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER); 
            }
        }

        public void Resize(int width, int height){
            if(elevatedService.GetIsRequired()){
                elevatedService.ExecuteCommand(ElevatedServiceCommand.Resize, hWnd, longParam1: width, longParam2: height);
            }else{
                SetWindowPos(hWnd, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE | SWP_NOZORDER); 
            }
        }

        public void Wrap(IntPtr wrapper){
            if(elevatedService.GetIsRequired()){
                elevatedService.ExecuteCommand(ElevatedServiceCommand.Wrap, hWnd, handleParam: wrapper);
            }else{
                WrapWindow(hWnd, wrapper);
            }
        }
    }
}
