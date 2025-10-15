using System;
using System.Net;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RedEye {
    public class ExternalObject {
        public ExternalObject(Type t, object o){
            type = t;
            obj = o;
        }

        public Type type;
        public object obj;
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptManager {
        WebForm form;        
        string objectGuidHeader;
        string formObjectGuid;
        Dictionary<string, ExternalObject> extObjects;
        List<Assembly> loadedAssemblies;

        public ScriptManager(WebForm f){
            form = f;
            objectGuidHeader = "HTW_HEADER_" + Guid.NewGuid().ToString() + ":";
            extObjects = new Dictionary<string, ExternalObject>();
            loadedAssemblies = new List<Assembly>();

            formObjectGuid = Guid.NewGuid().ToString();
            extObjects.Add(formObjectGuid, new ExternalObject(typeof(Form), form));
        }

        public void Close(){
            form.Close();
        }

        public void MoveTo(int x, int y){
            form.Location = new Point(x, y);
        }

        public void MoveBy(int x, int y){
            form.Location = new Point(form.Location.X + x, form.Location.Y + y);
        }

        public void ResizeTo(int w, int h){
            form.Size = new Size(w, h);
        }

        public void ResizeBy(int w, int h){
            form.Size = new Size(form.Size.Width + w, form.Size.Height + h);
        }

        public void SetTitle(string title){
            form.Text = title;
        }

        public void SetWindowState(int state){
            form.WindowState = (FormWindowState)state;
        }

        public void SetDesktopSize(int x, int y, int width, int height){
            //MessageBox.Show($"{x} {y} {width} {height}");
            WmxAPI.SetDesktopSize(x, y, width, height);
        }

        public string GetCurrentKeyboardLayout(){
            var fgWin = Native.GetForegroundWindow();
            var threadId = Native.GetWindowThreadProcessId(fgWin, out int _);
            var hkl = Native.GetKeyboardLayout(threadId);

            var localeName = new StringBuilder();
            Native.LCIDToLocaleName(hkl >> 16, localeName, Native.LOCALE_NAME_MAX_LENGTH, 0);

            var lang = new StringBuilder();
            Native.GetLocaleInfoEx(localeName.ToString(), Native.LOCALE_SISO639LANGNAME2, lang, 8);
            return lang.ToString();
        }

        public void NextKeyboardLayout(){
            var fgWin = Native.GetForegroundWindow();
            Native.SendMessage(fgWin, Native.WM_INPUTLANGCHANGEREQUEST, Native.INPUTLANGCHANGE_FORWARD, Native.ActivateKeyboardLayout(Native.HKL_NEXT, 0));
        }

        public void OpenDevTools(){
            form.WebWrapper.OpenDevTools();
        }

        public string GetPath(string path){
            return Util.GetPath(path);
        }

        public void LogError(string msg){
            Logger.Log(Logger.MessageType.Error, msg);
        }

        public void ReloadHtml(){
            DskForm.Instance.Reload();
        }

        public void OpenRunBox(string title, string description){
            Native.RunFileDlg(IntPtr.Zero, IntPtr.Zero, null, title.Length>0?title:null, description.Length>0?description:null, 0);
        }

        public object DllCall(string libName, string funcName, string retType, string argsStr){
            var args = new List<DllCallerLib.Argument>();
            var rawArgs = Util.ParseJsArray(argsStr);

            if(rawArgs.Length != 0 && rawArgs.Length % 2 != 0){
                return "ERR,Invalid argument count";
            }

            string longType = Util.GetFullTypeName(retType);

            if(retType == null){
                return "ERR,Invalid argument type";
            }

            for(int i = 0; i < rawArgs.Length; i+=2){
                var type = Util.GetFullTypeName(rawArgs[i].ToString());
                var obj = rawArgs[i+1];

                if(type == "System.IntPtr"){
                    obj = new IntPtr((int)obj);
                }

                args.Add(new DllCallerLib.Argument(type, obj));
            }

            return DllCallerLib.DllCaller.CallFunction(libName, funcName, longType, args);
        }

        public void DllCall_user32_SendMessage(int handle, int msgType, int lParam, int wParam){
            Native.SendMessage((IntPtr)handle, msgType, lParam, wParam);
        }

        public void DllCall_user32_ShowWindow(int handle, int cmdShow){
            Native.ShowWindow((IntPtr)handle, cmdShow);
        }

        public void DllCall_user32_SetForegroundWindow(int handle){
            Native.SetForegroundWindow((IntPtr)handle); 
        }

        public void DllCall_user32_RedrawWindow(int handle, int _u1, int _u2, int flags){
            Native.RedrawWindow((IntPtr)handle, (IntPtr)_u1, (IntPtr)_u2, flags);
        }

        public void DllCall_user32_SetWindowPos(int handle, int hia, int x, int y, int cx, int cy, int flags){
            Native.SetWindowPos((IntPtr)handle, (IntPtr)hia, x, y, cx, cy, (uint)flags);
        }
        
        public bool DllCall_user32_IsWindow(int handle){
            return Native.IsWindow((IntPtr)handle);
        }

        public string GetObjectGuidHeader(){
            return objectGuidHeader;
        }

        public string CreateObject(int objectType, string typeName, string argsStr){
            object[] args = Util.ParseJsArray(argsStr);
            for(int i = 0; i < args.Length; i++){
                object arg = args[i];

                if(arg.GetType().FullName == "System.String"){
                    string ar = arg.ToString();
                    if(ar.StartsWith(objectGuidHeader)){
                        args[i] = extObjects[ar.Substring(objectGuidHeader.Length)].obj;
                    }
                }
            }

            Type type = null;
            object obj = null;

            if(objectType == 0 || objectType == 1){
                if(loadedAssemblies.Count == 0){
                    loadedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
                }

                foreach(Assembly asm in loadedAssemblies){
                    type = asm.GetType(typeName);
                    if(type != null) break;
                }

                if(objectType == 0) obj = Activator.CreateInstance(type, args);
            }else if(objectType == 2){
                type = Type.GetTypeFromProgID(typeName);
                if(type == null) type = Type.GetTypeFromCLSID(new Guid(typeName));
                obj = Activator.CreateInstance(type, args);
            }else if(objectType == 3){
                type = Type.GetTypeFromProgID(typeName);
                obj = Marshal.GetActiveObject(typeName);
            }

            string name = Guid.NewGuid().ToString();
            extObjects.Add(name, new ExternalObject(type, obj));
            return name;
        }

        public object InvokeObjectMember(int invType, string objectGuid, string methodName, string argsStr){
            object[] args = Util.ParseJsArray(argsStr);
            for(int i = 0; i < args.Length; i++){
                object arg = args[i];

                if(arg.GetType().FullName == "System.String"){
                    string ar = arg.ToString();
                    if(ar.StartsWith(objectGuidHeader)){
                        args[i] = extObjects[ar.Substring(objectGuidHeader.Length)].obj;
                    }
                }
            }

            BindingFlags bf = BindingFlags.Default;
            if(invType == 0) bf = BindingFlags.GetProperty;
            else if(invType == 1) bf = BindingFlags.SetProperty;
            else if(invType == 2) bf = BindingFlags.InvokeMethod;

            ExternalObject eobj = extObjects[objectGuid];
            object result = eobj.type.InvokeMember(methodName, bf, null, eobj.obj, args);
            if(result == null) return null;

            string name = result.GetType().FullName;

            if(name != "System.String" && name != "System.Boolean" && name != "System.Int32" && name != "System.IntPtr" && name != "System.Int64" && name != "System.UInt32" && name != "System.UInt64" && name != "System.Single" && name != "System.Double"){
                string guid = Guid.NewGuid().ToString();
                extObjects.Add(guid, new ExternalObject(result.GetType(), result));
                return objectGuidHeader + guid;
            }

            return result;
        }

        public string GetObjectMembers(int mmbType, string objectGuid){
            Type type = extObjects[objectGuid].type;
            MemberInfo[] memberInfo = null;
            List<string> names = new List<string>();

            if(mmbType == 0) memberInfo = type.GetProperties();
            else if(mmbType == 1) memberInfo = type.GetMethods();

            foreach(MemberInfo mi in memberInfo){
                names.Add(mi.Name);
            }

            return string.Join(";", names.ToArray());
        }

        public string GetFormObjectGuid(){
            return formObjectGuid;
        }
    }
}