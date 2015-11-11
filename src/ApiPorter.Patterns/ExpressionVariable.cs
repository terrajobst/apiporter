using System;

namespace ApiPorter.Patterns
{
    public sealed class ExpressionVariable : PatternVariable
    {
        internal ExpressionVariable(string name, string typeName, bool allowDerivedTypes)
            : base(name)
        {
            TypeName = typeName;
            AllowDerivedTypes = allowDerivedTypes;
        }

        public override PatternVariableKind Kind => PatternVariableKind.Expression;

        public string TypeName { get; }

        public bool AllowDerivedTypes { get; set; }
    }
}