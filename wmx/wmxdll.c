#define UNICODE
#include <windows.h>
#include <stdlib.h>
#include "wmx.h"

typedef UINT (WINAPI *pGetDpiForWindow)(HWND);

typedef struct {
    long x;
    long y;
    long w;
    long h;
    long shellHook;
} SharedData;

typedef struct {
    HWND hWnd;
    WNDPROC WndProc;
} WndProcEntry;

WndProcEntry wpList[1000];
int wpCount = 0;

HANDLE hMapFile = NULL;
SharedData* pSharedData = NULL;

UINT wmxMsg = -1;
UINT wmxResMsg = -1;
UINT shellHookMsg = -1;
HWND rwMsgWin = NULL;

pGetDpiForWindow fGetDpiForWindow = NULL;

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD fdwReason, LPVOID lpvReserved){
    switch(fdwReason){
        case DLL_PROCESS_ATTACH: {
            hMapFile = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, sizeof(SharedData), TEXT(MAP_NAME));
            if(hMapFile == NULL) return FALSE;

            pSharedData = (SharedData*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(SharedData));

            if(pSharedData == NULL){
                CloseHandle(hMapFile);
                return FALSE;
            }

            wmxMsg = RegisterWindowMessage(TEXT(MSG_NAME));
            wmxResMsg = RegisterWindowMessage(TEXT(RES_MSG_NAME));
            shellHookMsg = RegisterWindowMessage(TEXT("SHELLHOOK"));
            rwMsgWin = FindWindowEx(HWND_MESSAGE, NULL, TEXT(RW_WND_NAME), NULL);

            HMODULE user32 = GetModuleHandle(TEXT("user32.dll"));
            if(user32){
                fGetDpiForWindow = (pGetDpiForWindow)GetProcAddress(user32, "GetDpiForWindow");
            }

            break;
        }
        case DLL_PROCESS_DETACH: {
            if(pSharedData){
                UnmapViewOfFile(pSharedData);
                pSharedData = NULL;
            }

            if(hMapFile){
                CloseHandle(hMapFile);
                hMapFile = NULL;
            }
        }
    }

    return TRUE;
}

void __declspec(dllexport) SetParam(int param, long value){
    switch(param){
        case WMX_PARAM_X: {
            pSharedData->x = value;
            break;
        }
        case WMX_PARAM_Y: {
            pSharedData->y = value;
            break;
        }
        case WMX_PARAM_W: {
            pSharedData->w = value;
            break;
        }
        case WMX_PARAM_H: {
            pSharedData->h = value;
            break;
        }
        case WMX_PARAM_SHELLHOOK: {
            pSharedData->shellHook = value;
            break;
        }
    }
}

LRESULT WINAPI HookWndProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam){
    if(uMsg == wmxMsg){
        if((int)wParam == WMX_PARAM_EXIT){
            for(int i = 0; i < wpCount; i++){
                if(wpList[i].hWnd == hWnd){
                    SetWindowLongPtr(hWnd, GWLP_WNDPROC, (LONG_PTR)wpList[i].WndProc);
                    return 0;
                }
            }
        }else{
            SetParam((int)wParam, (int)lParam);
        }
    }else if(uMsg == WM_GETMINMAXINFO){
        PMINMAXINFO pmmi = (PMINMAXINFO)lParam;

        UINT dpi = (fGetDpiForWindow == NULL || 1)?96:fGetDpiForWindow(hWnd);

        int x = MulDiv(pSharedData->x, dpi, 96);
        int y = MulDiv(pSharedData->y, dpi, 96);
        int w = MulDiv(pSharedData->w, dpi, 96);
        int h = MulDiv(pSharedData->h, dpi, 96);

        // why? because it's windows :D
        x -= 8;
        w += 16;
        h += 8;

        pmmi->ptMaxPosition.x = x;
        pmmi->ptMaxPosition.y = y;
        pmmi->ptMaxSize.x = w;
        pmmi->ptMaxSize.y = h;

        return 0;
    }else if(uMsg == WM_INPUTLANGCHANGE){
        #ifndef X32
        SendMessage(rwMsgWin, wmxResMsg, WMX_RES_LANG, LOWORD(lParam));
        #endif
    }else{
        LRESULT res = 0;

        for(int i = 0; i < wpCount; i++){
            if(wpList[i].hWnd == hWnd){
                res = CallWindowProc(wpList[i].WndProc, hWnd, uMsg, wParam, lParam);
            }
        }

        if((GetWindowLongPtr(hWnd, GWL_EXSTYLE) & WS_EX_OVERLAPPEDWINDOW)){
            switch(uMsg){
                case WM_CREATE: {
                    if(pSharedData->shellHook){
                        SendMessage(rwMsgWin, shellHookMsg, HSHELL_WINDOWCREATED, (LPARAM)hWnd);
                    }

                    break;
                }
                case WM_DESTROY: {
                    if(pSharedData->shellHook){
                        SendMessage(rwMsgWin, shellHookMsg, HSHELL_WINDOWDESTROYED, (LPARAM)hWnd);
                    }

                    break;
                }
                case WM_SETTEXT:
                case WM_SETICON: {
                    if(pSharedData->shellHook){
                        SendMessage(rwMsgWin, shellHookMsg, HSHELL_REDRAW, (LPARAM)hWnd);
                    }

                    break;
                }
                case WM_SIZE: {
                    if(pSharedData->shellHook){
                        SendMessage(rwMsgWin, shellHookMsg, HSHELL_GETMINRECT, (LPARAM)hWnd);
                    }

                    break;
                }
                case WM_ACTIVATE: {
                    if(pSharedData->shellHook){
                        SendMessage(rwMsgWin, shellHookMsg, HSHELL_WINDOWACTIVATED, (LPARAM)hWnd);
                    }

                    break;
                }
            }
        }

        return res;
    }
}

LRESULT __declspec(dllexport) HookProc(int code, WPARAM wParam, LPARAM lParam){
    switch(code){
        case HCBT_CREATEWND: {
            WndProcEntry wpe = {0};
            wpe.hWnd = (HWND)wParam;
            wpe.WndProc = (WNDPROC)GetWindowLongPtr((HWND)wParam, GWLP_WNDPROC);

            wpList[wpCount++] = wpe;
            SetWindowLongPtr((HWND)wParam, GWLP_WNDPROC, (LONG_PTR)HookWndProc);

            break;
        }
    }

    return CallNextHookEx(0, code, wParam, lParam);
}
