plugin.exportFunction('runBox', function(){
    var content = plugin.readFile('res\\runBox.html');

    var width = Math.round(300 * devicePixelRatio), height = Math.round(100 * devicePixelRatio);
    var wnd = new HtmlWindow(Math.floor(screen.width / 2 - width / 2), Math.floor(screen.height / 2 - height / 2), width, height, content);
    wnd.setTitle(redeye.locale.gs('core.shell.runBoxTitle'));
    wnd.setBorder('fixeddialog');
    wnd.configButtons(false, false, true);

    wnd.show();
});