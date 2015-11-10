using System;

using Microsoft.CodeAnalysis;

namespace ApiPorter.Patterns
{
    internal abstract class Matcher
    {
        public abstract Match Execute(SyntaxNodeOrToken nodeOrToken);
    }
}