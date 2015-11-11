using System;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    public sealed class PatternCapture
    {
        private PatternCapture(PatternVariable variable, SyntaxNodeOrToken startNodeOrToken, SyntaxNodeOrToken endNodeOrToken)
        {
            Variable = variable;
            StartNodeOrToken = startNodeOrToken;
            EndNodeOrToken = endNodeOrToken;
        }

        public static PatternCapture Create(PatternVariable variable, SyntaxNodeOrToken nodeOrToken)
        {
            return new PatternCapture(variable, nodeOrToken, nodeOrToken);
        }

        public static PatternCapture Create(PatternVariable variable, SyntaxNodeOrToken startNodeOrToken, SyntaxNodeOrToken endNodeOrToken)
        {
            return new PatternCapture(variable, startNodeOrToken, endNodeOrToken);
        }

        public PatternVariable Variable { get; }

        public SyntaxNodeOrToken StartNodeOrToken { get; }

        public SyntaxNodeOrToken EndNodeOrToken { get; }
    }
}