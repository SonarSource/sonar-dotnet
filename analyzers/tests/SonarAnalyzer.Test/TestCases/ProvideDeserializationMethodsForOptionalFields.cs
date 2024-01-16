using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    public class NoEventHandlerMethods // Noncompliant {{Add deserialization event handlers.}}
//               ^^^^^^^^^^^^^^^^^^^^^
    {
        [OptionalField]
        int optionalField = 5;
    }

    public class OnlyOnDeserializingEventHandlerMethod // Noncompliant {{Add the missing 'OnDeserializedAttribute' event handler.}}
    {
        [OptionalFieldAttribute]
        int optionalField = 5;

        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            optionalField = 5;
        }
    }

    public class OnlyOnDeserializedEventHandlerMethod // Noncompliant {{Add the missing 'OnDeserializingAttribute' event handler.}}
    {
        [OptionalField]
        int optionalField = 5;

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            // Set optionalField if dependent on other deserialized values.
        }
    }

    [Serializable]
    public class Compliant
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

    public struct NoncompliantStruct // Noncompliant {{Add deserialization event handlers.}}
    {
        [OptionalField]
        int optionalField;
    }
}
