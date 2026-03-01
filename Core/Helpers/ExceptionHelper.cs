using System;

namespace RedEye.Core {
    public static class ExceptionHelper {
        public static string FormatException(Exception ex, bool debug = false){
            var result = $"{ex.GetType().FullName}: {ex.Message} " + (debug ? ex.StackTrace : "");
            if(ex.InnerException is not null) result += " | " + FormatException(ex.InnerException, debug);
            return result;
        }
    }
}
