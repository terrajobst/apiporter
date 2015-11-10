using System;

namespace ApiPorter.Patterns
{
    public abstract partial class PatternVariable
    {
        internal PatternVariable(string name)
        {
            Name = name;
        }

        public abstract PatternVariableKind Kind { get; }

        public string Name { get; }
    }
}