function getTime(){
    let dt = new Date();
    return dt.getHours() + ':' + (dt.getMinutes().toString().length==1?'0':'') + dt.getMinutes();
}

plugin.registerTaskbarWidget('time', function(elem){
    elem.textContent = getTime();

    setInterval(function(){
        elem.textContent = getTime();
    }, 30000);
});
