using System;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiPorter.Patterns
{
    internal sealed class IdentifierRegexMatcher : Matcher
    {
        private readonly IdentifierVariable _variable;

        public IdentifierRegexMatcher(IdentifierVariable variable)
        {
            _variable = variable;
        }

        public override Match Run(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Kind() != SyntaxKind.IdentifierToken)
                return Match.NoMatch;

            var identifier = nodeOrToken.AsToken().ValueText;
            var regex = _variable.Regex;
            if (!string.IsNullOrEmpty(regex))
            {
                var regexOptions = _variable.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                if (!Regex.IsMatch(identifier, regex, regexOptions))
                    return Match.NoMatch;
            }

            return Match.Success.AddCapture(_variable, nodeOrToken);
        }
    }
}