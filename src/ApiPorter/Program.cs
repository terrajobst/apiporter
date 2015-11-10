using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ApiPorter.Patterns;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace ApiPorter
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                var applicationPath = Environment.GetCommandLineArgs()[0];
                var applicationName = Path.GetFileNameWithoutExtension(applicationPath);
                Console.Error.WriteLine("error: missing solution file");
                Console.Error.WriteLine("usage: {0} <solution>", applicationName);
                return -1;
            }

            try
            {
                var solutionPath = args.Single();
                RunAsync(solutionPath).Wait();
            }
            catch (AggregateException ex)
            {
                var aggregateException = ex.Flatten();
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    Console.Error.WriteLine(innerException);
                }
                return -1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }

            return 0;
        }

        private static async Task RunAsync(string solutionPath)
        {
            Console.WriteLine("Intialize workspace...");

            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionPath);

            Console.WriteLine("Searching...");

            var patternSearch = CreateSearchers();
            var documents = solution.Projects.SelectMany(p => p.Documents);
            var tasks = new List<Task<DocumentResults>>();

            foreach (var document in documents)
            {
                var task = Task.Run(() => ProcessDocumentAsync(document, patternSearch));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            foreach (var results in tasks.Select(t => t.Result))
            {
                var text = await results.Document.GetTextAsync();
                var fileName = results.Document.FilePath;
                foreach (var match in results.Matches)
                {
                    var syntaxNodeOrToken = match.NodeOrToken;
                    var position = syntaxNodeOrToken.Span.Start;
                    var lineNumber = text.Lines.GetLineFromPosition(position).LineNumber + 1;

                    var contextNode = syntaxNodeOrToken.Parent
                        .AncestorsAndSelf()
                        .First(n => n is StatementSyntax || n is MemberDeclarationSyntax);

                    Console.WriteLine(fileName + ":" + lineNumber);
                    Console.WriteLine("\t" + contextNode.ToString().Trim());

                    foreach (var capture in match.Captures)
                        Console.WriteLine("\t{0} = {1}", capture.Variable.Name, capture.NodeOrToken.ToString().Trim());
                }
            }
        }

        private static async Task<DocumentResults> ProcessDocumentAsync(Document document, ImmutableArray<PatternSearch> searches)
        {
            var semanticModel = await document.GetSemanticModelAsync();

            var tasks = new List<Task<ImmutableArray<Match>>>();

            foreach (var search in searches)
            {
                var task = Task.Run(() => RunSearchPatternAsync(semanticModel, search));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            return new DocumentResults(document, tasks.SelectMany(t => t.Result).ToImmutableArray());
        }

        private static ImmutableArray<Match> RunSearchPatternAsync(SemanticModel semanticModel, PatternSearch search)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var matcher = MatcherFactory.Create(semanticModel, search);
            var matches = MatchRunner.Run(syntaxTree, matcher).ToImmutableArray();
            return matches;
        }

        private sealed class DocumentResults
        {
            public DocumentResults(Document document, ImmutableArray<Match> matches)
            {
                Document = document;
                Matches = matches;
            }

            public Document Document { get; }

            public ImmutableArray<Match> Matches { get; }
        }

        private static ImmutableArray<PatternSearch> CreateSearchers()
        {
            return ImmutableArray.Create(new[]
            {
                //PatternSearch.Create("DataContext.Empty", Enumerable.Empty<PatternVariable>()),

                //PatternSearch.Create("$type$.Assembly", new[]
                //{
                //    PatternVariable.Create("$type$", "System.Type")
                //}),

                PatternSearch.Create("$type$.GetProperty($name$, $args$)", new[]
                {
                    PatternVariable.Create("$type$", "System.Type"),
                    PatternVariable.Create("$name$", "System.String"),
                    PatternVariable.Create("$args$", "System.Type[]")
                })
            });
        }
    }
}
