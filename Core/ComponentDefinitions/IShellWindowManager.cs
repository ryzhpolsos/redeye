using System;
using System.Collections.Generic;

namespace RedEye.Core {
    public interface IShellWindowManager : IComponent {
        public IShellWindow CreateWindow(ShellWindowConfig config);
        public IEnumerable<IShellWindow> GetWindows();
        public IShellWindow GetWindow(string id);
        public void ShowWindows();
    }
}