using System;
using System.Text.RegularExpressions;

namespace ApiPorter.Patterns
{
    // Argument (optional limit, n...m args)
    // Statement (optional limit n...m statements)
    // MemberAccess

    partial class PatternVariable
    {
        public static ExpressionPatternVariable Expression(string name, string typeName = null, bool allowDerivedTypes = true)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new ExpressionPatternVariable(name, typeName, allowDerivedTypes);
        }

        public static IdentifierPatternVariable Identifier(string name, string regex = null, bool caseSensitive = false)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!string.IsNullOrEmpty(regex))
            {
                try
                {
                    new Regex(regex);
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Regex is invalid", nameof(regex));
                }
            }

            return new IdentifierPatternVariable(name, regex, caseSensitive);
        }

        public static TypePatternVariable Type(string name, string typeName = null, bool allowDerivedTypes = true)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new TypePatternVariable(name, typeName, allowDerivedTypes);
        }
    }
}