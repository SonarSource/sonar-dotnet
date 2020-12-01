using System;
// Noncompliant@-1 // FP, the location is wrong for records
// Noncompliant@-2
// Noncompliant@-3

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

public record NoEventHandlerMethods // Compliant - FN
{
    [OptionalField]
    int optionalField = 5;
}

public record OnlyOnDeserializingEventHandlerMethod // Compliant - FN
{
    [OptionalField]
    int optionalField = 5;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context) => optionalField = 5;
}

[Serializable]
public record Record // Compliant - FN, attributes are added on local methods
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
