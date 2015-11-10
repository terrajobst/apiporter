using System;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    public sealed class PatternCapture
    {
        private PatternCapture(PatternVariable variable, SyntaxNodeOrToken nodeOrToken)
        {
            Variable = variable;
            NodeOrToken = nodeOrToken;
        }

        public static PatternCapture Create(PatternVariable variable, SyntaxNodeOrToken nodeOrToken)
        {
            return new PatternCapture(variable, nodeOrToken);
        }

        public PatternVariable Variable { get; }

        public SyntaxNodeOrToken NodeOrToken { get; }
    }
}