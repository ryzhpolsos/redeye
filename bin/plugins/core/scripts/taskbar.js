function setTaskbarBounds(clientRect){
    var left, top, width, height;

    switch(redeye.config.shell.taskbar.position){
        case 'left': {
            left = (clientRect.left + clientRect.width);
            top = 0;
            width = (screen.width - (clientRect.left + clientRect.width));
            height = screen.height;
            break;
        }
        case 'right': {
            left = 0;
            top = 0;
            width = (window.outerWidth - clientRect.width);
            height = screen.height;
            break;
        }
        case 'top': {
            left = 0;
            top = (clientRect.top + clientRect.height);
            width = screen.width;
            height = (screen.height - (clientRect.top + clientRect.height));
            break;
        }
        case 'bottom': {
            left = 0;
            top = 0;
            width = screen.width;
            height = (screen.height - clientRect.height);
            break;
        }
    }

    redeye.setDesktopBounds(left, top, width, height);
    redeye.setTaskbarBounds(clientRect.left, clientRect.top, clientRect.width, clientRect.height);
}

plugin.registerTaskbar(function(){
    var taskbar = document.createElement('div');
    taskbar.id = 'taskbar';
    taskbar.className = 'core-taskbar core-taskbar-' + redeye.config.shell.taskbar.position;
    document.body.appendChild(taskbar);

    var taskbarRect = taskbar.getBoundingClientRect();
    setTaskbarBounds(taskbarRect);

    var isTaskbarVertical = redeye.config.shell.taskbar.position != 'top' && redeye.config.shell.taskbar.position != 'bottom';

    var offset = 0;
    for(var i in redeye.config.shell.taskbar.widgets){
        var widget = redeye.config.shell.taskbar.widgets[i];

        var elem = document.createElement('div');
        elem.className = 'core-taskbar-widget core-taskbar-widget-' + redeye.config.shell.taskbar.position;
        redeye.taskbarWidgetHandlers[widget.name](elem, widget.args);

        elem.style.position = 'absolute';
        elem.style[isTaskbarVertical?'top':'left'] = (offset += widget.offset) + 'px';
        if(isTaskbarVertical){
            elem.style.width = taskbarRect.width + 'px';
            elem.style.height = (typeof widget.height == 'number')?widget.height+'px':widget.height;
        }else{
            elem.style.width = (typeof widget.width == 'number')?widget.width+'px':widget.width;
            elem.style.height = taskbarRect.height + 'px';
        }

        taskbar.appendChild(elem);
        offset += elem.getBoundingClientRect()[isTaskbarVertical?'height':'width'];
    }
});
