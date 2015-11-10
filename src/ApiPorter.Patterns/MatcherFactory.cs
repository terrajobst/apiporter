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

            private Matcher Create(SyntaxToken token)
            {
                if (token.Kind() != SyntaxKind.IdentifierToken)
                    return new SyntaxKindMatcher(token.Kind(), ImmutableArray<Matcher>.Empty);

                Matcher matcher;
                if (TryCreateIdentifierPatternMatcher(token, out matcher))
                    return matcher;

                return new IdentifierMatcher(token.ValueText);
            }

            private Matcher Create(SyntaxNode node)
            {
                Matcher matcher;

                if (TryCreateExpressionMatcher(node, out matcher))
                    return matcher;

                if (TryCreateTypeMatcher(node, out matcher))
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

            private bool TryGetVariable<T>(string name, out T variable)
                where T: PatternVariable
            {
                variable = null;

                PatternVariable result;
                if (!_variables.TryGetValue(name, out result))
                    return false;

                variable = result as T;
                return variable != null;
            }

            private bool TryCreateIdentifierPatternMatcher(SyntaxToken token, out Matcher matcher)
            {
                matcher = null;

                if (token.Kind() != SyntaxKind.IdentifierToken)
                    return false;

                IdentifierPatternVariable variable;
                if (!TryGetVariable(token.ValueText, out variable))
                    return false;

                matcher = new IdentifierRegexMatcher(variable);
                return true;
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

                ExpressionPatternVariable variable;
                if (!TryGetVariable(token.ValueText, out variable))
                    return false;

                ITypeSymbol type;
                if (!TryParseType(variable.TypeName, out type))
                    return false;

                matcher = new ExpressionMatcher(variable, _semanticModel, type, variable.AllowDerivedTypes);
                return true;
            }

            private bool TryCreateTypeMatcher(SyntaxNode node, out Matcher matcher)
            {
                matcher = null;

                var expression = node as TypeSyntax;
                if (expression == null)
                    return false;

                var tokens = expression.DescendantTokens().Take(2).ToImmutableArray();
                if (tokens.Length != 1)
                    return false;

                var token = tokens[0];
                if (token.Kind() != SyntaxKind.IdentifierToken)
                    return false;

                TypePatternVariable variable;
                if (!TryGetVariable(token.ValueText, out variable))
                    return false;

                ITypeSymbol type;
                if (!TryParseType(variable.TypeName, out type))
                    return false;

                matcher = new TypeMatcher(variable, _semanticModel, type, variable.AllowDerivedTypes);
                return true;
            }

            private bool TryParseType(string typeName, out ITypeSymbol type)
            {
                type = null;
                if (string.IsNullOrEmpty(typeName))
                    return true;

                var typeSyntax = SyntaxFactory.ParseTypeName(typeName);
                var speculativeTypeInfo = _semanticModel.GetSpeculativeTypeInfo(0, typeSyntax, SpeculativeBindingOption.BindAsTypeOrNamespace);

                type = speculativeTypeInfo.Type;
                return type != null;
            }
        }
    }
}