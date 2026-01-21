#ifndef _WMX_H
#define _WMX_H

#ifdef X32
    #define MAP_NAME "Local\\RedEye_Wmx32Crd"
    #define MSG_NAME "RedEye_Wmx32Msg"
#else
    #define MAP_NAME "Local\\RedEye_Wmx64Crd"
    #define MSG_NAME "RedEye_Wmx64Msg"
#endif

#define RES_MSG_NAME "RedEye_WmxResMsg"
#define RW_WND_NAME "RedEye_ShellWnd"

#define WMX_PARAM_EXIT -1
#define WMX_PARAM_X 0
#define WMX_PARAM_Y 1
#define WMX_PARAM_W 2
#define WMX_PARAM_H 3
#define WMX_PARAM_SHELLHOOK 4

#define WMX_RES_LANG 0

#ifndef SM_CXPADDEDBORDER
#define SM_CXPADDEDBORDER 92
#endif

#endif