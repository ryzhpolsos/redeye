plugin.registerTaskbarWidget('layout', function(elem){
    elem.textContent = redeye.keyboardLayout.getCurrent();

    redeye.keyboardLayout.registerHandler(function(langName){
        elem.textContent = langName;
    });
});