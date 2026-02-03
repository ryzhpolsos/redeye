using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class WindowManagerComponent : IWindowManager {
        Dictionary<IntPtr, Form> windowWrappers = new();

        ComponentManager manager = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){}

        public IntPtr CreateWindowWrapper(IntPtr hWnd){
            Console.WriteLine($"title = '{GetWindowText(hWnd)}', class = {GetWindowClass(hWnd)}");

            RECT rc = new();
            GetWindowRect(hWnd, ref rc);

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOZORDER);

            var form = new Form();

            form.Load += (_, _) => {
                Console.WriteLine($"{(int)(rc.right - rc.left)}, {(int)(rc.bottom - rc.top)}");
                form.ClientSize = new((int)(rc.right - rc.left), (int)(rc.bottom - rc.top));
            };

            form.SizeChanged += (_, _) => {
                SetWindowPos(hWnd, IntPtr.Zero, 0, 0, form.ClientSize.Width, form.ClientSize.Height, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER);
            };

            form.FormClosed += (_, _) => {
                SendMessage(hWnd, WM_CLOSE, 0, 0);
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
                    wrapper.Close();
                    break;
                }

                case ShellWindowEvent.Minimize: {
                    MinimizeWindow(wrapper.Handle);
                    break;
                }

                case ShellWindowEvent.Redraw: {
                    wrapper.Text = state.Title;
                    wrapper.Icon = state.Icon;
                    break;
                }
            }
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
