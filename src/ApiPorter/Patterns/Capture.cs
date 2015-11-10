using System;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    internal sealed class Capture
    {
        private Capture(PatternVariable variable, SyntaxNodeOrToken nodeOrToken)
        {
            Variable = variable;
            NodeOrToken = nodeOrToken;
        }

        public static Capture Create(PatternVariable variable, SyntaxNodeOrToken nodeOrToken)
        {
            return new Capture(variable, nodeOrToken);
        }

        public PatternVariable Variable { get; }

        public SyntaxNodeOrToken NodeOrToken { get; }
    }
}