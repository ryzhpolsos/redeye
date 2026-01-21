#define UNICODE
#include <windows.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include "wmx.h"

#define STR(x) __STR(x)
#define __STR(x) #x

#ifdef CL_NAME
    #define DLL_NAME STR(CL_NAME)
    #define CLASS_NAME "RedEye_Wmx64Wnd"
#elif X32
    #define DLL_NAME "wmx32.dll"
    #define CLASS_NAME "RedEye_Wmx32Wnd"
#else
    #define DLL_NAME "wmx64.dll"
    #define CLASS_NAME "RedEye_Wmx64Wnd"
#endif

typedef void (WINAPI *WmxSetParam)(int param, int value);

HHOOK hhk = NULL;
BOOL unhooked = FALSE;

UINT wmxMsg = -1;
WmxSetParam setParam = NULL;

WNDENUMPROC a;

BOOL CALLBACK killWmxEnumProc(HWND hWnd, LPARAM lParam){
    SendMessage(hWnd, wmxMsg, WMX_PARAM_EXIT, 0);
    return TRUE;
}

void killWmx(){
    if(!unhooked){
        EnumWindows(killWmxEnumProc, 0);
        UnhookWindowsHookEx(hhk);
        unhooked = TRUE;
    }
}

LRESULT WINAPI WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam){
    if(msg == wmxMsg){
        if(setParam != NULL) setParam((int)wParam, (int)lParam);
    }else if(msg == WM_CLOSE){
        killWmx();
        PostQuitMessage(0);
    }else{
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    return 0;
}

int WINAPI WinMain(HINSTANCE hInst, HINSTANCE hPrevInst, LPSTR lpCmdLine, int nCmdShow){
    HINSTANCE hmDll = LoadLibrary(TEXT(DLL_NAME));
    setParam = (WmxSetParam)GetProcAddress(hmDll, "SetParam");
    HOOKPROC hookProc = (HOOKPROC)GetProcAddress(hmDll, "HookProc");

    wmxMsg = RegisterWindowMessage(TEXT(MSG_NAME));

    setParam(WMX_PARAM_X, atol(strtok(lpCmdLine, " ")));
    setParam(WMX_PARAM_Y, atol(strtok(NULL, " ")));
    setParam(WMX_PARAM_W, atol(strtok(NULL, " ")));
    setParam(WMX_PARAM_H, atol(strtok(NULL, " ")));
    setParam(WMX_PARAM_SHELLHOOK, atol(strtok(NULL, " ")));

    hhk = SetWindowsHookEx(WH_CBT, hookProc, hmDll, 0);

    WNDCLASS wc = {0};
    wc.lpszClassName = TEXT(CLASS_NAME);
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInst;
    RegisterClass(&wc);

    CreateWindowEx(0, wc.lpszClassName, NULL, 0, 0, 0, 0, 0, HWND_MESSAGE, NULL, NULL, NULL);

    MSG msg;
    while(GetMessage(&msg, NULL, 0, 0) != 0){
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    killWmx();
}
