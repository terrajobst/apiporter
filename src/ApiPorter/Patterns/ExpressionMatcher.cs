using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal sealed class ExpressionMatcher : Matcher
    {
        private readonly PatternVariable _variable;
        private readonly SemanticModel _semanticModel;
        private readonly ITypeSymbol _type;

        public ExpressionMatcher(PatternVariable variable, SemanticModel semanticModel, ITypeSymbol type)
        {
            _variable = variable;
            _semanticModel = semanticModel;
            _type = type;
        }

        public override Match Execute(SyntaxNodeOrToken nodeOrToken)
        {
            if (!nodeOrToken.IsNode)
                return Match.NoMatch;

            var expressionSyntax = nodeOrToken.AsNode() as ExpressionSyntax;
            if (expressionSyntax == null)
                return Match.NoMatch;

            var typeInfo = _semanticModel.GetTypeInfo(expressionSyntax);
            if (typeInfo.Type == null || !typeInfo.Type.Equals(_type))
                return Match.NoMatch;

            var capture = PatternCapture.Create(_variable, nodeOrToken);
            return Match.Success.AddCapture(capture);
        }
    }
}