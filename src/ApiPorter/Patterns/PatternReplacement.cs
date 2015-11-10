using System;

namespace ApiPorter.Patterns
{
    public sealed class PatternReplacement
    {
        private PatternReplacement(PatternSearch search, string newText)
        {
            Search = search;
            NewText = newText;
        }

        public static PatternReplacement Create(PatternSearch search, string newText)
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));

            if (newText == null)
                throw new ArgumentNullException(nameof(newText));

            return new PatternReplacement(search, newText);
        }

        public PatternSearch Search { get; set; }

        public string NewText { get; }
    }
}