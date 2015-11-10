using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal abstract class ExpressionMatcher<T> : Matcher
        where T: ExpressionSyntax
    {
        private readonly PatternVariable _variable;
        private readonly SemanticModel _semanticModel;
        private readonly ITypeSymbol _type;
        private readonly bool _allowDerivedTypes;

        protected ExpressionMatcher(PatternVariable variable, SemanticModel semanticModel, ITypeSymbol type, bool allowDerivedTypes)
        {
            _variable = variable;
            _semanticModel = semanticModel;
            _type = type;
            _allowDerivedTypes = allowDerivedTypes;
        }

        public override Match Execute(SyntaxNodeOrToken nodeOrToken)
        {
            if (!nodeOrToken.IsNode)
                return Match.NoMatch;

            var expressionSyntax = nodeOrToken.AsNode() as T;
            if (expressionSyntax == null)
                return Match.NoMatch;

            if (_type != null)
            {
                var typeInfo = _semanticModel.GetTypeInfo(expressionSyntax);
                if (typeInfo.Type == null)
                    return Match.NoMatch;

                if (!IsMatch(typeInfo.Type))
                    return Match.NoMatch;
            }

            return Match.Success.AddCapture(_variable, nodeOrToken);
        }

        private bool IsMatch(ITypeSymbol targetType)
        {
            if (targetType.Equals(_type))
                return true;

            if (!_allowDerivedTypes)
                return false;

            var conversion = _semanticModel.Compilation.ClassifyConversion(_type, targetType);
            return conversion.IsReference;
        }
    }
}