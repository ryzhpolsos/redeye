using System.Drawing;
using System.Collections.Generic;

namespace PowerSearch {
    public interface ISuggestion {
        string GetText();
        Image GetIcon();
        void Invoke();
    }

    public static class SuggestionManager {
        public delegate IEnumerable<ISuggestion> SuggestionHandler(string input);

        List<SuggestionHandler> suggestions = new List<SuggestionHandler>();

        public void RegisterHandler(SuggestionHandler handler){
            suggestions.Add(handler);
        }

        public IEnumerable<ISuggestion> GetSuggestions(string input){
            var result = new List<ISuggestion>();
            foreach(var suggestion in suggestions) result.AddRange(suggestion.Invoke(input));
            return result;
        }
    }
}
