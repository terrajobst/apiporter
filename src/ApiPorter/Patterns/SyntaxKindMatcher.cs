using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiPorter.Patterns
{
    internal sealed class SyntaxKindMatcher : Matcher
    {
        private readonly SyntaxKind _syntaxKind;
        private readonly ImmutableArray<Matcher> _childMatchers;

        public SyntaxKindMatcher(SyntaxKind syntaxKind, ImmutableArray<Matcher> childMatchers)
        {
            _syntaxKind = syntaxKind;
            _childMatchers = childMatchers;
        }

        public override Match Execute(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Kind() != _syntaxKind)
                return Match.NoMatch;

            var result = Match.Success;

            if (nodeOrToken.IsNode)
            {
                var node = nodeOrToken.AsNode();
                var children = node.ChildNodesAndTokens();
                if (children.Count != _childMatchers.Length)
                    return Match.NoMatch;

                for (var i = 0; i < children.Count; i++)
                {
                    var match = _childMatchers[i].Execute(children[i]);
                    if (!match.IsMatch)
                        return Match.NoMatch;

                    result = result.AddCaptures(match.Captures);
                }
            }

            return result;
        }
    }
}