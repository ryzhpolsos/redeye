using System;
using System.IO;
using System.Web;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Text;

namespace RedEye {
    public class WebWrapper {
        WebView2 webView;
        WebBrowser webBrowser;
        WebForm form;
        string htmlCode;
        object scriptManager;

        bool isEdge;

        public Action OnLoad;

        public void Init(WebForm _form, object _scriptManager, string _htmlCode, bool _isEdge){
            form = _form;
            scriptManager = _scriptManager;
            htmlCode = _htmlCode;
            isEdge = _isEdge;

            if(isEdge){
                webView = new WebView2();

                webView.Location = new Point(0, 0);
                webView.Size = new Size(form.Size.Width, form.Size.Height);
                form.Controls.Add(webView);

                InitEWVAsync();
            }else{
                webBrowser = new WebBrowser();

                webBrowser.ScrollBarsEnabled = false;
                webBrowser.AllowWebBrowserDrop = false;
                webBrowser.WebBrowserShortcutsEnabled = false;

                webBrowser.ObjectForScripting = scriptManager;
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DocumentLoadHandler);
                webBrowser.DocumentText = htmlCode;

                webBrowser.Location = new Point(0, 0);
                webBrowser.Size = new Size(form.Size.Width, form.Size.Height);
                form.Controls.Add(webBrowser);
            }
        }

        public void ExecuteFunction(string functionName, object[] args){
            //Logger.Log(Logger.MessageType.Information, $"{functionName} called with {string.Join(", ", args)}");
            if(isEdge){
                webView.Invoke(()=>{
                    webView.CoreWebView2.ExecuteScriptAsync($"{functionName}.apply(window, {Util.ToJson(args)});");
                });
            }else{
                webBrowser.Invoke(()=>{
                    webBrowser.Document.InvokeScript(functionName, args);
                });
            }
        }

        public void OpenDevTools(){
            if(isEdge){
                webView.CoreWebView2.OpenDevToolsWindow();
            }
        }

        public Control GetControl(){
            if(isEdge) return (Control)webView;
            return (Control)webBrowser;
        }

        async void InitEWVAsync(){
            var env = await CoreWebView2Environment.CreateAsync(Util.GetPath("edgeRuntime"), null, new CoreWebView2EnvironmentOptions(Config.CurrentConfig.core.edgeArguments));
            await webView.EnsureCoreWebView2Async(env);

            webView.AllowExternalDrop = false;
            webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            webView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

            webView.CoreWebView2.WebResourceRequested += (object sender, CoreWebView2WebResourceRequestedEventArgs args)=>{
                if(!args.Request.Uri.StartsWith("file:///")) return;

                string path = Uri.UnescapeDataString(args.Request.Uri.Substring("file:///".Length).Replace("/", "\\"));

                var ms = new MemoryStream();

                try{
                    var buffer = File.ReadAllBytes(Util.GetPath(path));
                    ms.Write(buffer, 0, buffer.Length);

                    args.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(ms, 200, "OK", "Content-Type: " + MimeMapping.GetMimeMapping(path));
                }catch(Exception ex){
                    var buffer = Encoding.Default.GetBytes($"Failed to open \"{path}\": {ex.Message}");
                    ms.Write(buffer, 0, buffer.Length);
                    args.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(ms, 404, "Not Found", "Content-Type: text/plain");
                }
            };

            webView.CoreWebView2.AddHostObjectToScript("scriptManager", scriptManager);
            webView.CoreWebView2.NavigationCompleted += new EventHandler<CoreWebView2NavigationCompletedEventArgs>(DocumentLoadHandler);
            webView.CoreWebView2.NavigateToString(htmlCode);
        }

        void DocumentLoadHandler(object s, EventArgs a){
            if(OnLoad != null) OnLoad();
        }
    }
}