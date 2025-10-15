window.findIndex = function(list, callback) {
    for(var i = 0; i < list.length; i++){
        if(callback(list[i], i, list)) return i;
    }
    return -1;
};

window.find = function(list, callback) {
    for(var i = 0; i < list.length; i++){
        if(callback(list[i], i, list)) return list[i];
    }
    return -1;
};

window.objectAssign = function(target, source){
    for(var i in source){
        target[i] = source[i];
    }
};

if(!window.console){
    window.console = {
        log: function(){}
    };
}