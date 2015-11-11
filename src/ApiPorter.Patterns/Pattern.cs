using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiPorter.Patterns
{
    internal sealed class Pattern
    {
        private readonly Dictionary<string, PatternVariable> _variableMap;

        private Pattern(SyntaxNode node, Dictionary<string, PatternVariable> variableMap)
        {
            Node = node;
            _variableMap = variableMap;
        }

        public static Pattern Create(string text, ImmutableArray<PatternVariable> variables)
        {
            var textBuilder = new StringBuilder(text);
            var variableMap = new Dictionary<string, PatternVariable>();

            foreach (var variable in variables)
            {
                var fakeIdentifier = "__" + Guid.NewGuid().ToString("N");
                textBuilder.Replace(variable.Name, fakeIdentifier);
                variableMap.Add(fakeIdentifier, variable);
            }

            // TODO: For now we only support expressions
            var expression = SyntaxFactory.ParseExpression(textBuilder.ToString());

            return new Pattern(expression, variableMap);
        }

        public SyntaxNode Node { get; }

        public bool TryGetVariable(SyntaxNodeOrToken nodeOrToken, out PatternVariable variable)
        {
            return nodeOrToken.IsToken
                ? TryGetVariable(nodeOrToken.AsToken(), out variable)
                : TryGetVariable(nodeOrToken.AsNode(), out variable);
        }

        private bool TryGetVariable(SyntaxNode node, out PatternVariable variable)
        {
            variable = null;

            var tokens = node.DescendantTokens().Take(2).ToImmutableArray();
            if (tokens.Length != 1)
                return false;

            var token = tokens[0];
            return TryGetVariable(token, out variable);
        }

        private bool TryGetVariable(SyntaxToken token, out PatternVariable variable)
        {
            variable = null;
            if (token.Kind() != SyntaxKind.IdentifierToken)
                return false;

            var variableName = token.ValueText;
            return _variableMap.TryGetValue(variableName, out variable);
        }
    }
}