using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal sealed class EmptyArgumentListMatcher : Matcher
    {
        public override Match Run(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Kind() != SyntaxKind.ArgumentList)
                return Match.NoMatch;

            var argumentList = (ArgumentListSyntax) nodeOrToken.AsNode();
            if (argumentList.Arguments.Count != 0)
                return Match.NoMatch;

            return Match.Success;
        }
    }
}