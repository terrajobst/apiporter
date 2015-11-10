using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiPorter.Patterns
{
    internal sealed class IdentifierMatcher : Matcher
    {
        private readonly string _text;

        public IdentifierMatcher(string text)
        {
            _text = text;
        }

        public override Match Execute(SyntaxNodeOrToken nodeOrToken)
        {
            if (!nodeOrToken.IsToken || nodeOrToken.Kind() != SyntaxKind.IdentifierToken)
                return Match.NoMatch;

            return nodeOrToken.AsToken().ValueText == _text ? Match.Success : Match.NoMatch;
        }
    }
}