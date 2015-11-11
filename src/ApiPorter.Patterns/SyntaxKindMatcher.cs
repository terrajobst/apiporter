using System;
using System.Collections.Immutable;
using System.Linq;

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

        public override Match Run(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Kind() != _syntaxKind)
                return Match.NoMatch;

            var result = Match.Success;

            if (nodeOrToken.IsNode)
            {
                var node = nodeOrToken.AsNode();
                var children = node.ChildNodesAndTokens();

                var matcherIndex = 0;
                var matchedEnd = 0;

                for (var i = 0; i < children.Count; i++)
                {
                    if (matcherIndex >= _childMatchers.Length)
                        return Match.NoMatch;

                    while (i < children.Count && children[i].Span.Start < matchedEnd)
                        i++;

                    if (i >= children.Count)
                        break;

                    var child = children[i];
                    var matcher = _childMatchers[matcherIndex];
                    var match = matcher.Run(child);
                    if (!match.IsMatch)
                        return Match.NoMatch;

                    // Depending on much was captured, we need to skip some matchers.

                    if (match.Captures.Any())
                        matchedEnd = match.Captures.Max(c => c.EndNodeOrToken.Span.End);

                    result = result.AddCaptures(match.Captures);
                    matcherIndex++;
                }

                var allMatchersUsed = matcherIndex >= _childMatchers.Length;
                if (!allMatchersUsed)
                    return Match.NoMatch;
            }

            return result;
        }
    }
}