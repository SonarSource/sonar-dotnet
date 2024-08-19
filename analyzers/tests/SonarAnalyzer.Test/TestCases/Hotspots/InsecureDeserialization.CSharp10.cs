using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    [Serializable]
    public record struct RecordStructWithoutConstructorIsSafe
    {
        public RecordStructWithoutConstructorIsSafe() { }

        public string Name { get; set; } = "";
    }

    [Serializable]
    public record struct CtorParameterInIfStatement
    {
        public string Name { get; set; } = "";

        public CtorParameterInIfStatement(string name) // Noncompliant {{Make sure not performing data validation after deserialization is safe here.}}
        //     ^^^^^^^^^^^^^^^^^^^^^^^^^^
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record struct CtorParameterInIfStatementPositionalRecordStruct(string Property)
    {
        public string Name { get; set; } = "";

        public CtorParameterInIfStatementPositionalRecordStruct(string name, string property) : this(property) // Noncompliant
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record struct WithParameters(int X)
    {
        public string Name { get; set; } = "";

        public WithParameters(string name) : this(name.Length) // Noncompliant
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record struct CtorParameterInCoalesceAssignmentExpression
    {
        public CtorParameterInCoalesceAssignmentExpression(string name) // Noncompliant
        {
            name ??= string.Empty;
        }

        [Serializable]
        public record struct Nested
        {
            public Nested(string name) // Noncompliant
            {
                name ??= string.Empty;
            }
        }
    }

    [Serializable]
    public struct SomeStruct
    {
        public string Name { get; set; } = "";

        public SomeStruct(string name) // Noncompliant
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }
}
