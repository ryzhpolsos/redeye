var _windowList = {};
var shellEventType = ['create', 'destroy', 'minimize', 'restore', 'redraw', 'activate', 'layoutChange'];
var _eventsQueue = [];
var _winEventHooks = [];
var _layoutChangeHooks = [];

var _shellElements = {
    desktop: document.getElementById('desktop')
};

var _desktopSize = {};
var _taskbarSize = {};
var _registeredTaskbarHandler = false;

window._handleShellEvent = function(evType, woStr, evStr){
    var wnd = evType == -1 ? woStr : JSON.parse(woStr);
    var event = evType == -1 ? evStr : shellEventType[evType];

    if(globalConfig.core.debugMode) console.log('window "' + wnd.title + '" (#' + wnd.handle + '):', event);

    var lWnd = {};

    if(event == 'layoutChange'){
        for(var i in _layoutChangeHooks){
            _layoutChangeHooks[i](wnd.data);
        }

        return;
    }else if(event == 'create'){
        lWnd = {};
        objectAssign(lWnd, wnd);

        lWnd.remove = function(){
            delete _windowList[lWnd.handle];
        };

        lWnd.minimize = function(){
            rwExternal.DllCall_user32_ShowWindow(lWnd.handle, 6);
            //rwExternal.DllCall_user32_SendMessage(wnd.handle, 0x0112, 0xF020, 0);
        };

        lWnd.restore = function(){
            rwExternal.DllCall_user32_SetForegroundWindow(lWnd.handle);
            rwExternal.DllCall_user32_ShowWindow(lWnd.handle, 9);
            rwExternal.DllCall_user32_RedrawWindow(lWnd.handle, 0, 0, 257);
        };

        lWnd.show = function(){
            rwExternal.DllCall_user32_ShowWindow(lWnd.handle, 1);
            rwExternal.DllCall_user32_SetForegroundWindow(lWnd.handle);
        }

        lWnd.sendMessage = function(msg, wParam, lParam){
            rwExternal.DllCall_user32_SendMessage(lWnd.handle, msg, wParam, lParam);
        };

        _windowList[wnd.handle] = lWnd;
    }else{
        if(wnd.handle && _windowList[wnd.handle]){
            lWnd = _windowList[wnd.handle];
            objectAssign(lWnd, wnd);
        }
    }

    if(event == 'activate'){
        for(var i in _windowList){
            if(i != lWnd.handle){
                _windowList[i].isActive = false;
                _handleShellEvent(-1, _windowList[i], 'deactivate');
            }

            if(!rwExternal.DllCall_user32_IsWindow(i)){
                _handleShellEvent(-1, _windowList[i], 'destroy');
            }
        }
    }

    for(var i in _winEventHooks){
        var ret = _winEventHooks[i](event, lWnd);
        if(ret === false) return;
        if(ret) lWnd = ret;
    }

    if(!_registeredTaskbarHandler){
        _eventsQueue.push({ event: event, window: lWnd });
    }
};

window.setTaskbarBounds = function(left, top, width, height){
    _taskbarSize.left = left;
    _taskbarSize.top = top;
    _taskbarSize.width = width;
    _taskbarSize.height = height;
};

window.setDesktopBounds = function(left, top, width, height){
    _shellElements.desktop.style.position = 'absolute';
    _shellElements.desktop.style.left = left + 'px';
    _shellElements.desktop.style.top = top + 'px';
    _shellElements.desktop.style.width = width + 'px';
    _shellElements.desktop.style.height = height + 'px';

    _desktopSize.left = left;
    _desktopSize.top = top;
    _desktopSize.width = width;
    _desktopSize.height = height;
    rwExternal.SetDesktopSize(left, top, width, height);

    var zIndex = 10000;

    for(var i in globalConfig.shell.desktop.widgets){
        var widget = globalConfig.shell.desktop.widgets[i];

        var elem = document.createElement('div');
        elem.className = 'desktop-widget';
        elem.style.position = 'absolute';
        elem.style.zIndex = (widget.zIndex === undefined)?(++zIndex):widget.zIndex;

        if(widget.style) objectAssign(elem.style, widget.style);
        if(typeof widget.x !== undefined) elem.style.left = (typeof widget.x == 'number')?widget.x+'px':widget.x;
        if(typeof widget.y !== undefined) elem.style.top = (typeof widget.y == 'number')?widget.y+'px':widget.y;
        if(typeof widget.width !== undefined) elem.style.width = (typeof widget.width == 'number')?widget.width+'px':widget.width;
        if(typeof widget.height !== undefined) elem.style.height = (typeof widget.height == 'number')?widget.height+'px':widget.height;

        desktopWidgetHandlers[widget.name](elem, widget.args);
        _shellElements.desktop.appendChild(elem);
    }
};

window.getTaskbarBounds = function(){
    return _taskbarSize;
};

window.getDesktopBounds = function(){
    return _desktopSize;
}

window.registerTaskbar = function(pluginName, handler){
    _registeredTaskbarHandler = true;

    if(pluginName == globalConfig.shell.taskbar.handler){
        window.addEventListener('DOMContentLoaded', handler);
    }

    if(_eventsQueue.length > 0){
        for(let i in _eventsQueue){
            _handleShellEvent(-1, _eventsQueue[i].window, _eventsQueue[i].event);
        }
    }
};

window.registerWindowEventHook = function(hookProc){
    _winEventHooks.push(hookProc);
};

window.registerLayoutChangeHook = function(hookProc){
    _layoutChangeHooks.push(hookProc);
};

window.getWindowList = function(){
    return _windowList;
}

_shellElements.desktop.style.background = globalConfig.shell.desktop.background.value;