using System;
using System.Text.RegularExpressions;

namespace ApiPorter.Patterns
{
    // TODO: new $type$<BoundExpression>()
    //
    // The type variable should match identifier in the generic name.
    // Of course, we still need to create matchers for generic name.
    // That's interesting because ArgumentVariable should be matched
    // for generic arguments.
    //
    // TODO: Statement (optional limit n...m statements)
    // TODO: MemberAccess

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

        public static ArgumentVariable Argument(string name, int minOccurrences = 0, int? maxOccurrences = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (minOccurrences < 0)
                throw new ArgumentOutOfRangeException(nameof(minOccurrences));

            if (maxOccurrences < 0)
                throw new ArgumentOutOfRangeException(nameof(maxOccurrences));

            return new ArgumentVariable(name, minOccurrences, maxOccurrences);
        }
    }
}