using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using RedEye.Core;
using System.Collections.Generic;

namespace RedEye.Components {
    public class ConfigComponent :IConfig {
        ComponentManager manager = null;
        ILogger logger = null;
        IShellWindowManager windowManager = null;
        IHotKeyManager hotKeyManager = null;
        ILayoutLoader layoutLoader = null;
        IScriptEngine engine = null;

        ConfigNode rootNode = null;
        ConfigNode layoutNode = null;
        string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            logger = manager.GetComponent<ILogger>();
            windowManager = manager.GetComponent<IShellWindowManager>();
            hotKeyManager = manager.GetComponent<IHotKeyManager>();
            layoutLoader = manager.GetComponent<ILayoutLoader>();
            engine = manager.GetComponent<IScriptEngine>();
            
            ConfigNode.ComponentManager = manager;

        }

        public IConfig LoadConfig(){
            rootNode = new ConfigNode(manager, "root");
            rootNode.SetVariable("screen.width", Screen.AllScreens.Select(s => s.Bounds.Width).Sum().ToString());
            rootNode.SetVariable("screen.height", Screen.AllScreens.Select(s => s.Bounds.Height).Sum().ToString());
            LoadFile("config.xml", rootNode);

            layoutNode = rootNode["config"]["layout"];
            PrintNode(rootNode);
            
            foreach(var hotkey in rootNode["config"]["hotkeys"].GetNodes("hotkey")){
                hotKeyManager.RegisterHotKey(hotkey.GetAttribute("keys").Split('+'), () => {
                    hotkey.GetAttribute("action");
                    return ParseHelper.ParseBool(hotkey.GetAttribute("continue", "false"));
                }, ParseHelper.ParseBool(hotkey.GetAttribute("multiActivate", "false")));
            }

            foreach(var keyHandler in rootNode["config"]["hotkeys"].GetNodes("keyHandler")){
                hotKeyManager.RegisterKeyHandler((keyName, isUp) => {
                    keyHandler.SetVariable("keyName", keyName);
                    keyHandler.SetVariable("isUp", isUp.ToString());
                    keyHandler.GetAttribute("action");
                    return ParseHelper.ParseBool(keyHandler.GetAttribute("continue", "false"));
                });
            }
            
            foreach(var wnd in layoutNode.GetNodes("window")){
                layoutLoader.CreateWindowFromNode(wnd);
            }

            return this;
        }

        void PrintNode(ConfigNode node, int tabAmount = 0){
            Console.WriteLine(new string(' ', tabAmount * 4) + $"{node.Name} [{string.Join(", ", node.GetAttributes())}]");
            foreach(var nd in node.GetNodes()){
                PrintNode(nd, tabAmount + 1);
            }
        }

        public void LoadFile(string fileName, ConfigNode parentNode){
            XmlDocument doc = new();
            doc.Load(Path.Combine(appDirectory, fileName));
            LoadNode(doc.DocumentElement, parentNode);
        }

        public void LoadString(string data, ConfigNode parentNode){
            XmlDocument doc = new();
            doc.LoadXml(data);
            LoadNode(doc.DocumentElement, parentNode);
        }

        void LoadNode(XmlNode docNode, ConfigNode parentNode){
            if(docNode.Name.StartsWith("#")) return;
            Dictionary<string, string> attributes = new();

            if(docNode.Attributes is not null){
                for(int i = 0; i < docNode.Attributes.Count; i++){
                    var attr = docNode.Attributes[i];
                    attributes.Add(attr.Name, attr.Value);
                }
            }

            var node = new ConfigNode(manager, docNode.Name, attributes, docNode.InnerText);
            parentNode.AddNode(node);

            foreach(XmlNode childNode in docNode.ChildNodes){
                LoadNode(childNode, node);
            }
        }

        public string GetAppDirectory(){
            return appDirectory;
        }

        public string GetPath(params string[] relativePath){
            var combinedPath = Path.Combine(relativePath);

            if(Path.IsPathRooted(combinedPath)) return combinedPath;
            return Path.Combine(appDirectory, combinedPath);
        }

        public ConfigNode GetRootNode(){
            return rootNode;
        }

        public ConfigNode GetLayoutNode(){
            return layoutNode;
        }
    }
}
