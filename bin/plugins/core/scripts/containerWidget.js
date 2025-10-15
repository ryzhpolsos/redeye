plugin.registerDesktopWidget('container', function(elem, args){
    elem.innerHTML = args.file?redeye.file.read(redeye.file.getPath(args.file)):args.content;
});