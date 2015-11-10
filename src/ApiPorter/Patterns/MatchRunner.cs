using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    internal static class MatchRunner
    {
        public static IEnumerable<Match> Run(SyntaxTree tree, Matcher matcher)
        {
            return tree.GetRoot()
                .DescendantNodesAndTokensAndSelf()
                .Select(n => CreateMatch(n, matcher))
                .Where(n => n.IsMatch);
        }

        private static Match CreateMatch(SyntaxNodeOrToken nodeOrToken, Matcher matcher)
        {
            var match = matcher.Execute(nodeOrToken);
            return match.IsMatch ? match.WithSyntaxNodeOrToken(nodeOrToken) : Match.NoMatch;
        }
    }
}