using System;

namespace ApiPorter.Patterns
{
    // Argument (optional limit, n...m args)
    // Expression (optional: of type X, exactly this type)
    // Identifier (optional regex, case sensitive)
    // Statement (optional limit n...m statements)
    // Type (optional of type X, exactly this type)
    // ---
    // Any member

    public sealed class PatternVariable
    {
        private PatternVariable(string name, string typeName)
        {
            Name = name;
            TypeName = typeName;
        }

        public static PatternVariable Create(string name, string typeName)
        {
            return new PatternVariable(name, typeName);
        }

        public string Name { get; }

        public string TypeName { get; }
    }
}