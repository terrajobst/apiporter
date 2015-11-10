using System;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    internal sealed class Match
    {
        private Match(SyntaxNodeOrToken nodeOrToken, bool isMatch, ImmutableArray<Capture> captures)
        {
            NodeOrToken = nodeOrToken;
            IsMatch = isMatch;
            Captures = captures;
        }

        public static readonly Match Success = new Match(default(SyntaxNodeOrToken), true, ImmutableArray<Capture>.Empty);

        public static readonly Match NoMatch = new Match(default(SyntaxNodeOrToken), false, ImmutableArray<Capture>.Empty);

        public SyntaxNodeOrToken NodeOrToken { get; set; }

        public bool IsMatch { get; }

        public ImmutableArray<Capture> Captures { get; }

        public Match AddCapture(Capture capture)
        {
            Debug.Assert(IsMatch);
            return new Match(NodeOrToken, IsMatch, Captures.Add(capture));
        }

        public Match AddCaptures(ImmutableArray<Capture> captures)
        {
            Debug.Assert(IsMatch);
            return new Match(NodeOrToken, IsMatch, Captures.AddRange(captures));
        }

        public Match WithSyntaxNodeOrToken(SyntaxNodeOrToken syntaxNodeOrToken)
        {
            Debug.Assert(IsMatch);
            return new Match(syntaxNodeOrToken, IsMatch, Captures);
        }
    }
}