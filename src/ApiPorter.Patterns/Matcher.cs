using System;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    internal abstract partial class Matcher
    {
        public abstract Match Run(SyntaxNodeOrToken nodeOrToken);
    }
}