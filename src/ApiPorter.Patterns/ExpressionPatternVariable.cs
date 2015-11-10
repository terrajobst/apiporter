using System;

namespace ApiPorter.Patterns
{
    public sealed class ExpressionPatternVariable : PatternVariable
    {
        internal ExpressionPatternVariable(string name, string typeName, bool allowDerivedTypes)
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