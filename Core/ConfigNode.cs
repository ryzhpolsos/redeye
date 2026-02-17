using System;
using System.Linq;
using System.Collections.Generic;

namespace RedEye.Core {
    public enum ConfigNodeEventType {
        AddNode,
        RemoveNode,
        SetAttribute,
        SetVariable
    }

    public class ConfigNodeEvent {
        public ConfigNodeEventType EventType = default;
        public ConfigNode Node = null;
        public ConfigNode AddedNode = null;
        public ConfigNode RemovedNode = null;
        public string AttributeName = null;
        public string OldAttributeValue = null;
        public string NewAttributeValue = null;
        public string VariableName = null;
        public string OldVariableValue = null;
        public string NewVariableValue = null;
    }

    internal struct ConfigNodeEventWatcher {
        public ConfigNodeEventWatcher(){}
        public Action<ConfigNodeEvent> EventWatcher = null;
        public bool IsRecursive = false;
    }

    public class ConfigNode : IVariableStorage<string> {
        string name = null;
        public string Name { get => name; }

        string value = null;
        public string Value { get => value; }

        bool isTextNode = false;
        public bool IsTextNode { get => isTextNode; }

        bool isAttribute = false;
        public bool IsAttribute { get => isAttribute; }

        ConfigNode parentNode = null;
        public ConfigNode ParentNode { get => parentNode; }

        ConfigNode rootNode = null;
        public ConfigNode RootNode {
            get {
                if(rootNode is not null) return rootNode;
                if(parentNode is null) return this;
                return parentNode.RootNode;
            }
        }

        Dictionary<string, Dictionary<string, string>> childNodeAttributes = new();
        Dictionary<string, ConfigNodeEventWatcher> watchers = new();
        Dictionary<string, string> variables = new();
        Dictionary<string, string> attributes = new();
        List<ConfigNode> childNodes = new();

        ComponentManager manager = null;

        IExpressionParser expressionParser = null;
        IPluginManager pluginManager = null;
        IScriptEngine engine = null;
        IConfig config = null;
        ILogger logger = null;

        public ConfigNode(ComponentManager manager, string name, IDictionary<string, string> attributes = null, string value = null, Dictionary<string, Dictionary<string, string>> childNodeAttributes = null){
            this.name = name;
            this.manager = manager;
            this.config = manager.GetComponent<IConfig>();
            this.logger = manager.GetComponent<ILogger>();
            this.engine = manager.GetComponent<IScriptEngine>();
            this.pluginManager = manager.GetComponent<IPluginManager>();
            this.expressionParser = manager.GetComponent<IExpressionParser>();

            if(attributes is not null){
                foreach(var kvp in attributes){
                    this.attributes.Add(kvp.Key, kvp.Value);
                }
            }

            if(value is null){
                this.isTextNode = false;
                this.value = string.Empty;
            }else{
                this.isTextNode = true;
                this.value = value;
            }

            if(childNodeAttributes is not null){
                this.childNodeAttributes = childNodeAttributes;
            }
        }

        public void SetParentNode(ConfigNode node){
            parentNode = node;
        }

        public void ProcessNodes(){
            for(int i = 0; i < childNodes.Count; i++){
                var node = childNodes[i];

                switch(node.Name){
                    case "variables": {
                        foreach(var setNode in node.GetNodes("set")){
                            SetVariable(setNode.GetAttribute("name"), setNode.GetAttribute("value"));
                        }

                        node.Remove();
                        break;
                    }

                    case "import": {
                        config.LoadFile(node.GetAttribute("from"), this);
                        node.Remove();
                        ProcessNodes();
                        break;
                    }

                    case "if": {
                        if(ParseHelper.ParseBool(node.GetAttribute("condition")) && node.TryGetNode("then", out var thenNode)){
                            foreach(var cNode in thenNode.GetNodes()){
                                childNodes.Insert(i, cNode);
                                cNode.SetParentNode(this);
                            }
                        }else if(node.TryGetNode("else", out var elseNode)){
                            foreach(var cNode in elseNode.GetNodes()){
                                childNodes.Insert(i, cNode);
                                cNode.SetParentNode(this);
                            }
                        }

                        node.Remove();
                        ProcessNodes();
                        break;
                    }

                    case "eval":
                    case "exec": {
                        node.SetValue(node.GetAttribute("action"));
                        break;
                    }
                }
            }
        }

        public string AddEventWatcher(Action<ConfigNodeEvent> eventWatcher, bool isRecursive = false){
            var name = Guid.NewGuid().ToString();
            watchers.Add(name, new(){ EventWatcher = eventWatcher, IsRecursive = isRecursive });
            return name;
        }

        void ProcessEventWatchers(ConfigNodeEvent @event){
            foreach(var watcher in watchers.Values){
                watcher.EventWatcher.Invoke(@event);
            }
        }

