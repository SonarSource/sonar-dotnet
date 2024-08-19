using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    [Serializable]
    public record ClassWithoutConstructorIsSafe
    {
        public string Name { get; set; }
    }

    [Serializable]
    public record CtorParameterInIfStatement
    {
        public string Name { get; set; }

        public CtorParameterInIfStatement(string name) // Noncompliant
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record WithParameters(int X)
    {
        public string Name { get; set; }

        public WithParameters(string name) : this(name.Length) // Noncompliant
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record CtorParameterInCoalesceAssignmentExpression
    {
        public CtorParameterInCoalesceAssignmentExpression(string name) // Noncompliant {{Make sure not performing data validation after deserialization is safe here.}}
        {
            name ??= string.Empty;
        }

        [Serializable]
        public record Nested
        {
            public Nested(string name) // Noncompliant {{Make sure not performing data validation after deserialization is safe here.}}
            {
                name ??= string.Empty;
            }
        }
    }
}
