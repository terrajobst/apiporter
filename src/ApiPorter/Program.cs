using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ApiPorter.Patterns;

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

            var searches = CreateSearches();
            var results = await PatternSearch.RunAsync(solution, searches);

            foreach (var result in results)
            {
                var text = await result.Document.GetTextAsync();
                var fileName = result.Document.FilePath;

                var syntaxNodeOrToken = result.NodeOrToken;
                var position = syntaxNodeOrToken.Span.Start;
                var lineNumber = text.Lines.GetLineFromPosition(position).LineNumber + 1;

                var contextNode = syntaxNodeOrToken.Parent
                    .AncestorsAndSelf()
                    .First(n => n is StatementSyntax || n is MemberDeclarationSyntax);

                Console.WriteLine(fileName + ":" + lineNumber);
                Console.WriteLine("\t" + contextNode.ToString().Trim());

                foreach (var capture in result.Captures)
                    Console.WriteLine("\t{0} = {1}", capture.Variable.Name, capture.NodeOrToken.ToString().Trim());
            }
        }

        private static ImmutableArray<PatternSearch> CreateSearches()
        {
            return ImmutableArray.Create(new[]
            {
                PatternSearch.Create("$type$.Assembly", new[]
                {
                    PatternVariable.Create("$type$", "System.Type")
                }),

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
