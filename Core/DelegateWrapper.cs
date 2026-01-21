using System;

namespace RedEye.Core {
    public class DelegateWrapper {
        public object Data { get; set; }
        protected Action<object, object[]> callback = null;

        public DelegateWrapper(Action<object, object[]> callback){
            this.callback = callback;
        } 

        public void InvokeCallback(){
            callback.Invoke(Data, new object[0]);
        }

        public Delegate GetDelegate(Type delegateType){
            return Delegate.CreateDelegate(delegateType, this, nameof(InvokeCallback));
        }
    }

    public class DelegateWrapper<T1>: DelegateWrapper {
        public DelegateWrapper(Action<object, object[]> callback): base(callback){}

        public void InvokeCallback(T1 arg1){
            callback.Invoke(Data, new object[]{ arg1 }); 
        }
    }

    public class DelegateWrapper<T1, T2>: DelegateWrapper {
        public DelegateWrapper(Action<object, object[]> callback): base(callback){}

        public void InvokeCallback(T1 arg1, T2 arg2){
            callback.Invoke(Data, new object[]{ arg1, arg2 }); 
        }
    }
}
