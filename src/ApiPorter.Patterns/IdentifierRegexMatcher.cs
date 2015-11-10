using System;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiPorter.Patterns
{
    internal sealed class IdentifierRegexMatcher : Matcher
    {
        private readonly IdentifierPatternVariable _variable;

        public IdentifierRegexMatcher(IdentifierPatternVariable variable)
        {
            _variable = variable;
        }

        public override Match Execute(SyntaxNodeOrToken nodeOrToken)
        {
            if (!nodeOrToken.IsToken || nodeOrToken.Kind() != SyntaxKind.IdentifierToken)
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