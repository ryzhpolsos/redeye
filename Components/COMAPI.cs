using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.COM {
    static class ComStorage {
        public static ComponentManager Manager;
    }

    [ComVisible(true)]
    public class StringDictionary {
        IDictionary<string, string> dict;

        public void Init(IDictionary<string, string> dict){
            this.dict = dict;
        } 

        public string Get(string key){
            return dict[key];
        }
    }

    [ComVisible(true)]
    public class Message {
        public StringDictionary Data;

        public void Init(IDictionary<string, string> dict){
            if(dict is null){
                Data = null;
            }else{
                Data = new();
                Data.Init(dict);
            }
        }

        public bool HasData(){
            return Data is not null;
        }
    }

    [ComVisible(true)]
    [Guid("bdc3a5fa-6e67-4478-a7ad-96ae237ff33b")]
    public interface IShell {
        Message GetMessage(string rcid); 
        IComponent GetComponent(string name);
    }

    [ComVisible(true)]
    [ProgId("RedEye.Shell")]
    [Guid("97f6c3b5-9229-44ed-9b65-5e8fa7878ac6")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Shell : IShell {
        ICOMAPI comApi;

        public Shell(){
            comApi = ComStorage.Manager.GetComponent<ICOMAPI>();
        }

        public Message GetMessage(string rcid){
            Message msg = new();
            msg.Init(comApi.GetMessage(rcid));
            return msg;
        }

        public IComponent GetComponent(string name){
            return ComStorage.Manager.GetComponentByName("I" + name);
        }
    }
}

namespace RedEye.Components {
    [ComVisible(true)]
    public class COMAPIComponent : ICOMAPI {
        Dictionary<string, Queue<IDictionary<string, string>>> msgQueues = new();

        object lockObject = new();
        Dictionary<string, string> returnValues = new();

        ComponentManager manager;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            RedEye.COM.ComStorage.Manager = manager;
        }

        public void RegisterInROT(){
            IRunningObjectTable rot;
            IMoniker moniker;

            GetRunningObjectTable(0, out rot);
            CreateItemMoniker("!", "{97f6c3b5-9229-44ed-9b65-5e8fa7878ac6}", out moniker);
            rot.Register(1, new RedEye.COM.Shell(), moniker);
        }

        public IDictionary<string, string> GetMessage(string rcid){
            if(!msgQueues.ContainsKey(rcid)) return null;
            if(msgQueues[rcid].Count == 0) return null;
            return msgQueues[rcid].Dequeue();
        }

        public void SendMessage(string rcid, IDictionary<string, string> args){
            if(!msgQueues.ContainsKey(rcid)){
                msgQueues.Add(rcid, new());
            }

            msgQueues[rcid].Enqueue(args);
        }
    }
}
