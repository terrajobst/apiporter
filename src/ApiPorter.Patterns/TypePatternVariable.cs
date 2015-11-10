using System;

namespace ApiPorter.Patterns
{
    public sealed class TypePatternVariable : PatternVariable
    {
        internal TypePatternVariable(string name, string typeName, bool allowDerivedTypes)
            : base(name)
        {
            TypeName = typeName;
            AllowDerivedTypes = allowDerivedTypes;
        }

        public override PatternVariableKind Kind => PatternVariableKind.Type;

        public string TypeName { get; }

        public bool AllowDerivedTypes { get; set; }
    }
}