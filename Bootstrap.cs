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
            .AddComponent<ICOMAPI>(new COMAPIComponent())
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

            var comApi = manager.GetComponent<ICOMAPI>();
            comApi.RegisterInROT();

            // Console.ReadLine();
            // Console.WriteLine(comApi.SendMessage(
            //     "MYTEST",
            //     new System.Collections.Generic.Dictionary<string, string>(){
            //         {"Meow", "UwU"}
            //     }
            // ));

            try{
                manager.GetComponent<ILogger>().LogInformation("Shell started");
                manager.GetComponent<IPluginManager>().LoadPlugins();
                manager.GetComponent<IConfig>().LoadConfig();
                manager.GetComponent<IShellWindowManager>().ShowWindows();
            }catch(Exception ex){
                manager.GetComponent<ILogger>().LogFatal(ExceptionHelper.FormatException(ex, true));
            }
        }
    }
}
