using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    partial class PatternSearch
    {
        public static Task<ImmutableArray<PatternSearchResult>> RunAsync(Solution solution, ImmutableArray<PatternSearch> searches)
        {
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            return RunAsync(solution.Projects, searches);
        }

        public static Task<ImmutableArray<PatternSearchResult>> RunAsync(IEnumerable<Project> projects, ImmutableArray<PatternSearch> searches)
        {
            if (projects == null)
                throw new ArgumentNullException(nameof(projects));

            var documents = projects.SelectMany(p => p.Documents);
            return RunAsync(documents, searches);
        }

        public static Task<ImmutableArray<PatternSearchResult>> RunAsync(Project project, ImmutableArray<PatternSearch> searches)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            var documents = project.Documents;
            return RunAsync(documents, searches);
        }

        public static async Task<ImmutableArray<PatternSearchResult>> RunAsync(IEnumerable<Document> documents, ImmutableArray<PatternSearch> searches)
        {
            if (documents == null)
                throw new ArgumentNullException(nameof(documents));

            var tasks = new List<Task<ImmutableArray<PatternSearchResult>>>();

            foreach (var document in documents)
            {
                var task = Task.Run(() => RunAsync(document, searches));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            return tasks.SelectMany(t => t.Result).ToImmutableArray();
        }

        public static async Task<ImmutableArray<PatternSearchResult>> RunAsync(Document document, ImmutableArray<PatternSearch> searches)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var semanticModel = await document.GetSemanticModelAsync();

            var tasks = new List<Task<ImmutableArray<PatternSearchResult>>>();

            foreach (var search in searches)
            {
                var task = Task.Run(() => RunAsync(document, semanticModel, search));
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            return tasks.SelectMany(t => t.Result).ToImmutableArray();
        }

        private static ImmutableArray<PatternSearchResult> RunAsync(Document document, SemanticModel semanticModel, PatternSearch search)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var matcher = MatcherFactory.Create(semanticModel, search);
            var matches = MatchRunner.Run(syntaxTree, matcher);
            var results = matches.Select(m => PatternSearchResult.Create(search, document, m.NodeOrToken, m.Captures));
            return results.ToImmutableArray();
        }
    }
}