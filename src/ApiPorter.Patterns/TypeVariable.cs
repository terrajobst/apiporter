using System;

namespace ApiPorter.Patterns
{
    public sealed class TypeVariable : PatternVariable
    {
        internal TypeVariable(string name, string typeName, bool allowDerivedTypes)
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