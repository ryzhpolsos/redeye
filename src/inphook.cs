using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static RedEye.Native;

namespace RedEye {
    public class InputHook {
        public static void Start(){
            var kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, (nCode, wParam, lParam)=>{
                var kbDll = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                try{
                    DskForm.Instance.WebWrapper.ExecuteFunction("_handleKbEvent", new object[]{ (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)?"down":"up", ((Keys)kbDll.vkCode).ToString() });
                }catch(Exception){}

                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }, IntPtr.Zero, 0);

            if(kbHook == IntPtr.Zero){
                Logger.Log(Logger.MessageType.Critical, "Failed to set keyboard hook");
            }

            var msHook = SetWindowsHookEx(WH_MOUSE_LL, (nCode, wParam, lParam)=>{
                var msDll = Marshal.PtrToStructure<MSDLLHOOKSTRUCT>(lParam);
                string msgName = "";

                switch(wParam){
                    case WM_LBUTTONDOWN: {
                        msgName = "ldown";
                        break;
                    }
                    case WM_LBUTTONUP: {
                        msgName = "lup";
                        break;
                    }
                    case WM_RBUTTONDOWN: {
                        msgName = "rdown";
                        break;
                    }
                    case WM_RBUTTONUP: {
                        msgName = "rup";
                        break;
                    }
                    case WM_MOUSEMOVE: {
                        msgName = "move";
                        break;
                    }
                }

                try{
                    DskForm.Instance.WebWrapper.ExecuteFunction("_handleMsEvent", new object[]{ msgName, msDll.pt.x, msDll.pt.y });
                }catch(Exception){}

                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }, IntPtr.Zero, 0);

            if(msHook == IntPtr.Zero){
                Logger.Log(Logger.MessageType.Critical, "Failed to set mouse hook");
            }
        }
    }
}
