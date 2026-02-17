using System;
using System.IO;
using System.Xml;
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
        }

        public IConfig LoadConfig(){
            rootNode = new ConfigNode(manager, "root");
            rootNode.SetVariable("screen.width", Screen.PrimaryScreen.Bounds.Width.ToString());
            rootNode.SetVariable("screen.height", Screen.PrimaryScreen.Bounds.Height.ToString());
            LoadFile("config.xml", rootNode);

            layoutNode = rootNode["config"]["layout"];
            PrintNode(rootNode);
            
            foreach(var hotkey in rootNode["config"]["hotkeys"].GetNodes("hotkey")){
                hotKeyManager.RegisterHotKey(hotkey.GetAttribute("keys").Split('+'), () => {
                    hotkey.GetAttribute("action");
                    return ParseHelper.ParseBool(hotkey.GetAttribute("continue", "false"));
                });
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

        public IConfig ExecuteScripts(){
            rootNode.EnumNodes("script", (scriptNode) => {
                if(string.IsNullOrEmpty(scriptNode.GetAttribute("defer"))){
                    Dictionary<string, object> nameSpace = new();
                    nameSpace.Add("CurrentNode", this);
                    nameSpace.Add("ComponentManager", manager);
                    nameSpace.Add("Config", manager.GetComponent<IConfig>());
                    nameSpace.Add("LayoutLoader", manager.GetComponent<ILayoutLoader>());
                    nameSpace.Add("Logger", manager.GetComponent<ILogger>());
                    nameSpace.Add("PluginManager", manager.GetComponent<IPluginManager>());
                    nameSpace.Add("ScriptEngine", manager.GetComponent<IScriptEngine>());
                    nameSpace.Add("ShellWindowManager", manager.GetComponent<IShellWindowManager>());
                    nameSpace.Add("ShellEventListener", manager.GetComponent<IShellEventListener>());
                    nameSpace.Add("WmxManager", manager.GetComponent<IWmxManager>());

                    var code = string.Empty;

                    if(string.IsNullOrEmpty(scriptNode.GetAttribute("src"))){
                        code = scriptNode.GetRawValue();
                    }else{
                        code = File.ReadAllText(Path.Combine(appDirectory, scriptNode.GetAttribute("src")));
                    }

                    engine.ExecuteScript(scriptNode.GetAttribute("language", "csharp"), code, nameSpace);
                }

                return true;
            });

            return this;
        }
    }
}
