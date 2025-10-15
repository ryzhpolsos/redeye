var _kbStack = [];
var _lButtonDown = false;
var _rButtonDown = false;

var _keyAliases = {
    'Menu': 'Alt',
    'ControlKey': 'Ctrl',
    'ShiftKey': 'Shift',
    'LMenu': 'LeftAlt',
    'RMenu': 'RightAlt',
    'LControlKey': 'LeftCtrl',
    'RControlKey': 'RightCtrl',
    'LShiftKey': 'LeftShift',
    'RShiftKey': 'RightShift',
    'LWin': 'LeftWin',
    'RWin': 'RightWin',
    'Return': 'Enter'
};

var _inputHooks = {
    keydown: [],
    keyup: [],
    keypress: [],
    mousemove: [],
    mousedown: [],
    mouseup: [],
    click: []
};

var _ignoreKeys = [];

for(var i in globalConfig.keyBinds){
    (function(bind){
        _inputHooks.keypress.push({
            param: bind.keys,
            listener: function(){
                exportedFunction.invoke(bind.function);
            }
        });
    })(globalConfig.keyBinds[i]);
}

window._handleKbEvent = function(state, key){
    if(_ignoreKeys.indexOf(key) != -1) return;

    // Lkey, Rkey -> key
    if((key[0] == 'L' || key[0] == 'R') && key.length > 1 && key != 'Return'){
        _handleKbEvent(state, key.slice(1));
        return;
    }

    // D0, D1, ... D9 -> 0, 1, ... 9
    if(key.length == 2 && key[0] == 'D'){
        key = key[1];
    }

    if(_keyAliases[key]){
        key = _keyAliases[key];
    }

    if(state == 'down'){
        var index = _kbStack.indexOf(key);
        if(index == -1) _kbStack.push(key);

        for(var i in _inputHooks.keydown){
            var hk = _inputHooks.keydown[i];
            if(!hk.param || hk.param == key) hk.listener(key);
        }
    }else{
        _kbStack.sort();

        for(var i in _inputHooks.keypress){
            var hk = _inputHooks.keypress[i];

            if(!hk.sorted){
                hk.param.sort();
                hk.sorted = true;
            }

            if(hk.param.length != _kbStack.length) continue;

            var callHook = true;
            for(var j in hk.param){
                if(hk.param[j] != _kbStack[j]){
                    callHook = false;
                    break;
                }
            }

            //if(callHook) console.log('calling hook for', hk.param);
            if(callHook){
                hk.listener();
                _kbStack = [];
            }
        }

        var index = _kbStack.indexOf(key);
        if(index != -1) _kbStack.splice(index, 1);

        for(var i in _inputHooks.keyup){
            var hk = _inputHooks.keyup[i];
            if(!hk.param || hk.param == key) hk.listener(key);
        }
    }
}

window._handleMsEvent = function(state, x, y){
    //console.log(state, x, y);
}