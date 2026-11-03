using System;
using System.Runtime.Serialization;

[Serializable]
public record Compliant
{
    [OptionalField]
    int optionalField = 5;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context)
    {
        optionalField = 5;
    }

    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        // Set optionalField if dependent on other deserialized values.
    }
}

public record NoEventHandlerMethods // Noncompliant
{
    [OptionalField]
    int optionalField = 5;
}

public record OnlyOnDeserializingEventHandlerMethod // Noncompliant
{
    [OptionalField]
    int optionalField = 5;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context) => optionalField = 5;
}

[Serializable]
public record Record // Noncompliant, attributes are added on local methods
{
    [OptionalField]
    int optionalField = 5;

    public Record()
    {
        [OnDeserializing]
        void OnDeserializing(StreamingContext context) => optionalField = 5;

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            // Set optionalField if dependent on other deserialized values.
        }
    }
}

public record NoEventHandlerMethodsWithParams(string Name) // Noncompliant
{
    [OptionalField]
    int optionalField = 5;
}

namespace CSharp15
{
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union
    public union UnionRepro(UnionCaseA, UnionCaseB) // Noncompliant
    {
        [OptionalField]
        static int optionalField = 5;
    }

    public union UnionCompliant(UnionCaseA, UnionCaseB) // Compliant
    {
        [OptionalField]
        static int optionalField = 5;

        [OnDeserializing]
        void OnDeserializing(StreamingContext context) => optionalField = 5;

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            // Set optionalField if dependent on other deserialized values.
        }
    }

    public record class UnionCaseA(string Name);
    public record class UnionCaseB(string Name);
}
