using System;
using System.Drawing;
using System.Collections.Generic;

using RedEye.Core;

namespace RedEye.Components {
    public class SpecialFolderWrapperComponent : ISpecialFolderWrapper {
        ComponentManager manager;

        dynamic shellApp;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            shellApp = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
        }

        public IEnumerable<IApplicationListEntry> GetApplicationList(){
            dynamic items = shellApp.NameSpace("shell:AppsFolder").Items();
            List<IApplicationListEntry> list = new();

            for(int i = 0; i < items.Count; i++){
                list.Add(new ApplicationListEntryImpl(items.Item(i)));
            }

            return list;
        }
    }

    internal class ApplicationListEntryImpl : IApplicationListEntry {
        dynamic item;

        public ApplicationListEntryImpl(dynamic item){
            this.item = item;
        }

        public string GetName(){
            return item.Name;
        }

        public Icon GetIcon(){
            NativeHelper.SHFILEINFO shfi = NativeHelper.GetFileInfo(item.Path, NativeHelper.SHGFI_ICON);
            return Icon.FromHandle(shfi.hIcon);
        }

        public void Invoke(){
            item.InvokeVerb("open");
        }
    }

    internal interface IShellDispatch {
        public IFolder NameSpace(string path);
    }

    internal interface IFolder {
        public IFolderItems Items();
    }

    internal interface IFolderItems {
        public int Count { get; }
        public IFolderItem Item(int index);
    }

    internal interface IFolderItem {
        public string Name { get; }
        public string Path { get; }
        public void InvokeVerb(string verb);
    }
}
