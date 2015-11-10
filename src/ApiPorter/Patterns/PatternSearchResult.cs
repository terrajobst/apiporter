using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    public sealed class PatternSearchResult
    {
        private PatternSearchResult(PatternSearch search, Document document, SyntaxNodeOrToken nodeOrToken,  ImmutableArray<PatternCapture> captures)
        {
            Search = search;
            Document = document;
            NodeOrToken = nodeOrToken;
            Captures = captures;
        }

        public static PatternSearchResult Create(PatternSearch search, Document document, SyntaxNodeOrToken nodeOrToken, ImmutableArray<PatternCapture> captures)
        {
            if (search == null)
                throw new ArgumentNullException(nameof(search));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return new PatternSearchResult(search, document, nodeOrToken, captures);
        }

        public PatternSearch Search { get; }

        public Document Document { get; }
        public SyntaxNodeOrToken NodeOrToken { get; set; }

        public ImmutableArray<PatternCapture> Captures { get; }
    }
}