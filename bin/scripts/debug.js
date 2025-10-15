window._log = function(type, str){
    console.log('[' + type + '] ' + str);
};

window.onerror = function(e){
    rwExternal.LogError("JavaScript Error: " + e);
};