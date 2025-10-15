window.addEventListener('DOMContentLoaded', function(){
    window.oncontextmenu = function(ev){
        if(ev.target.nodeName != 'INPUT' && ev.target.nodeName != 'TEXTAREA'){
            ev.preventDefault();
            return false;
        }
    }
});