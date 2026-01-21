using System;
using System.Text;
using System.Runtime.InteropServices;

namespace RedEye.Core {
    public class NativeHelper {
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int RedrawWindow(IntPtr hWnd, IntPtr _u1, IntPtr _u2, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndAfter, string lpClass, IntPtr lpName);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int RegisterClassEx(ref WNDCLASSEX wndClass);

        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetMessage(ref MSG lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, long lParam, StringBuilder wParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, long lParam, long wParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int lParam, int wParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetAncestor(IntPtr hWnd, int gaFlags);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern IntPtr SetShellWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetProgmanWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetTaskmanWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern uint GetClassLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, ref RECT pvParam, int fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, ref MINIMIZEDMETRICS pvParam, int fWinIni);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int LCIDToLocaleName(int Locale, StringBuilder lpName, int cchName, int dwFlags);

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int GetLocaleInfoEx(string lpLocaleName, int LCType, StringBuilder lpLCData, int cchData);

        [DllImport("user32.dll")]
        public static extern int GetKeyboardLayout(int idThread);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, long value);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hmod, int dwThreadId);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int ActivateKeyboardLayout(int hkl, int flags);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("shell32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string pszExeFileName, int nIconIndex);

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);

        [DllImport("Dxva2.dll")]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, int dwPhysicalMonitorArraySize, ref _PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("Dxva2.dll")]
        public static extern bool GetMonitorCapabilities(IntPtr hMonitor, out int pdwMonitorCapabilities, out int pdwSupportedColorTemperatures);

        [DllImport("Dxva2.dll", SetLastError=true)]
        public static extern bool GetMonitorBrightness(IntPtr hMonitor, out int pdwMinimumBrightness, out int pdwCurrentBrightness, out int pdwMaximumBrightness);

        [DllImport("Dxva2.dll")]
        public static extern bool SetMonitorBrightness(IntPtr hMonitor, int dwNewBrightness);

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_NOZORDER = 0x0004;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_CLOSE = 16;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public static readonly IntPtr HWND_BOTTOM = (IntPtr)1;
        public static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
        public static readonly IntPtr HWND_MESSAGE = (IntPtr)(-3);
        public const int HSHELL_GETMINRECT = 5;
        public const int HSHELL_REDRAW = 6;
        public const int HSHELL_WINDOWCREATED = 1;
        public const int HSHELL_WINDOWDESTROYED = 2;
        public const int HSHELL_WINDOWACTIVATED = 4;
        public const int HSHELL_RUDEAPPACTIVATED = 32772;
        public const int GA_ROOT = 2;
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_OVERLAPPEDWINDOW = 0x300;
        public const int WS_EX_TOOLWINDOW = 0x80;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WM_QUIT = 0x0012;
        public const int WM_GETICON = 0x7F;
        public const int WM_INPUTLANGCHANGE = 81;
        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;
        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;
        public const int SPI_SETMINIMIZEDMETRICS = 0x002C;
        public const int SPI_SETWORKAREA = 0x002F;
        public const int ARW_HIDE = 8;
        public const int SPIF_SENDCHANGE = 0x2;
        public const int CS_GLOBALCLASS = 16384;
        public const int LOCALE_NAME_MAX_LENGTH = 85;
        public const int LOCALE_SISO639LANGNAME2 = 103;
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;
        public const int INPUTLANGCHANGE_FORWARD = 0x2;
        public const int HKL_NEXT = 1;
        public const int MONITOR_DEFAULTTOPRIMARY = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public long left;
            public long right;
            public long top;
            public long bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public POINT pt;
            public int lPrivate;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct WNDCLASSEX {
            public int cbSize;
            public int style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
            public RECT rcDeivce;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLHOOKINFO {
            public IntPtr hwnd;
            public RECT rc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINIMIZEDMETRICS {
            public int cbSize;
            public int iWidth;
            public int iHorzGap;
            public int iVertGap;
            public int iArrange;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public long dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSDLLHOOKSTRUCT {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public long dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct _PHYSICAL_MONITOR {
            public IntPtr hPhysicalMonitor;
            public ushort[] szPhysicalMonitorDescription;
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int LowLevelProc(int nCode, int wParam, IntPtr lParam);

        public static void MinimizeWindow(IntPtr hWnd){
            ShowWindow(hWnd, 6);
        }

        public static void RestoreWindow(IntPtr hWnd){
            ShowWindow(hWnd, 9);
            RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, 257);
            SetForegroundWindow(hWnd);
        }

        public static void ActivateWindow(IntPtr hWnd){
            ShowWindow(hWnd, 5);
            SetForegroundWindow(hWnd);
        }

        public static void CloseWindow(IntPtr hWnd){
            SendMessage(hWnd, WM_CLOSE, 0, 0);
        }
    }
}
