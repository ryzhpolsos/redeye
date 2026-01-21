using System;
using System.Runtime.InteropServices;

using RedEye.Core;
using RedEye.PluginAPI;

internal static class NativeFunctions {
    [DllImport("user32.dll", EntryPoint="#61", CharSet=CharSet.Auto)]
    public static extern bool RunFileDlg(IntPtr hWnd, IntPtr hIcon, string lpszDirectory, string lpszTitle, string lpszDescription, int uFlags);
}

public class RunBoxPlugin : Plugin {
    public override void Initialize(){
        pluginManager.ExportFunction("openRunBox", (args) => {
            NativeFunctions.RunFileDlg(IntPtr.Zero, IntPtr.Zero, null, null, null, 0);
        });
    }
}
