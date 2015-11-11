using System;

namespace ApiPorter.Patterns
{
    public sealed class IdentifierVariable : PatternVariable
    {
        internal IdentifierVariable(string name, string regex, bool caseSensitive)
            : base(name)
        {
            Regex = regex;
            CaseSensitive = caseSensitive;
        }

        public override PatternVariableKind Kind => PatternVariableKind.Identifier;

        public string Regex { get; }

        public bool CaseSensitive { get; }
    }
}