using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    [Serializable]
    public record struct RecordStructWithoutConstructorIsSafe
    {
        public string Name { get; set; } = "";
    }

    [Serializable]
    public record struct CtorParameterInIfStatement
    {
        public string Name { get; set; } = "";

        public CtorParameterInIfStatement(string name) // FN
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record struct CtorParameterInIfStatementPositionalRecordStruct(string Property)
    {
        public string Name { get; set; } = "";

        public CtorParameterInIfStatementPositionalRecordStruct(string name, string property) : this(property) // FN
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record struct WithParameters(int X)
    {
        public string Name { get; set; } = "";

        public WithParameters(string name) : this(name.Length) // FN
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public record struct CtorParameterInCoalesceAssignmentExpression
    {
        public CtorParameterInCoalesceAssignmentExpression(string name) // FN
        {
            name ??= string.Empty;
        }

        [Serializable]
        public record struct Nested
        {
            public Nested(string name) // FN
            {
                name ??= string.Empty;
            }
        }
    }

    [Serializable]
    public struct SomeStruct
    {
        public string Name { get; set; } = "";

        public SomeStruct(string name) // FN
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }
}
