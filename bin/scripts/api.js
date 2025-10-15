function link(name, thisArg){
    return new Function('var n = ' + name + '; return typeof(n)=="function"?n.apply(' + (thisArg||'window') + ', arguments):n;');
}

var File = new NetClass('System.IO.File');

window.redeye = {
    config: globalConfig,
    updateBackground: function(){
        _shellElements.desktop.style.background = globalConfig.shell.desktop.background.value;
    },
    keyboardLayout: {
        getCurrent: link('rwExternal.GetCurrentKeyboardLayout'),
        selectNext: link('rwExternal.NextKeyboardLayout'),
        registerHandler: link('window.registerLayoutChangeHook'),
    },
    file: {
        read: function(name){
            return File.ReadAllText(name);
        },
        write: function(name, text){
            File.WriteAllText(name, text);
        },
        getPath: function(path){
            return rwExternal.GetPath(path);
        }
    },
    inputHook: {
        register: function(type, param, listener){
            return _inputHooks[type].push({ param: param, listener: listener }) - 1;
        },
        remove: function(type, id){
            _inputHooks[type].splice(id, 1);
        }
    },
    locale: {
        store: {},
        getString: function(id){
            return this.store[globalConfig.locale.language][id];
        },
        gs: function(id){
            return this.getString(id);
        },
        str: function(id){
            return this.getString(id);
        },
        getCurrentLanguage: function(){
            return globalConfig.locale.language;
        }
    },
    reloadShell: link('rwExternal.ReloadHtml'),
    getWindowList: link('window.getWindowList'),
    exportedFunction: window.exportedFunction,
    taskbarWidgetHandlers: window.taskbarWidgetHandlers,
    desktopWidgetHandlers: window.desktopWidgetHandlers,
    setDesktopBounds: link('window.setDesktopBounds'),
    setTaskbarBounds: link('window.setTaskbarBounds'),
    getTaskbarBounds: link('window.getTaskbarBounds'),
    getDesktopBounds: link('window.getDesktopBounds'),
    registerWindowEventHook: link('window.registerWindowEventHook')
};

window.lstr = function(s){
    return redeye.locale.getString(s);
};

exportedFunction.register('shell.reload', link('rwExternal.ReloadHtml'));
exportedFunction.register('shell.openDevTools', link('rwExternal.OpenDevTools'));
exportedFunction.register('shell.nextKeyboardLayout', link('rwExternal.NextKeyboardLayout'));

exportedFunction.register('shell.runProcess', function(args){
    alert(JSON.stringify(args));
});