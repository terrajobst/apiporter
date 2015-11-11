using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    partial class Matcher
    {
        public IEnumerable<Match> Run(SyntaxTree tree)
        {
            return tree.GetRoot()
                       .DescendantNodesAndTokensAndSelf()
                       .Select(CreateMatch)
                       .Where(n => n.IsMatch);
        }

        private Match CreateMatch(SyntaxNodeOrToken nodeOrToken)
        {
            var match = Run(nodeOrToken);
            return match.IsMatch ? match.WithSyntaxNodeOrToken(nodeOrToken) : Match.NoMatch;
        }
    }
}