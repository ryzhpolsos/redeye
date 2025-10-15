using System;
using System.Drawing;
using System.Windows.Forms;

namespace RedEye {
    public class HtmlWindowScriptManager : ScriptManager {
        string handlerId;
        public HtmlWindowScriptManager(WebForm f, string hid) : base(f) {
            handlerId = hid;
        }

        public void HandleEvent(string selector, string evName, string eoStr){
            DskForm.Instance.WebWrapper.ExecuteFunction("_HtmlWindow_"+handlerId, new object[]{ selector, evName, eoStr });
        }
    }

    public class HtmlWindow : WebForm {
        Rectangle _bounds;
        string content;
        string handlerId;
        bool isShell;
        
        public HtmlWindow(int x, int y, int width, int height, string cnt, string hid, bool shell){
            this.Load += new EventHandler(OnLoad);
            this.Shown += new EventHandler(OnShow);
            this.SizeChanged += new EventHandler(OnSizeChange);

            _bounds = new Rectangle();
            _bounds.X = x;
            _bounds.Y = y;
            _bounds.Width = width;
            _bounds.Height = height;
            handlerId = hid;
            content = cnt;
            isShell = shell;

            if(shell){
                ShowInTaskbar = false;
            }
        }

        void OnLoad(object sender, EventArgs ea){
            Location = new Point(_bounds.X, _bounds.Y);
            Size = new Size(_bounds.Width, _bounds.Height);
            ShowIcon = false;
        }

        void OnShow(object sender, EventArgs ea){
            string docText = Util.ReplaceTemplate(
                Util.ReadFile("res\\htmlWindowTemplate.html"),
                "IE_VERSION", Config.CurrentConfig.core.ieVersion,
                "GLOBAL_CONFIG", Config.CurrentConfigText,
                "COMMON_SCRIPTS", Loader.IterateFiles("scripts", "script", Config.CurrentConfig.core.loadHTWScripts),
                "CONTENT", content
            );

            WebWrapper = new WebWrapper();

            WebWrapper.OnLoad = ()=>{
                DskForm.Instance.WebWrapper.ExecuteFunction("_HtmlWindow_loaded_"+handlerId, new object[0]);
            };

            WebWrapper.Init(this, new HtmlWindowScriptManager(this, handlerId), docText, Config.CurrentConfig.core.useEdgeRuntime);
        }

        void OnSizeChange(object sender, EventArgs ea){
            if(WebWrapper == null) return;

            var ctr = WebWrapper.GetControl();
            if(ctr == null) return;

            ctr.Size = new Size(ClientSize.Width, ClientSize.Height);
        }

        public void move(int x, int y){
            Location = new Point(x, y);
        }

        public void resize(int width, int height){
            Size = new Size(width, height);
            WebWrapper.GetControl().Size = new Size(width, height);
        }

        public void sendEvent(string evType, string evData){
            WebWrapper.ExecuteFunction("_handleEvent", new object[]{ evType, evData });
        }

        public void setBorder(int bt){
            FormBorderStyle = (FormBorderStyle)bt;
        }

        public void openDevTools(){
            WebWrapper.OpenDevTools();
        }

        protected override CreateParams CreateParams {
            get {
                var Params = base.CreateParams;
                if(isShell) Params.ExStyle |= Native.WS_EX_TOOLWINDOW;
                return Params;
            }
        }
    }
}