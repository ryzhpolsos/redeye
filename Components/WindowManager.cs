using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class WindowManagerComponent : IWindowManager {
        Dictionary<IntPtr, Form> windowWrappers = new();

        ComponentManager manager = null;
        IShellEventListener shellEventListener = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            shellEventListener = manager.GetComponent<IShellEventListener>();
        }

        public IntPtr CreateWindowWrapper(IntPtr hWnd){
            Console.WriteLine($"title = '{GetWindowText(hWnd)}', class = {GetWindowClass(hWnd)}");

            RECT rc = new();
            GetWindowRect(hWnd, ref rc);

            SetWindowPos(hWnd, IntPtr.Zero, 0, 50, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOZORDER);

            var form = new Form();

            form.FormBorderStyle = FormBorderStyle.None;

            Button btnClose = new();
            btnClose.Text = "Close";
            btnClose.AutoSize = true;
            btnClose.Location = new Point(0, 0);
            btnClose.Click += (_, _) => {
                // shellEventListener.TriggerEvent(ShellWindowEvent.Destroy, hWnd);
                CloseWindow(form.Handle);
            };

            bool isMinimized = false;

            Button btnMinimize = new();
            btnMinimize.Text = "Minimize";
            btnMinimize.AutoSize = true;
            btnMinimize.Location = new Point(200, 0);
            btnMinimize.Click += (_, _) => {
                if(isMinimized){
                    // shellEventListener.TriggerEvent(ShellWindowEvent.Restore, hWnd); 
                    RestoreWindow(form.Handle);
                    btnMinimize.Text = "Restore";
                }else{
                    // shellEventListener.TriggerEvent(ShellWindowEvent.Minimize, hWnd);
                    MinimizeWindow(form.Handle);
                    btnMinimize.Text = "Restore";
                }
            };

            Button btnMaximize = new();
            btnMaximize.Text = "Maximize";
            btnMaximize.AutoSize = true;
            btnMaximize.Location = new Point(300, 0);
            btnMaximize.Click += (_, _) => {
                shellEventListener.TriggerEvent(ShellWindowEvent.Redraw, hWnd);
                MaximizeWindow(form.Handle);
            };

            form.Controls.AddRange(new Control[]{ btnMinimize, btnMaximize, btnClose });

            form.Load += (_, _) => {
                Console.WriteLine($"{(int)(rc.right - rc.left)}, {(int)(rc.bottom - rc.top)}");
                form.ClientSize = new((int)(rc.right - rc.left), 50 + (int)(rc.bottom - rc.top));

                // shellEventListener.TriggerEvent(ShellWindowEvent.Activate, form.Handle);

            };

            form.SizeChanged += (_, _) => {
                SetWindowPos(hWnd, IntPtr.Zero, 0, 50, form.ClientSize.Width, form.ClientSize.Height, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER);
            };

            form.FormClosed += (_, _) => {
                SendMessage(hWnd, WM_CLOSE, 0, 0);
            };

            form.Shown += (_, _) => {
                // shellEventListener.TriggerEvent(ShellWindowEvent.Activate, hWnd);
                form.Activated += (_, _) => {
                    shellEventListener.TriggerEvent(ShellWindowEvent.Activate, form.Handle);
                };

                form.Deactivate += (_, _) => {
                    shellEventListener.TriggerEvent(ShellWindowEvent.Deactivate, form.Handle);
                };
            };

            windowWrappers.Add(hWnd, form);

            form.Show();
            return form.Handle;
        }

        public void ProcessWrapperEvent(ShellWindowEvent evt, ShellWindowState state){
            var wrapper = windowWrappers[state.Handle];
            
            switch(evt){
                case ShellWindowEvent.Create: {
                    wrapper.Text = state.Title;
                    wrapper.Icon = state.Icon;
                    break;
                }

                case ShellWindowEvent.Activate: {
                    ActivateWindow(wrapper.Handle);
                    break;
                }

                case ShellWindowEvent.Destroy: {
                    // wrapper.Close();
                    break;
                }

                case ShellWindowEvent.Minimize: {
                    MinimizeWindow(wrapper.Handle);
                    break;
                }

                case ShellWindowEvent.Restore: {
                    RestoreWindow(wrapper.Handle);
                    break;
                }

                case ShellWindowEvent.Redraw: {
                    wrapper.Text = state.Title;
                    wrapper.Icon = state.Icon;
                    break;
                }
            }
        }

        public IntPtr GetWrapper(IntPtr hWnd){
            if(false && windowWrappers.ContainsKey(hWnd)){
                Console.WriteLine($"Found a wrapper for {hWnd}");
                return windowWrappers[hWnd].Handle;
            }

            return hWnd;
        }

        string GetWindowText(IntPtr h){
            int len = SendMessage(h, 0xE, 0L, 0L)+1;
            StringBuilder buff = new StringBuilder(len);
            SendMessage(h, 0xD, len, buff);
            return buff.ToString();
        }

        string GetWindowClass(IntPtr h){
            var buff = new StringBuilder(255);
            GetClassName(h, buff, buff.Capacity);
            return buff.ToString();
        }
    }
}
