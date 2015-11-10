using System;
using System.Collections.Immutable;

namespace ApiPorter.Patterns
{
    public sealed partial class PatternSearch
    {
        private PatternSearch(string text, ImmutableArray<PatternVariable> variables)
        {
            Text = text;
            Variables = variables;
        }

        public static PatternSearch Create(string text, ImmutableArray<PatternVariable> variables)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return new PatternSearch(text, variables.ToImmutableArray());
        }

        public static PatternSearch Create(string text, params PatternVariable[] variables)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var variableArray = variables == null
                ? ImmutableArray<PatternVariable>.Empty
                : variables.ToImmutableArray();

            return Create(text, variableArray);
        }

        public string Text { get; }

        public ImmutableArray<PatternVariable> Variables { get; set; }
    }
}