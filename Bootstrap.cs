using System;

using RedEye.Core;
using RedEye.Components;
using RedEye.Core.ScriptEngine;
using RedEye.UI.BuiltInWidgets;

namespace RedEye {
    public class Bootstrap {
        public void StartApplication(){
            ComponentManager manager = new();

            manager
            .RegisterComponentType<IShellWindow>(typeof(ShellWindowComponent))
            .AddComponent<ILogger>(new LoggerComponent())
            .AddComponent<IScriptEngine>(new ScriptEngineComponent())
            .AddComponent<IShellWindowManager>(new ShellWindowManagerComponent())
            .AddComponent<ILayoutLoader>(new LayoutLoaderComponent())
            .AddComponent<IConfig>(new ConfigComponent())
            .AddComponent<IShellEventListener>(new ShellEventListenerComponent())
            .AddComponent<IPluginManager>(new PluginManagerComponent())
            .AddComponent<IResourceManager>(new ResourceManagerComponent())
            .AddComponent<IExpressionFunctions>(new ExpressionFunctionsComponent())
            .AddComponent<IHotKeyManager>(new HotKeyManagerComponent())
            .AddComponent<IMediaManager>(new MediaManagerComponent())
            .AddComponent<IExpressionParser>(new ExpressionParserComponent())
            .AddComponent<IWindowManager>(new WindowManagerComponent())
            .AddComponent<ISpecialFolderWrapper>(new SpecialFolderWrapperComponent())
            .AddComponent<IExplorerIntegration>(new ExplorerIntegrationComponent())
            .InitializeComponents();

            manager
            .GetComponent<IScriptEngine>()
            .RegisterScriptHandler("csharp", new CSharpHandler())
            .RegisterScriptHandler("javascript", new JScriptHandler());

            manager
            .GetComponent<IPluginManager>()
            .ExportWidget("panel", typeof(Panel))
            .ExportWidget("flowPanel", typeof(FlowPanel))
            .ExportWidget("label", typeof(Label))
            .ExportWidget("image", typeof(Image))
            .ExportWidget("windowList", typeof(WindowList))
            .ExportWidget("appList", typeof(AppList))
            .ExportWidget("contextMenu", typeof(ContextMenu))
            .ExportWidget("tablePanel", typeof(TablePanel))
            .ExportWidget("textBox", typeof(TextBox))
            .ExportWidget("button", typeof(Button))
            .ExportWidget("externalProcess", typeof(ExternalProcess))
            .ExportWidget("webView", typeof(WebView));

            // var node = new ConfigNode(manager, "meow");
            // node.SetVariable("nya", "uwu");
            // Console.WriteLine(manager.GetComponent<IExpressionParser>().EvaluateExpression("showMessage(concat(${nya}, ' and meow', 'and something else'))", node));
            // Console.ReadLine();
            // return;
            // manager.GetComponent<IMediaManager>().DecreaseVolume();
            // // Console.WriteLine(manager.GetComponent<IMediaManager>().GetBrightness());
            // Console.WriteLine(manager.GetComponent<IMediaManager>().GetVolume());
            // return;
            // //
            // var list = manager.GetComponent<ISpecialFolderWrapper>().GetApplicationList();
            // list.First(e => e.GetName() == "Discord").GetIcon().ToBitmap().Save("meowmeow.bmp");
            // return;
            
            // var appList = manager.GetComponent<ISpecialFolderWrapper>().GetApplicationList();

            // void nya(IApplicationListEntry entry, int tab = 0){
            //     if(entry.GetIsFolder()){
            //         Console.WriteLine(new string(' ', tab * 4) + entry.GetName() + " -->");
            //         foreach(var sub in entry.GetChildEntries()){
            //             nya(sub, tab + 1);
            //         }
            //     }else{
            //         Console.WriteLine(new string(' ', tab * 4) + entry.GetName());
            //     }
            // }

            // foreach(var a in appList) nya(a);
            // return;

            try{
                manager.GetComponent<IPluginManager>().LoadPlugins();
                manager.GetComponent<IConfig>().LoadConfig();
                manager.GetComponent<IExplorerIntegration>().RunHiddenExplorer();
                manager.GetComponent<IShellWindowManager>().ShowWindows();
            }catch(Exception ex){
                manager.GetComponent<ILogger>().LogFatal(ExceptionHelper.FormatException(ex, true));
            }
        }
    }
}
