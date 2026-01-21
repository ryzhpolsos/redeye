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
    }

    public class HotKeyManagerComponent : IHotKeyManager {
        ComponentManager manager = null;
 
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

        public void SetManager(ComponentManager manager){
            this.manager = manager;
        }

        public void Initialize(){
            kbProc = KbHandler;
            var kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, kbProc, IntPtr.Zero, 0);
        }

        int KbHandler(int nCode, int wParam, IntPtr lParam){
            if(lParam != IntPtr.Zero){
                var kbDll = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                var keyName = ((Keys)kbDll.vkCode).ToString();
                bool isUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;

                // Console.WriteLine(keyName);

                foreach(var handler in keyHandlers){
                    if(!handler.Invoke(keyName, isUp)) return 0;
                }

                // List<List<string>> keyLists = new();
                // keyLists.Add(keys);

                // for(int i = 0; i < keys.Count; i++){
                //     if(keyMap.ContainsKey(keys[i])){
                //         foreach(var mappedKey in keyMap[keys[i]]){
                //             List<string> newKeys = new();
                //             newKeys.AddRange(keys);
                //             newKeys[i] = mappedKey;
                //             newKeys.Sort();
                //             keyLists.Add(newKeys);
                //         }
                //     }
                // }

                bool found = false;

                foreach(var handler in hotKeys){
                    List<List<string>> keyLists = new();
                    keyLists.Add(handler.Keys.ToList());

                    for(int i = 0; i < handler.Keys.Count(); i++){
                        var key = handler.Keys.ElementAt(i);

                        if(keyMap.ContainsKey(key)){
                            foreach(var mappedKey in keyMap[key]){
                                List<string> newKeys = new();
                                newKeys.AddRange(handler.Keys);
                                newKeys[i] = mappedKey;
                                newKeys.Sort();
                                keyLists.Add(newKeys);
                            }
                        }
                    }

                    foreach(var keyList in keyLists){
                        if(keyList.SequenceEqual(keys)){
                            found = true;

                            if(!handler.Handler.Invoke()){
                                break;
                            }
                        }
                    }

                    if(found && keys.Any()) keys.Clear();
                }

                if(isUp){
                    keys.Remove(keyName);
                }else{
                    keys.Add(keyName);
                }

                if(keys.Any()) keys.Sort();
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public void RegisterKeyHandler(Func<string, bool, bool> handler){
            keyHandlers.Add(handler);
        }

        public void RegisterHotKey(IEnumerable<string> keys, Func<bool> handler){
            Console.WriteLine("Registering!");
            hotKeys.Add(new(){ Keys = keys.OrderBy(x => x), Handler = handler });
        }
    }
}
