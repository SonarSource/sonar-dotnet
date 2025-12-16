using System;
using System.Runtime.Serialization;

namespace CSharp9
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

namespace CSharp10
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

namespace CSharp13
{
    [Serializable]
    partial class Partial
    {
        partial string Name { get; set; }

        public Partial(string name) // Noncompliant
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    partial class Partial
    {
        partial string Name
        {
            get => "Name";
            set => Name = value;
        }
    }
}

[Serializable]
public class ClassWithDefaultConstructorWithConditionalsIsSafe
{
    public string Name { get; set; }

    public ClassWithDefaultConstructorWithConditionalsIsSafe()
    {
        Name ??= string.Empty;
    }
}

[Serializable]
public class CtorParameterInCoalesceAssignmentExpression
{
    public CtorParameterInCoalesceAssignmentExpression(string name) // Noncompliant {{Make sure not performing data validation after deserialization is safe here.}}
    {
        name ??= string.Empty;
    }

    [Serializable]
    public class InNestedClass
    {
        public InNestedClass(string name) // Noncompliant {{Make sure not performing data validation after deserialization is safe here.}}
        {
            name ??= string.Empty;
        }
    }
}

[Serializable]
public class CtorParameterInSwitchExpression
{
    public string Name { get; set; }

    public CtorParameterInSwitchExpression(string name) // Noncompliant {{Make sure not performing data validation after deserialization is safe here.}}
    {
        Name = name switch
        {
            "1" => "one",
            _ => "else"
        };
    }

    public CtorParameterInSwitchExpression(int tmp)  // Compliant - no checks done on the parameter
    {
        Name = DateTime.Now.Kind switch
        {
            DateTimeKind.Unspecified => tmp.ToString(),
            _ => "else"
        };
    }
}

[Serializable]
public class CtorParameterInSwitchExpressionArm
{
    public string Name { get; set; }

    public CtorParameterInSwitchExpressionArm(string name) // Noncompliant {{Make sure not performing data validation after deserialization is safe here.}}
    {
        Name = "" switch
        {
            { } s when s == name => "one",
            _ => "else"
        };
    }
}


[Serializable]
public class DifferentConditionsInCtor : IDeserializationCallback
{
    internal string Name { get; private set; }

    public DifferentConditionsInCtor(string name) // Noncompliant
    {
        Name = name ?? string.Empty;
    }

    public void UnrelatedMethod(object sender) { Name ??= string.Empty; } // Name != OnDeserialization

    public void OnDeserialization() => Name ??= string.Empty; // Wrong number of parameters

    public void OnDeserialization(string a, string b) => Name ??= string.Empty; // wrong number of parameters

    public void OnDeserialization(object sender) { }
}

[Serializable]
public partial class PartialConstructor
{
    internal string Name { get; private set; }
    public partial PartialConstructor(string name);
}
