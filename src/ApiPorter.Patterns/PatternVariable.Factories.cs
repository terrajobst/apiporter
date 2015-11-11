using System;
using System.Text.RegularExpressions;

namespace ApiPorter.Patterns
{
    // Statement (optional limit n...m statements)
    // MemberAccess

    partial class PatternVariable
    {
        public static ExpressionVariable Expression(string name, string typeName = null, bool allowDerivedTypes = true)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new ExpressionVariable(name, typeName, allowDerivedTypes);
        }

        public static IdentifierVariable Identifier(string name, string regex = null, bool caseSensitive = false)
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

            return new IdentifierVariable(name, regex, caseSensitive);
        }

        public static TypeVariable Type(string name, string typeName = null, bool allowDerivedTypes = true)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new TypeVariable(name, typeName, allowDerivedTypes);
        }

        public static ArgumentVariable Argument(string name, int minOccurrences = 1, int? maxOccurrences = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return new ArgumentVariable(name, minOccurrences, maxOccurrences);
        }
    }
}