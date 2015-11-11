﻿using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ApiPorter.Patterns;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

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

                var nodeOrToken = result.NodeOrToken;
                var position = nodeOrToken.Span.Start;
                var lineNumber = text.Lines.GetLineFromPosition(position).LineNumber + 1;

                var contextNode = nodeOrToken.Parent
                    .AncestorsAndSelf()
                    .First(n => n is StatementSyntax || n is MemberDeclarationSyntax);

                Console.WriteLine(fileName + ":" + lineNumber);

                var nodeSpan = contextNode.Span;
                var matchSpan = nodeOrToken.Span;
                var prefixSpan = TextSpan.FromBounds(nodeSpan.Start, matchSpan.Start);
                var suffixSpan = TextSpan.FromBounds(matchSpan.End, nodeSpan.End);
                var prefixText = text.ToString(prefixSpan);
                var matchText = text.ToString(matchSpan);
                var suffixText = text.ToString(suffixSpan);

                Console.Write("\t");
                Console.Write(prefixText);
                var old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(matchText);
                Console.ForegroundColor = old;
                Console.Write(suffixText);
                Console.WriteLine();

                foreach (var capture in result.Captures)
                {
                    var start = capture.StartNodeOrToken.Span.Start;
                    var end = capture.EndNodeOrToken.Span.End;
                    var span = TextSpan.FromBounds(start, end);
                    var s = text.ToString(span);
                    Console.WriteLine("\t{0} = {1}", capture.Variable.Name, s);
                }
            }
        }

        private static ImmutableArray<PatternSearch> CreateSearches()
        {
            return ImmutableArray.Create(
                PatternSearch.Create("$type$.Create($args$, syntaxTree)",
                    PatternVariable.Expression("$type$"),
                    PatternVariable.Identifier("$identifier$"),
                    PatternVariable.Argument("$args$")
                ),
                PatternSearch.Create("$type$.Assembly",
                    PatternVariable.Expression("$type$", "System.Type")
                ),
                PatternSearch.Create("$type$.GetProperty($args$)",
                    PatternVariable.Expression("$type$", "System.Type"),
                    PatternVariable.Argument("$args$")
                ),
                PatternSearch.Create("Expression<$type$>.$identifier$",
                    PatternVariable.Type("$type$"),
                    PatternVariable.Identifier("$identifier$")
                ),
                PatternSearch.Create("$identifier$.Text",
                    PatternVariable.Identifier("$identifier$", "query|syntaxTree", true)
                )
            );
        }
    }
}
