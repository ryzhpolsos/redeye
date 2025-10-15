var windows = {};

plugin.registerTaskbarWidget('windowList', function(elem){
    redeye.registerWindowEventHook(function(event, wnd){
        var item = (event == 'create')?document.createElement('div'):(windows[wnd.handle]);

        switch(event){
            case 'create': {
                item.className = 'core-taskbar-item core-taskbar-item-' + redeye.config.shell.taskbar.position;

                var img = document.createElement('img');
                img.src = wnd.icon;
                img.className = 'core-taskbar-item-icon';
                img.ondragstart = function(e){
                    e.preventDefault();
                    return false;
                };
    
                var txt = document.createElement('span');
                txt.textContent = wnd.title;
                txt.className = 'core-taskbar-item-title';
                
                item.appendChild(img);
                item.appendChild(txt);
                if(wnd.isMinimized) item.classList.add('core-taskbar-item-minimized');
                if(wnd.isActive) item.classList.add('core-taskbar-item-active');
    
                item.addEventListener('click', function(){
                    if(wnd.isMinimized){
                        wnd.restore();
                    }else if(wnd.isActive){
                        wnd.minimize();
                    }else{
                        wnd.show();
                    }
                });

                windows[wnd.handle] = item;
                elem.appendChild(item);
                break;
            }
            case 'destroy': {
                wnd.remove();
                elem.removeChild(item);
                delete windows[wnd.handle];
                break;
            }
            case 'minimize': {
                item.classList.remove('core-taskbar-item-active');
                break;
            }
            case 'redraw': {
                item.querySelector('img').src = wnd.icon;
                item.querySelector('span').textContent = wnd.title;
                break;
            }
            case 'activate': {
                item.classList.add('core-taskbar-item-active');
                break;
            }
            case 'deactivate': {
                item.classList.remove('core-taskbar-item-active');
                break;
            }
            default: {
                console.log('We\'re into DEF with', event);
            }
        }
    });
});