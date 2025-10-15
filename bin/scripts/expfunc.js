window.exportedFunction = {
    list: {},
    register: function(name, func){
        this.list[name] = func;
    },
    invoke: function(name, args){
        this.list[name].apply(window, args || []);
    },
    remove: function(name){
        delete this.list[name];
    }
};