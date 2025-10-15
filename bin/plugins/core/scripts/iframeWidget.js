plugin.registerDesktopWidget('iframe', function(elem, args){
    var iframe = document.createElement('iframe');
    iframe.style.width = '100%';
    iframe.style.height = '100%';
    iframe.style.border = 'none';
    iframe.src = args.url || ('data:'+(args.mimeType||'text/html')+';,'+(args.file?redeye.file.read(redeye.file.getPath(args.file)):args.content));

    elem.appendChild(iframe);
});