        public void RemoveEventWatcher(string name){
            watchers.Remove(name);
        }

        public void AddNode(ConfigNode node){
            node.SetParentNode(this);
            childNodes.Add(node);

            foreach(var watcher in watchers.Values.Where(w => w.IsRecursive)){
                node.AddEventWatcher(watcher.EventWatcher, true);
            }

            ProcessEventWatchers(new(){ EventType = ConfigNodeEventType.AddNode, AddedNode = node });
        }

        public IEnumerable<ConfigNode> GetRawNodes(){
            return childNodes;
        }

        public IEnumerable<ConfigNode> GetNodes(){
            ProcessNodes();
            return childNodes;
        }

        public IEnumerable<ConfigNode> GetNodes(Func<ConfigNode, bool> filter){
            return GetNodes().Where(filter);
        }

        public IEnumerable<ConfigNode> GetNodes(string name){
            return GetNodes(node => node.Name == name);
        }

        public ConfigNode GetNode(string name){
            var nodes = GetNodes(name);
            if(!nodes.Any()) return null;
            return nodes.First();
        }

        public bool TryGetNode(string name, out ConfigNode node){
            var nodes = GetNodes(name);
            
            if(!nodes.Any()){
                node = null;
                return false;
            }

            node = nodes.First();
            return true;
        }

        public ConfigNode this[string name]{
            get {
                var nodes = GetNodes(name);
                if(!nodes.Any()) throw new KeyNotFoundException($"No nodes with name \"{name}\" found");
                return nodes.First();
            }
        }

        public void RemoveNode(ConfigNode node){
            childNodes.Remove(node);

            ProcessEventWatchers(new(){ EventType =ConfigNodeEventType.RemoveNode, RemovedNode = node });
        }

        public void Remove(){
            parentNode.RemoveNode(this);
        }

        public void EnumRawNodes(Func<ConfigNode, bool> callback){
            foreach(var node in childNodes){
                callback.Invoke(node);
                node.EnumRawNodes(callback);
            }
        }

        public void EnumRawNodes(Func<ConfigNode, bool> filter, Func<ConfigNode, bool> callback){
            foreach(var node in childNodes){
                if(filter.Invoke(node)) callback.Invoke(node);
                node.EnumRawNodes(filter, callback);
            }
        }

        public void EnumNodes(Func<ConfigNode, bool> callback){
            foreach(var node in GetNodes()){
                callback.Invoke(node);
                node.EnumNodes(callback);
            }
        }

        public void EnumNodes(Func<ConfigNode, bool> filter, Func<ConfigNode, bool> callback){
            foreach(var node in GetNodes()){
                if(filter.Invoke(node)) callback.Invoke(node);
                node.EnumNodes(filter, callback);
            }
        }

        public void EnumNodes(string name, Func<ConfigNode, bool> callback){
            EnumNodes(node => node.Name == name, callback);
        }

        public string GetVariable(string name){
            if(variables.ContainsKey(name)){
                return variables[name];
            }else if(parentNode is not null){
                return parentNode.GetVariable(name);
            }else{
                return string.Empty;
            }
        }

        public IEnumerable<string> GetVariables(){
            return variables.Keys;
        }

        public void SetVariable(string name, string value){
            var oldValue = GetVariable(name);

            if(variables.ContainsKey(name)){
                variables[name] = value;
            }else{
                variables.Add(name, value);
            }

            ProcessEventWatchers(new(){ EventType = ConfigNodeEventType.SetVariable, OldVariableValue = oldValue, NewVariableValue = value });
        }

        public string GetRawValue(){
            return Value;
        }

        public string GetValue(){
            return ParseValue(Value);
        }
        
        public void SetValue(string value){
            this.value = value;
        }

        public IEnumerable<string> GetAttributes(){
            return attributes.Keys;
        }

        public string GetRawAttribute(string name, string defaultValue = ""){
            if(attributes.ContainsKey(name)){
                return attributes[name];
            }else{
                return defaultValue;
            }
        }

        public string GetAttribute(string name, string defaultValue = ""){
            return ParseValue(GetRawAttribute(name, defaultValue));
        }

        public void SetAttribute(string name, string value){
            var oldValue = GetAttribute(name);

            if(attributes.ContainsKey(name)){
                attributes[name] = value;
            }else{
                attributes.Add(name, value);
            }

            ProcessEventWatchers(new(){ EventType = ConfigNodeEventType.SetAttribute, OldAttributeValue = oldValue, NewAttributeValue = value });
        }

        public ConfigNode Clone(){
            var node = new ConfigNode(manager, name, attributes, value);

            foreach(var variable in variables){
                node.SetVariable(variable.Key, variable.Value);
            }

            foreach(var childNode in childNodes){
                node.AddNode(childNode.Clone());
            }

            return node;
        }

        string ParseValue(string value){
            return expressionParser.EvaluateExpression(value, this);
        }
   }
}
