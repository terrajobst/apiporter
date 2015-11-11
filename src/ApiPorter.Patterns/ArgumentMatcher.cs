using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal sealed class ArgumentMatcher : Matcher
    {
        private readonly ArgumentPatternVariable _variable;
        private readonly int _following;

        public ArgumentMatcher(ArgumentPatternVariable variable, int following)
        {
            _variable = variable;
            _following = following;
        }

        public override Match Execute(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Kind() != SyntaxKind.Argument)
                return Match.NoMatch;

            var argument = (ArgumentSyntax) nodeOrToken.AsNode();
            if (_variable.MinOccurrences == 1 && _variable.MaxOccurrences == 1)
                return Match.Success.WithSyntaxNodeOrToken(argument);

            var argumentList = argument.Parent as ArgumentListSyntax;
            if (argumentList == null)
                return Match.NoMatch;

            var currentIndex = argumentList.Arguments.IndexOf(argument);
            var availableCount = argumentList.Arguments.Count - currentIndex - _following;
            var captureCount = _variable.MaxOccurrences == null
                ? availableCount
                : Math.Min(availableCount, _variable.MaxOccurrences.Value);

            if (captureCount < _variable.MinOccurrences)
                return Match.NoMatch;

            var endIndex = currentIndex + captureCount - 1;
            var endArgument = argumentList.Arguments[endIndex];
            return Match.Success.AddCapture(_variable, argument, endArgument);
        }
    }
}