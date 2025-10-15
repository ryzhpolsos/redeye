window.HTW_AUTO = -1;
window.HtmlWindow = function(x, y, width, height, content, isShell){
    this._handlerId = Math.random().toString().replace('.', '');

    var eventMap = {};
    window['_HtmlWindow_'+this._handlerId] = function(selector, evName, eoStr){
        eventMap[evName][selector](JSON.parse(eoStr));
    }

    var tho = this;
    window['_HtmlWindow_loaded_'+this._handlerId] = function(){
        if(tho.onload) tho.onload();
    };

    this._wobj = new NetObject('RedEye.HtmlWindow', [x || 0, y || 0, width || 500, height || 400, content || '', this._handlerId, isShell || false]);

    this.invoke = function(code){
        this._wobj.sendEvent('invoke', JSON.stringify({ code: code }));
    };

    this.setProperty = function(selector, name, value){
        this._wobj.sendEvent('setprop', JSON.stringify({ selector: selector, name: name, value: value }));
    };

    this.addEvent = function(selector, type, callback){
        if(!eventMap[type]) eventMap[type] = {};
        eventMap[type][selector] = callback;
        this._wobj.sendEvent('addevent', JSON.stringify({ selector: selector, event: type }));
    };

    this.show = function(){
        this._wobj.Show();
    };

    this.setTitle = function(title){
        this._wobj.Text = title;
    };

    this.configButtons = function(minBtn, maxBtn, clsBtn){
        if(minBtn !== null) this._wobj.MinimizeBox = minBtn;
        if(maxBtn !== null) this._wobj.MaximizeBox = maxBtn;
        if(clsBtn !== null) this._wobj.ControlBox = clsBtn;
    };

    this.setBorder = function(border){
        var map = ['none', 'fixedsingle', 'fixed3d', 'fixeddialog', 'sizable', 'fixedtoolwindow', 'sizabletoolwindow'];
        this._wobj.setBorder(map.indexOf(border.toLowerCase()));
    };

    this.close = function(){
        this._wobj.Close();
    };

    this.getNetObject = function(){
        return this._wobj;
    };
}