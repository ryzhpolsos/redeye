using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace RedEye {
    public class WebForm : Form {
        public WebWrapper WebWrapper;
    }

    public class DskForm : WebForm {
        public static DskForm Instance;

        public DskForm(){
            Instance = this;
            this.Load += new EventHandler(OnLoad);
            this.Shown += new EventHandler(OnShow);
            this.FormClosing += new FormClosingEventHandler(OnFormClosing);

            BackColor = Color.Black;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
        }

        void OnLoad(object sender, EventArgs e){
            int width = 0;
            int height = 0;

            foreach(var screen in Screen.AllScreens){
                width += screen.Bounds.Width;
                height += screen.Bounds.Height;
            }

            Location = new Point(0, 0);
            Size = new Size(width, height);

            //Native.SetWindowPos(Handle, Native.HWND_BOTTOM, 0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
        }

        void OnShow(object sender, EventArgs e){
            WebWrapper = new WebWrapper();

            WebWrapper.OnLoad = ()=>{
                ShellEventListener.Start(ShellEventHandler);
                InputHook.Start();
            };
            
            WebWrapper.Init(this, new ScriptManager(this), Loader.Load(), Config.CurrentConfig.core.useEdgeRuntime);
        }

        public void Reload(){
            Controls.Remove(WebWrapper.GetControl());

            WebWrapper = new WebWrapper();
            WebWrapper.OnLoad = ()=>{ ShellEventListener.CreateAllKnownWindows(); };
            WebWrapper.Init(this, new ScriptManager(this), Loader.Load(), Config.CurrentConfig.core.useEdgeRuntime);
        }

        struct ShellEvt {
            public ShellEventListener.EventType eventType;
            public ShellEventListener.ShellWnd window;
        }

        List<ShellEvt> failedEvents = new List<ShellEvt>();

        void ShellEventHandler(ShellEventListener.EventType eventType, ShellEventListener.ShellWnd window){
            //Logger.Log(Logger.MessageType.Information, $"shell event recv with {eventType} and {Util.ToJson(window)}");

            try{
                foreach(var evt in failedEvents.ToArray()){
                    WebWrapper.ExecuteFunction("_handleShellEvent", new object[]{ (int)eventType, Util.ToJson(evt.window) });
                    failedEvents.Remove(evt);
                }

                WebWrapper.ExecuteFunction("_handleShellEvent", new object[]{ (int)eventType, Util.ToJson(window) });
            }catch(Exception e){
                Logger.Log(Logger.MessageType.Information, e.Message + ";" + eventType + ";" + e.StackTrace + ";" + e.Source);
                failedEvents.Add(new ShellEvt(){ eventType = eventType, window = window });
            }
        }

        void OnFormClosing(object sender, FormClosingEventArgs e){
            e.Cancel = true;
        }

        protected override void WndProc(ref Message msg){
            if(msg.Msg == Native.WM_WINDOWPOSCHANGING){
                var wndPos = Marshal.PtrToStructure<Native.WINDOWPOS>(msg.LParam);
                
                if(wndPos.hwndInsertAfter != Native.HWND_BOTTOM){
                    wndPos.flags |= Native.SWP_NOZORDER;
                    Marshal.StructureToPtr(wndPos, msg.LParam, true);
                }
            }else if(msg.Msg == Native.WM_INPUTLANGCHANGE){
                ShellEventListener.SendLayoutChange((int)msg.LParam >> 16);
            }else{
                base.WndProc(ref msg);
            }
        }

        protected override CreateParams CreateParams {
            get {
                var Params = base.CreateParams;
                Params.ExStyle |= Native.WS_EX_TOOLWINDOW;
                return Params;
            }
        }
    }
}
