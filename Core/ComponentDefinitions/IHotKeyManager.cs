using System;
using System.Collections.Generic;

namespace RedEye.Core {
    public interface IHotKeyManager : IComponent {
        public void RegisterKeyHandler(Func<string, bool, bool> handler);
        public void RegisterHotKey(IEnumerable<string> keys, Func<bool> callback);
    }
}
