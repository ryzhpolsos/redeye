using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RedEye.Core {
    [ComVisible(true)]
    public interface ICOMAPI : IComponent {
        public void RegisterInROT();
        public IDictionary<string, string> GetMessage(string rcid);
        public void SendMessage(string rcid, IDictionary<string, string> args);
    }
}
