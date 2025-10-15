var taskbarWidgetHandlers = {};

window.registerTaskbarWidget = function(pluginName, name, handler){
    taskbarWidgetHandlers[pluginName+'.'+name] = handler;
};

var desktopWidgetHandlers = {};

window.registerDesktopWidget = function(pluginName, name, handler){
    desktopWidgetHandlers[pluginName+'.'+name] = handler;
};
