using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ApiPorter.Patterns
{
    partial class PatternReplacement
    {
        public static async Task<Solution> RunAsync(Solution solution, ImmutableArray<PatternReplacement> replacements)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            var projects = solution.Projects;
            var documents = await RunAsync(projects, replacements);

            var result = solution;
            foreach (var document in documents)
            {
                var text = await document.GetTextAsync();
                result = result.WithDocumentText(document.Id, text);
            }

            return result;
        }

        public static Task<ImmutableArray<Document>> RunAsync(IEnumerable<Project> projects, ImmutableArray<PatternReplacement> replacements)
        {
            if (projects == null)
                throw new ArgumentNullException(nameof(projects));

            var documents = projects.SelectMany(p => p.Documents);
            return RunAsync(documents, replacements);
        }

        public static async Task<ImmutableArray<Document>> RunAsync(IEnumerable<Document> documents, ImmutableArray<PatternReplacement> replacements)
        {
            if (documents == null)
                throw new ArgumentNullException(nameof(documents));

            var tasks = new List<Task<Document>>();
            foreach (var document in documents)
            {
                var task = Task.Run(() => RunAsync(document, replacements));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            return tasks.Select(t => t.Result).ToImmutableArray();
        }

        public static async Task<Document> RunAsync(Document document, ImmutableArray<PatternReplacement> replacements)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var searches = replacements.Select(r => r.Search).ToImmutableArray();
            var searchResults = await PatternSearch.RunAsync(document, searches);
            return await RunAsync(document, replacements, searchResults);
        }

        public static async Task<Document> RunAsync(Document document, ImmutableArray<PatternReplacement> replacements, ImmutableArray<PatternSearchResult> results)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var root = await document.GetSyntaxRootAsync();

            var replacer = new DocumentReplacer(replacements, results);
            var newRoot = replacer.Visit(root);

            return document.WithSyntaxRoot(newRoot);
        }

        private sealed class ReplacedCapture
        {
            public ReplacedCapture(PatternVariable variable, IEnumerable<SyntaxNodeOrToken> contents)
            {
                Variable = variable;
                Contents = contents.ToImmutableArray();
            }

            public PatternVariable Variable { get; }

            public ImmutableArray<SyntaxNodeOrToken> Contents { get; }
        }

        private sealed class DocumentReplacer : CSharpSyntaxRewriter
        {
            private readonly Dictionary<SyntaxNodeOrToken, PatternSearchResult> _results;
            private readonly Dictionary<PatternSearch, Pattern> _patterns;

            public DocumentReplacer(IEnumerable<PatternReplacement> replacements, IEnumerable<PatternSearchResult> results)
            {
                _patterns = replacements.ToDictionary(r => r.Search, r => Pattern.Create(r.NewText, r.Search.Variables));

                // NOTE: There is no gurantee that a node isn't matched by different searches.
                //       Ideally, we'd select the search that is more specific but such as
                //       a selection doens't seem to be well defined. For now, we simply select
                //       any result.

                _results = results.GroupBy(r => r.NodeOrToken)
                                  .ToDictionary(g => g.Key, g => g.First());
            }

            private static IEnumerable<SyntaxNodeOrToken> GetCaptureRange(PatternCapture capture)
            {
                var parent = capture.StartNodeOrToken.Parent;
                var children = parent.ChildNodesAndTokens();
                var start = capture.StartNodeOrToken.Span.Start;
                var end = capture.EndNodeOrToken.Span.End;
                var span = TextSpan.FromBounds(start, end);
                return children.Where(c => span.Contains(c.Span));
            }

            private SyntaxNodeOrToken VisitNodeOrToken(SyntaxNodeOrToken nodeOrToken)
            {
                if (nodeOrToken.IsNode)
                    return Visit(nodeOrToken.AsNode());

                return VisitToken(nodeOrToken.AsToken());
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                PatternSearchResult result;
                if (!_results.TryGetValue(node, out result))
                    return base.Visit(node);

                var pattern = _patterns[result.Search];
                var rewrittenCaptures = result.Captures.Select(VisitCapture)
                                                       .ToDictionary(c => c.Variable);

                var expander = new PatternExpander(pattern, rewrittenCaptures);
                var syntaxNode = expander.Visit(pattern.Node);
                return syntaxNode.WithLeadingTrivia(node.GetLeadingTrivia())
                                 .WithTrailingTrivia(node.GetTrailingTrivia());
            }

            private ReplacedCapture VisitCapture(PatternCapture capture)
            {
                var contents = GetCaptureRange(capture).Select(VisitNodeOrToken);
                return new ReplacedCapture(capture.Variable, contents);
            }
        }

        private sealed class PatternExpander : CSharpSyntaxRewriter
        {
            private readonly Pattern _pattern;
            private readonly Dictionary<PatternVariable, ReplacedCapture> _captures;

            public PatternExpander(Pattern pattern, Dictionary<PatternVariable, ReplacedCapture> captures)
            {
                _pattern = pattern;
                _captures = captures;
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                SyntaxToken result;
                if (TryExpandToken(token, out result))
                    return result;

                return base.VisitToken(token);
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == null)
                    return null;

                SyntaxNode result;
                if (TryExpandNode(node, out result))
                    return result;

                return base.Visit(node);
            }

            public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
            {
                List<SyntaxNodeOrToken> newArgumentList = null;

                var argumentsAndCommas = node.Arguments.GetWithSeparators();

                for (var i = 0; i < argumentsAndCommas.Count; i++)
                {
                    var argumentOrComma = argumentsAndCommas[i];
                    var argument = argumentOrComma.IsToken
                        ? null
                        : (ArgumentSyntax) argumentOrComma.AsNode();

                    ImmutableArray<SyntaxNodeOrToken> expandedNodes;
                    if (argument == null || !TryExpandNode(argument.Expression, out expandedNodes))
                    {
                        newArgumentList?.Add(argumentOrComma);
                    }
                    else
                    {
                        if (newArgumentList == null)
                        {
                            var preceeding = argumentsAndCommas.Take(i);
                            newArgumentList = new List<SyntaxNodeOrToken>(preceeding);
                        }

                        newArgumentList.AddRange(expandedNodes);
                    }
                }

                if (newArgumentList == null)
                    return node;

                var separatorArgumentList = SyntaxFactory.SeparatedList<ArgumentSyntax>(newArgumentList);
                return node.WithArguments(separatorArgumentList);
            }

            private bool TryExpandToken(SyntaxToken toBeEpanded, out SyntaxToken expanded)
            {
                expanded = default(SyntaxToken);

                PatternVariable variable;
                if (!_pattern.TryGetVariable(toBeEpanded, out variable))
                    return false;

                ReplacedCapture capture;
                if (!_captures.TryGetValue(variable, out capture))
                    return false;

                if (capture.Contents.Length != 1 || !capture.Contents[0].IsToken)
                    return false;

                expanded = capture.Contents.Single().AsToken();
                return true;
            }

            private bool TryExpandNode(SyntaxNode toBeExpanded, out SyntaxNode expanded)
            {
                expanded = null;

                PatternVariable variable;
                if (!_pattern.TryGetVariable(toBeExpanded, out variable))
                    return false;

                ReplacedCapture capture;
                if (!_captures.TryGetValue(variable, out capture))
                    return false;

                if (capture.Contents.Length != 1 || !capture.Contents[0].IsNode)
                    return false;

                expanded = capture.Contents.Single().AsNode();
                return true;
            }

            private bool TryExpandNode(SyntaxNode toBeExpanded, out ImmutableArray<SyntaxNodeOrToken> expandedNodes)
            {
                expandedNodes = ImmutableArray<SyntaxNodeOrToken>.Empty;

                PatternVariable variable;
                if (!_pattern.TryGetVariable(toBeExpanded, out variable))
                    return false;

                var argument = variable as ArgumentVariable;
                if (argument == null)
                    return false;

                ReplacedCapture capture;
                if (!_captures.TryGetValue(argument, out capture))
                    return argument.MinOccurrences == 0;

                expandedNodes = capture.Contents;
                return true;
            }
        }
    }
}