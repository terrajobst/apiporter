using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal static class MatcherFactory
    {
        public static Matcher Create(SemanticModel semanticModel, PatternSearch search)
        {
            var textBuilder = new StringBuilder(search.Text);
            var variableMap = new Dictionary<string, PatternVariable>();

            foreach (var patternVariable in search.Variables)
            {
                var fakeIdentifier = "__" + Guid.NewGuid().ToString("N");
                textBuilder.Replace(patternVariable.Name, fakeIdentifier);
                variableMap.Add(fakeIdentifier, patternVariable);
            }
            
            // TODO: For now we only support expressions
            var expression = SyntaxFactory.ParseExpression(textBuilder.ToString());
            var builder = new MatcherBuilder(semanticModel, variableMap);
            var matcher = builder.Create(expression);
            return matcher;
        }

        private sealed class MatcherBuilder
        {
            private readonly SemanticModel _semanticModel;
            private readonly Dictionary<string, PatternVariable> _variables;

            public MatcherBuilder(SemanticModel semanticModel, Dictionary<string, PatternVariable> variables)
            {
                _semanticModel = semanticModel;
                _variables = variables;
            }

            public Matcher Create(SyntaxNodeOrToken nodeOrToken)
            {
                return nodeOrToken.IsToken
                    ? Create(nodeOrToken.AsToken())
                    : Create(nodeOrToken.AsNode());
            }

            private static Matcher Create(SyntaxToken token)
            {
                if (token.Kind() != SyntaxKind.IdentifierToken)
                    return new SyntaxKindMatcher(token.Kind(), ImmutableArray<Matcher>.Empty);

                return new IdentifierMatcher(token.ValueText);
            }

            private Matcher Create(SyntaxNode node)
            {
                Matcher matcher;
                if (TryCreateExpressionMatcher(node, out matcher))
                    return matcher;

                var children = node.ChildNodesAndTokens();
                var childMatchers = new List<Matcher>(children.Count);
                foreach (var child in children)
                {
                    var childMatcher = Create(child);
                    childMatchers.Add(childMatcher);
                }

                return new SyntaxKindMatcher(node.Kind(), childMatchers.ToImmutableArray());
            }

            private bool TryCreateExpressionMatcher(SyntaxNode node, out Matcher matcher)
            {
                matcher = null;

                var expression = node as ExpressionSyntax;
                if (expression == null)
                    return false;

                var tokens = expression.DescendantTokens().Take(2).ToImmutableArray();
                if (tokens.Length != 1)
                    return false;

                var token = tokens[0];
                if (token.Kind() != SyntaxKind.IdentifierToken)
                    return false;

                PatternVariable variable;
                if (!_variables.TryGetValue(token.ValueText, out variable))
                    return false;

                var typeSyntax = SyntaxFactory.ParseTypeName(variable.TypeName);
                var speculativeTypeInfo = _semanticModel.GetSpeculativeTypeInfo(0, typeSyntax, SpeculativeBindingOption.BindAsTypeOrNamespace);

                if (speculativeTypeInfo.Type == null)
                    return false;

                matcher = new ExpressionMatcher(variable, _semanticModel, speculativeTypeInfo.Type);
                return true;
            }
        }
    }
}