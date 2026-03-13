using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using RedEye.UI;
using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    public class ShellWindowComponent : IShellWindow {
        ComponentManager manager = null;
        IConfig shellConfig = null;
        IShellEventListener listener = null;

        string title = null;
        ShellWindowConfig config = null;
        Dictionary<string, IShellWidget> widgets = new();

        Form form = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            shellConfig = manager.GetComponent<IConfig>();
            listener = manager.GetComponent<IShellEventListener>();
        }

        public void InitWindow(){
            switch(config.Type){
                case ShellWindowType.Normal: {
                    form = new ShellForm();
                    break;
                }

                case ShellWindowType.Top: {
                    form = new TopForm(listener);
                    break;
                }

                case ShellWindowType.TopMost: {
                    form = new TopMostForm(listener);
                    break;
                }

                case ShellWindowType.Background: {
                    form = new BackgroundForm(listener);
                    break;
                }
            }

            form.AutoScaleMode = AutoScaleMode.None;

            if(config.BorderType != ShellWindowBorderType.Normal){
                form.FormBorderStyle = ParseHelper.ParseEnum<FormBorderStyle>(config.BorderType.ToString(), FormBorderStyle.Sizable);
            }

            form.FormClosing += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                if(config.AllowClose) HideWindow();
            };

            form.MinimizeBox = config.MinimizeButton;
            form.MaximizeBox = config.MaximizeButton;

            form.Text = title;

            form.Load += (_, _) => {
                form.Location = new Point(config.X, config.Y);
                form.Size = new Size(config.Width, config.Height);
                form.AutoSize = config.AutoSize;
            };

            form.Shown += (_, _) => {
                SetWindowPos(form.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                SetWindowPos(form.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                BringWindowToTop(form.Handle);
                SetForegroundWindow(form.Handle);
                // ActivateWindow(form.Handle);
                // SetActiveWindow(form.Handle);
            };

            if(!string.IsNullOrEmpty(config.Padding)) form.Padding = ParseHelper.ParsePadding(config.Padding);
            if(!string.IsNullOrEmpty(config.Color)) form.ForeColor = ColorTranslator.FromHtml(config.Color);
            if(!string.IsNullOrEmpty(config.BackgroundColor)) form.BackColor = ColorTranslator.FromHtml(config.BackgroundColor);

            if(config.AllowTransparency){
                form.AllowTransparency = true;
                form.TransparencyKey = ColorTranslator.FromHtml(shellConfig.GetRootNode()["config"]["core"]["ui"]["transparencyKey"].Value);
            }

            form.Opacity = config.Opacity;
        }

        public void ShowWindow(){
            form.Show();
        }

        public void HideWindow(){
            form.Hide();
        }

        public void CloseWindow(){
            form.Close();
        }

        public void ToggleWindow(){
            if(form.Visible){
                form.Hide();
            }else{
                form.Show();
            }
        }

        public string GetTitle(){
            return title;
        }

        public void SetTitle(string newTitle){
            title = newTitle;
        }

        public ShellWindowConfig GetConfig(){
            return config;
        }

        public void SetConfig(ShellWindowConfig newConfig){
            config = newConfig;
            SetTitle(newConfig.Title);
        }

        public IDictionary<string, IShellWidget> GetWidgets(){
            return widgets;
        }

        public IShellWidget GetWidget(string id){
            if(widgets.ContainsKey(id)) return widgets[id];

            foreach(var widget in widgets.Values){
                if(widget is IContainerWidget container && container is not null){
                    if(container.GetWidget(id) is var wid && wid is not null){
                        return wid;
                    }
                }
            }

            return null;
        }

        public void AddWidget(IShellWidget widget, bool addToForm = true){
            if(addToForm && widget.GetControl() is not null) form.Controls.Add(widget.GetControl());
            widget.SetWindow(this);
            widgets.Add(widget.GetConfig().Id, widget);
        }

        public void RemoveWidget(IShellWidget widget){
            if(form.Controls.Contains(widget.GetControl())) form.Controls.Remove(widget.GetControl());
            widgets.Remove(widget.GetConfig().Id);
            widget.SetWindow(null);
        }

        public void RegisterEventHandler(string name, Action eventHandler){
             if(form.GetType().GetEvent(name) is var @event){
                DelegateWrapper<object, EventArgs> delegateWrapper = new((data, args) => {
                    eventHandler.Invoke();
                });

                @event.GetAddMethod().Invoke(form, new object[]{ delegateWrapper.GetDelegate(@event.EventHandlerType) });
            } 

        }
    }

    internal class ShellForm : Form {
        public ShellForm() : base() {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        protected override CreateParams CreateParams {
            get {
                var @params = base.CreateParams;
                @params.ExStyle |= 0x02000000;
                return @params;
            }
        }
    }

    internal class NoTaskbarForm : ShellForm {
        IShellEventListener listener = null;

        public NoTaskbarForm(IShellEventListener listener) : base() {
            ShowInTaskbar = false;
            this.listener = listener;
            Load += OnLoad;
            FormClosing += OnFormClosing;
        }

        void OnLoad(object sender, EventArgs ea){
            listener.AddIgnoredHandle(Handle);
        }

        void OnFormClosing(object sender, FormClosingEventArgs ea){
            ea.Cancel = true;
        }

        protected override CreateParams CreateParams {
            get {
                var @params = base.CreateParams;
                @params.ExStyle |= WS_EX_TOOLWINDOW;
                return @params;
            }
        }
    }

    internal class BackgroundForm : NoTaskbarForm {
        public BackgroundForm(IShellEventListener listener) : base(listener){}

        protected override void WndProc(ref Message msg){
            if(msg.Msg == WM_WINDOWPOSCHANGING){
                var wndPos = Marshal.PtrToStructure<WINDOWPOS>(msg.LParam);
                
                if(wndPos.hwndInsertAfter != HWND_BOTTOM){
                    wndPos.flags |= SWP_NOZORDER;
                    Marshal.StructureToPtr(wndPos, msg.LParam, true);
                }
            }

            base.WndProc(ref msg);
        }
    }

    internal class TopForm : NoTaskbarForm {
        public TopForm(IShellEventListener listener) : base(listener){}

        protected override void WndProc(ref Message msg){
            if(msg.Msg == WM_WINDOWPOSCHANGING){
                SetWindowPos(msg.HWnd, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }

            base.WndProc(ref msg);
        }
    }


    internal class TopMostForm : NoTaskbarForm {
        public TopMostForm(IShellEventListener listener) : base(listener){}

        protected override void WndProc(ref Message msg){
            if(msg.Msg == WM_WINDOWPOSCHANGING){
                SetWindowPos(msg.HWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }

            base.WndProc(ref msg);
        }
    }
}
