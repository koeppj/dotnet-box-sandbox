
namespace BoxCli
{
    public partial class TypeaheadCommandReader : IAutoCompleteHandler
    {
        private readonly BoxItemFetcher _boxItemFetcher;

        public TypeaheadCommandReader(BoxItemFetcher boxItemFetcher)
        {
            _boxItemFetcher = boxItemFetcher;
            ReadLine.AutoCompletionHandler = this;
        }

        public char[] Separators { get; set; } = new[] { ' ' };

        public async Task<string[]> ReadCommandAsync()
        {
            string input = await Task.Run(() => ReadLine.Read());
            ReadLine.AddHistory(input);
            return Utils.ParseWords(input);
        }

        public string[] GetSuggestions(string text, int index)
        {
            // Only evaluate the portion after the last space or quote
            int lastSpaceIndex = text.LastIndexOf(' ');
            int lastSingleQuoteIndex = text.LastIndexOf('\'');
            int lastDoubleQuoteIndex = text.LastIndexOf('"');
            int lastArgStart = Math.Max(lastSpaceIndex, Math.Max(lastSingleQuoteIndex, lastDoubleQuoteIndex));
            string evalText = lastArgStart >= 0 ? text.Substring(lastArgStart + 1) : text;

            // Only suggest for non-command words
            if (string.IsNullOrWhiteSpace(evalText) || evalText.StartsWith(".") || evalText.StartsWith("/") || evalText.StartsWith("-"))
                return Array.Empty<string>();

            // Determine if the argument started with a quote
            char? quoteChar = null;
            if (lastArgStart == lastSingleQuoteIndex)
                quoteChar = '\'';
            else if (lastArgStart == lastDoubleQuoteIndex)
                quoteChar = '"';

            var match = _boxItemFetcher.GetItemByPartialName(evalText);
            string suggestion = match.FirstMatch != null ? match.FirstMatch.Name : evalText;
            if (quoteChar.HasValue)
                return new[] { quoteChar.Value + suggestion };
            return new[] { suggestion };
        }

    }
}