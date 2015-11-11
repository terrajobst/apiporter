using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal sealed class ArgumentListMatcher : Matcher
    {
        private readonly ArgumentVariable _variable;

        public ArgumentListMatcher(ArgumentVariable variable)
        {
            _variable = variable;
        }

        public override Match Run(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Kind() != SyntaxKind.ArgumentList)
                return Match.NoMatch;

            var argumentList = (ArgumentListSyntax) nodeOrToken.AsNode();
            if (_variable.MaxOccurrences != null && _variable.MaxOccurrences < argumentList.Arguments.Count)
                return Match.NoMatch;

            if (argumentList.Arguments.Count == 0)
                return Match.Success;

            var first = argumentList.Arguments.First();
            var last = argumentList.Arguments.Last();
            return Match.Success.AddCapture(_variable, first, last);
        }
    }
}