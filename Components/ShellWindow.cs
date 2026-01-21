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
        IShellEventListener listener = null;

        string title = null;
        ShellWindowConfig config = null;
        Dictionary<string, IShellWidget> widgets = new();

        Form form = null;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            listener = manager.GetComponent<IShellEventListener>();
        }

        public void InitWindow(){
            switch(config.Type){
                case ShellWindowType.Normal: {
                    form = new Form();
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

            if(config.BorderType != ShellWindowBorderType.Normal){
                form.FormBorderStyle = ParseHelper.ParseEnum<FormBorderStyle>(config.BorderType.ToString(), FormBorderStyle.Sizable);
            }

            if(!config.AllowClose){
                form.FormClosing += (sender, eventArgs) => {
                    eventArgs.Cancel = true;
                };
            }

            form.MinimizeBox = config.MinimizeButton;
            form.MaximizeBox = config.MaximizeButton;

            form.Text = title;

            form.Load += (s, e) => {
                form.Location = new Point(config.X, config.Y);
                form.Size = new Size(config.Width, config.Height);
                form.AutoSize = config.AutoSize;
            };

            if(!string.IsNullOrEmpty(config.Padding)) form.Padding = ParseHelper.ParsePadding(config.Padding);
            if(!string.IsNullOrEmpty(config.Color)) form.ForeColor = ColorTranslator.FromHtml(config.Color);
            if(!string.IsNullOrEmpty(config.BackgroundColor)) form.BackColor = ColorTranslator.FromHtml(config.BackgroundColor);
        }

        public void ShowWindow(){
            form.Show();
        }

        public void HideWindow(){
            form.Hide();
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
            return widgets[id];
        }

        public void AddWidget(IShellWidget widget, bool addToForm = true){
            widgets.Add(widget.GetConfig().Id, widget);
            if(addToForm && widget.GetControl() is not null) form.Controls.Add(widget.GetControl());
            widget.SetWindow(this);
        }

        public void RemoveWidget(IShellWidget widget){
            if(form.Controls.Contains(widget.GetControl())) form.Controls.Remove(widget.GetControl());
            widgets.Remove(widget.GetConfig().Id);
            widget.SetWindow(null);
        }
    }

    internal class NoTaskbarForm : Form {
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
