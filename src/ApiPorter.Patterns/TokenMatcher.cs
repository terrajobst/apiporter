using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiPorter.Patterns
{
    internal sealed class TokenMatcher : Matcher
    {
        private readonly SyntaxKind _kind;
        private readonly string _text;

        public TokenMatcher(SyntaxKind kind, string text)
        {
            _kind = kind;
            _text = text;
        }

        public override Match Execute(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Kind() != _kind)
                return Match.NoMatch;

            return nodeOrToken.AsToken().ValueText == _text ? Match.Success : Match.NoMatch;
        }
    }
}