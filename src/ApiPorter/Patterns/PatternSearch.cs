using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ApiPorter.Patterns
{
    public sealed class PatternSearch
    {
        private PatternSearch(string text, ImmutableArray<PatternVariable> variables)
        {
            Text = text;
            Variables = variables;
        }

        public static PatternSearch Create(string text, IEnumerable<PatternVariable> variables)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (variables == null)
                throw new ArgumentNullException(nameof(variables));

            return new PatternSearch(text, variables.ToImmutableArray());
        }

        public string Text { get; }

        public ImmutableArray<PatternVariable> Variables { get; set; }
    }
}