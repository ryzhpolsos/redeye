using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using RedEye.Core;
using static RedEye.Core.NativeHelper;

namespace RedEye.Components {
    internal struct HotKey {
        public Func<bool> Handler;
        public IEnumerable<string> Keys;
        public bool AllowMultiActivate;
    }

    public class HotKeyManagerComponent : IHotKeyManager {
        ComponentManager manager = null;
        IExplorerIntegration explorerIntegration = null;
 
        List<string> keys = new List<string>();
        List<HotKey> hotKeys = new List<HotKey>();
        List<Func<string, bool, bool>> keyHandlers = new List<Func<string, bool, bool>>();

        LowLevelProc kbProc = null;

        Dictionary<string, List<string>> keyMap = new(){
            { "Ctrl", new(){ "LControlKey", "RControlKey" } },
            { "Alt", new(){ "LMenu", "RMenu" } },
            { "Shift", new(){ "LShiftKey", "RShiftKey" } },
            { "Win", new(){ "LWin", "RWin" } },
            { "0", new(){ "D0" } },
            { "1", new(){ "D1" } },
            { "2", new(){ "D2" } },
            { "3", new(){ "D3" } },
            { "4", new(){ "D4" } },
            { "5", new(){ "D5" } },
            { "6", new(){ "D6" } },
            { "7", new(){ "D7" } },
            { "8", new(){ "D8" } },
            { "9", new(){ "D9" } },
            { ".", new(){ "OemPeriod" } },
            { ",", new(){ "Oemcomma" } }
        };

        int lastLength = 0;

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            explorerIntegration = manager.GetComponent<IExplorerIntegration>();

            kbProc = KbHandler;
            var kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, kbProc, IntPtr.Zero, 0);
        }

        public void RegisterKeyHandler(Func<string, bool, bool> handler){
            keyHandlers.Add(handler);
        }

        public void RegisterHotKey(IEnumerable<string> keys, Func<bool> handler, bool allowMultiActivate = false){
            hotKeys.Add(new(){ Keys = keys.OrderBy(x => x), Handler = handler, AllowMultiActivate = allowMultiActivate });
        }

        bool IsPrintableKey(string keyName){
            return keyName.Length == 1 || (keyName.Length == 2 && keyName[0] == 'D') || keyName == "OemPeriod" || keyName == "Oemcomma";
        }

        int KbHandler(int nCode, int wParam, IntPtr lParam){
            bool isUp = false, found = false;
            string keyName = string.Empty;

            if(lParam != IntPtr.Zero){
                var kbDll = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                keyName = ((Keys)kbDll.vkCode).ToString();
                isUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;

                foreach(var handler in keyHandlers){
                    if(!handler.Invoke(keyName, isUp)) return 0;
                }

                foreach(var handler in hotKeys.OrderBy(h => h.Keys.Count()).Reverse()){
                    if(handler.Keys.Count() < lastLength) continue;

                    found = true;

                    for(int i = 0; i < handler.Keys.Count(); i++){
                        if(keys.Count <= i){
                            found = false;
                            break;
                        }

                        var key = handler.Keys.ElementAt(i);
                        if(keyMap.ContainsKey(key) ? !keys.Any(k => keyMap[key].Contains(k)) : !keys.Contains(key)){
                            found = false;
                            break;
                        }
                    }

                    Console.WriteLine(string.Join(",", handler.Keys) + " " + string.Join(",", keys) + $" found: {found} isUp: {isUp}");

                    if(found){
                        lastLength = handler.Keys.Count();

                        if(isUp || handler.AllowMultiActivate){
                            if(!handler.Handler.Invoke()){
                                break;
                            }

                            keys.Clear();
                        }
                    }

                    if(found){
                        break;
                    }
                }

                if(keys.Any()){
                    keys = keys.Distinct().ToList();
                }else{
                    lastLength = 0;
                }

                if(isUp){
                    keys.Remove(keyName);
                }else{
                    keys.Add(keyName);
                }
            }

            if(
                (found && IsPrintableKey(keyName)) ||
                (explorerIntegration.GetIsEnabled() && (keyName == "LWin" || keyName == "RWin"))
            ){
                return 1;
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam); 
        }
    }
}
