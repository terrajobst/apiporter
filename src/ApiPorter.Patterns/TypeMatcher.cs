using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiPorter.Patterns
{
    internal sealed class TypeMatcher : ExpressionMatcher<TypeSyntax>
    {
        public TypeMatcher(PatternVariable variable, SemanticModel semanticModel, ITypeSymbol type, bool allowDerivedTypes)
            : base(variable, semanticModel, type, allowDerivedTypes)
        {
        }
    }
}