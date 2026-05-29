using System.Drawing;
using System.Collections.Generic;

namespace RedEye.Core {
    public interface ISpecialFolderWrapper : IComponent {
        public IEnumerable<IApplicationListEntry> GetApplicationList();
    }

    public interface IApplicationListEntry {
        public IEnumerable<IApplicationListEntry> GetChildEntries();
        public bool GetIsFolder();
        public string GetName();
        public Icon GetIcon();
        public void Invoke();
        public string GetCommand();
    }
}
