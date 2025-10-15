var args, startMenu, startMenuOpened = false;

function toggleStartMenu(){
    if(startMenuOpened){
        startMenu.getNetObject().Hide();
    }else{
        if(startMenu){
            startMenu.getNetObject().Show();
        }else{
            var taskbarBounds = redeye.getTaskbarBounds();
            var x = 0, y = 0;

            switch(redeye.config.shell.taskbar.position){
                case 'left': {
                    x = taskbarBounds.width;
                    break;
                }
                case 'right': {
                    x = screen.width - taskbarBounds.width - args.width;
                    break;
                }
                case 'top': {
                    y = taskbarBounds.height;
                    break;
                }
                case 'bottom': {
                    y = screen.height - taskbarBounds.height - args.height;
                    break;
                }
            }

            var htw = new HtmlWindow(x * devicePixelRatio, y * devicePixelRatio, args.width, args.height, redeye.file.read(redeye.file.getPath(args.page)), true);
            htw.setBorder('none');

            htw.show();
            startMenu = htw;
        }
    }

    startMenuOpened = !startMenuOpened;
}

plugin.registerTaskbarWidget('startButton', function(elem, aargs){
    args = aargs;

    var startButton = document.createElement('div');
    startButton.className = 'core-start-button';
    startButton.textContent = redeye.locale.gs('core.shell.startButtonText');
    startButton.onclick = toggleStartMenu;
    elem.appendChild(startButton);

    plugin.exportFunction('toggleStartMenu', toggleStartMenu);
});