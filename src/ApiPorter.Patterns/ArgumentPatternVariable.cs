using System;

namespace ApiPorter.Patterns
{
    public sealed class ArgumentPatternVariable : PatternVariable
    {
        internal ArgumentPatternVariable(string name, int minOccurrences, int? maxOccurrences)
            : base(name)
        {
            MinOccurrences = minOccurrences;
            MaxOccurrences = maxOccurrences;
        }

        public override PatternVariableKind Kind => PatternVariableKind.Argument;

        public int MinOccurrences { get; }

        public int? MaxOccurrences { get; }
    }
}