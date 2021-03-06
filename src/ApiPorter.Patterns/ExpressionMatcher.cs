using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal sealed class ExpressionMatcher : ExpressionMatcher<ExpressionSyntax>
    {
        public ExpressionMatcher(PatternVariable variable, SemanticModel semanticModel, ITypeSymbol type, bool allowDerivedTypes)
            : base(variable, semanticModel, type, allowDerivedTypes)
        {
        }
    }
}