using System;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    internal sealed class Match
    {
        private Match(SyntaxNodeOrToken nodeOrToken, bool isMatch, ImmutableArray<PatternCapture> captures)
        {
            NodeOrToken = nodeOrToken;
            IsMatch = isMatch;
            Captures = captures;
        }

        public static readonly Match Success = new Match(default(SyntaxNodeOrToken), true, ImmutableArray<PatternCapture>.Empty);

        public static readonly Match NoMatch = new Match(default(SyntaxNodeOrToken), false, ImmutableArray<PatternCapture>.Empty);

        public SyntaxNodeOrToken NodeOrToken { get; set; }

        public bool IsMatch { get; }

        public ImmutableArray<PatternCapture> Captures { get; }

        public Match AddCapture(PatternVariable variable, SyntaxNodeOrToken nodeOrToken)
        {
            var capture = PatternCapture.Create(variable, nodeOrToken);
            return AddCapture(capture);
        }

        public Match AddCapture(PatternCapture patternCapture)
        {
            Debug.Assert(IsMatch);
            return new Match(NodeOrToken, IsMatch, Captures.Add(patternCapture));
        }

        public Match AddCaptures(ImmutableArray<PatternCapture> captures)
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