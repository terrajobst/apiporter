using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    partial class Matcher
    {
        public static Matcher Create(SemanticModel semanticModel, PatternSearch search)
        {
            var pattern = Pattern.Create(search.Text, search.Variables);
            var builder = new MatcherBuilder(semanticModel, pattern);
            return builder.Create(pattern.Node);
        }

        private sealed class MatcherBuilder
        {
            private readonly SemanticModel _semanticModel;
            private readonly Pattern _pattern;

            public MatcherBuilder(SemanticModel semanticModel, Pattern pattern)
            {
                _semanticModel = semanticModel;
                _pattern = pattern;
            }

            public Matcher Create(SyntaxNodeOrToken nodeOrToken)
            {
                return nodeOrToken.IsToken
                    ? Create(nodeOrToken.AsToken())
                    : Create(nodeOrToken.AsNode());
            }

            private Matcher Create(SyntaxToken token)
            {
                Matcher matcher;
                if (TryCreateIdentifierPatternMatcher(token, out matcher))
                    return matcher;

                return new TokenMatcher(token.Kind(), token.ValueText);
            }

            private Matcher Create(SyntaxNode node)
            {
                Matcher matcher;

                if (TryCreateArgumentMatcher(node, out matcher))
                    return matcher;

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

            private bool TryGetVariable<T>(SyntaxNodeOrToken nodeOrToken, out T variable)
                where T : PatternVariable
            {
                variable = null;

                PatternVariable patternVariable;
                if (!_pattern.TryGetVariable(nodeOrToken, out patternVariable))
                    return false;

                variable = patternVariable as T;
                return variable != null;
            }

            private bool TryCreateIdentifierPatternMatcher(SyntaxToken token, out Matcher matcher)
            {
                matcher = null;

                IdentifierVariable variable;
                if (!TryGetVariable(token, out variable))
                    return false;

                matcher = new IdentifierRegexMatcher(variable);
                return true;
            }

            private bool TryCreateArgumentMatcher(SyntaxNode node, out Matcher matcher)
            {
                matcher = null;

                var argument = node as ArgumentSyntax;
                if (argument == null)
                    return false;

                ArgumentVariable variable;
                if (!TryGetVariable(argument.Expression, out variable))
                    return false;

                var argumentList = node.Parent as ArgumentListSyntax;
                if (argumentList == null)
                    return false;

                var index = argumentList.Arguments.IndexOf(argument);
                var following = argumentList.Arguments.Count - index - 1;

                matcher = new ArgumentMatcher(variable, following);
                return true;
            }

            private bool TryCreateExpressionMatcher(SyntaxNode node, out Matcher matcher)
            {
                matcher = null;

                var expression = node as ExpressionSyntax;
                if (expression == null)
                    return false;

                ExpressionVariable variable;
                if (!TryGetVariable(expression, out variable))
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

                TypeVariable variable;
                if (!TryGetVariable(expression, out variable))
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