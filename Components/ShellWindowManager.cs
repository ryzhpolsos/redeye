using System;
using System.Windows.Forms;
using System.Collections.Generic;

using RedEye.Core;

namespace RedEye.Components {
    public class ShellWindowManagerComponent : IShellWindowManager {
        ComponentManager manager = null;
        ILogger logger = null;

        public Dictionary<string, IShellWindow> windows = new();

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            logger = manager.GetComponent<ILogger>();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

            Application.ThreadException += (sender, eventArgs) => {
                logger.LogFatal($"shlevt {eventArgs.Exception.GetType().FullName}: {eventArgs.Exception.Message}\r\n\r\n{eventArgs.Exception.StackTrace}");
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
                var exc = (Exception)eventArgs.ExceptionObject;
                logger.LogFatal($"shlevt {exc.GetType().FullName}: {exc.Message}\r\n\r\n{exc.StackTrace}");
            };
        }

        public IShellWindow CreateWindow(ShellWindowConfig config){
            var window = manager.CreateInstance<IShellWindow>();
            window.SetConfig(config);
            window.InitWindow();

            if(string.IsNullOrEmpty(config.Id)) config.Id = Guid.NewGuid().ToString();
            windows.Add(config.Id, window);

            return window;
        }

        public IEnumerable<IShellWindow> GetWindows(){
            return windows.Values;
        }

        public IShellWindow GetWindow(string id){
            return windows[id];
        }

        public void ShowWindows(){
            foreach(var wnd in windows.Values){
                if(wnd.GetConfig().AutoShow) wnd.ShowWindow();
            }

            Application.Run();
        }
    }
}
