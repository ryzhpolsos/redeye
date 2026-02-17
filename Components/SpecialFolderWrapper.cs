using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

using RedEye.Core;

namespace RedEye.Components {
    public class SpecialFolderWrapperComponent : ISpecialFolderWrapper {
        readonly string localApplicationsFolder = Environment.ExpandEnvironmentVariables(@"%APPDATA%\Microsoft\Windows\Start Menu\Programs");
        readonly string globalApplicationsFolder = Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs");

        ComponentManager manager;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
        }

        public IEnumerable<IApplicationListEntry> GetApplicationList(){
            return GetApplicationListInternal(new[]{ localApplicationsFolder, globalApplicationsFolder });
        }

        IEnumerable<IApplicationListEntry> GetApplicationListInternal(string[] dirs){
            List<IApplicationListEntry> entries = new();

            foreach(var dir in dirs){
                foreach(var entry in Directory.GetFileSystemEntries(dir)){
                    bool isFolder = false;
                    IEnumerable<IApplicationListEntry> childEntries = null;

                    if(Directory.Exists(entry)){
                        isFolder = true;
                        childEntries = GetApplicationListInternal(new[]{entry});
                    }else{
                        if(!new[]{".lnk", ".url"}.Contains(Path.GetExtension(entry))) continue;
                    }

                    entries.Add(new ApplicationListEntryImpl(entry, isFolder, childEntries));
                }
            }

            return entries;
        }
    }

    internal class ApplicationListEntryImpl : IApplicationListEntry {
        string path = string.Empty;
        string name = null;
        bool isFolder = false;
        Icon icon = null;
        List<IApplicationListEntry> childEntries = new();

        public ApplicationListEntryImpl(string path, bool isFolder = false, IEnumerable<IApplicationListEntry> childEntries = null){
            this.path = path;
            this.isFolder = isFolder;
            if(childEntries is not null) this.childEntries.AddRange(childEntries);
        }

        public string GetName(){
            if(name is null){
                name = Path.GetFileNameWithoutExtension(path);
            }

            return name;
        }

        public Icon GetIcon(){
            if(icon is null){
                var ext = Path.GetExtension(path);
                if(ext == ".lnk" || ext == ".url"){
                    dynamic wshShell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                    dynamic shortcut = wshShell.CreateShortcut(path);

                    try{
                        dynamic shellApp = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
                        dynamic link = shellApp.NameSpace(Path.GetDirectoryName(path)).Items().Item(Path.GetFileName(path)).GetLink();
                        link.GetIconLocation(out string iconLocation);
                        // Console.WriteLine($"ICON LOCATION IS {iconLocation}");
                    }catch(Exception){}

                    var hIcon = NativeHelper.GetIconFromLocation(shortcut.IconLocation);

                    if(hIcon == IntPtr.Zero){
                        // Console.WriteLine($"GetIconFromLocation failed for {path} => {shortcut.IconLocation}");
                        icon = Icon.FromHandle(NativeHelper.GetFileInfo(path, NativeHelper.SHGFI_ICON).hIcon);
                    }else{
                        icon = Icon.FromHandle(hIcon);
                    }
                }else{
                    icon = Icon.FromHandle(NativeHelper.GetFileInfo(path, NativeHelper.SHGFI_ICON).hIcon);
                }
            }

            return icon;
        }

        public IEnumerable<IApplicationListEntry> GetChildEntries(){
            return childEntries;
        }

        public void Invoke(){
            Process.Start(path);
        }

        public bool GetIsFolder(){
            return isFolder;
        }

        public string GetCommand(){
            return path;
        }
    }
}
