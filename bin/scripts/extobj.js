try{
    window.rwExternal = ((!globalConfig.core.useEdgeRuntime)?window.external:window.chrome.webview.hostObjects.sync.scriptManager);
}catch(e){
    window.rwExternal = window.external;
}

var objectGuidHeader = rwExternal.GetObjectGuidHeader();

window.ExternalObject = function(objectType, typeName, constArgs){
    if(typeof objectType == 'number'){
        this._htw_guid = rwExternal.CreateObject(objectType, typeName, JSON.stringify(constArgs || []));
    }else{
        this._htw_guid = objectType;
    }

    var props = rwExternal.GetObjectMembers(0, this._htw_guid).split(';');
    var methods = rwExternal.GetObjectMembers(1, this._htw_guid).split(';');

    for(var i in props){

        (function(tho, prop){
            tho.__defineGetter__(prop, function(){
                if(tho['get_'+prop]) return tho['get_'+prop]();
                var result = rwExternal.InvokeObjectMember(0, tho._htw_guid, prop, '[]');
                if(typeof result == 'string' && result.indexOf(objectGuidHeader) == 0) result = new ExternalObject(result.replace(objectGuidHeader, ''));
                return result;
            });

            tho.__defineSetter__(prop, function(value){
                if(tho['set_'+prop]) return tho['set_'+prop](value);
                if(value._htw_guid) value = objectGuidHeader + value._htw_guid;
                rwExternal.InvokeObjectMember(0, tho._htw_guid, prop, JSON.stringify([value]));
            });
        })(this, props[i]);
    }

    for(var i in methods){
        (function(tho, method){
            tho[method] = function(){
                var args = Array.prototype.slice.call(arguments);

                for(var j in args){
                    if(args[j]._htw_guid) args[j] = objectGuidHeader + args[j]._htw_guid;
                }

                var result = rwExternal.InvokeObjectMember(2, this._htw_guid, method, JSON.stringify(args));
                if(typeof result == 'string' && result.indexOf(objectGuidHeader) == 0) result = new ExternalObject(result.replace(objectGuidHeader, ''));
                return result;
            }
        })(this, methods[i]);
    }
}

var _netCache = {};

window.NetObject = function(objectName, constArgs){
    return net.createObject(objectName, constArgs);
}

window.NetClass = function(objectName){
    return net.getClass(objectName);
}

window.net = {
    createObject: function(objectName, constArgs){
        if(!constArgs) constArgs = [];
        return new ExternalObject(0, objectName, constArgs);
    },
    getClass: function(objectName){
        if(_netCache[objectName]) return _netCache[objectName];
        var obj = new ExternalObject(1, objectName, []);
        _netCache[objectName] = obj;
        return obj;
    }
};

window.com = {
    createObject: function(objectName){
        return new ExternalObject(2, objectName, []);
    },
    getObject: function(objectName){
        return new ExternalObject(3, objectName, []);
    }
};

window.formObject = new ExternalObject(rwExternal.GetFormObjectGuid());

window.DllCall = function(libName, funcName, retType, argTypes){
    libName = libName.toLowerCase();

    return function(){
        // if((!args || args.length == 0) && rwExternal['DllCall_' + libName + '_' + funcName]){
        //     return rwExternal['DllCall_' + libName + '_' + funcName].apply(null, arguments);
        // }

        var args = [];

        for(var i in argTypes){
            args.push(argTypes[i]);
            args.push(arguments[i]);
        }

        return rwExternal.DllCall(libName, funcName, retType, JSON.stringify(args));
    }
};
