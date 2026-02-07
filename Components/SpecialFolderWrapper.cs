using System;
using System.Drawing;
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
            List<IApplicationListEntry> entries = new();

            foreach(var dir in new string[]{ localApplicationsFolder, globalApplicationsFolder }){
                
            }

            return entries;
        }
    }
}
