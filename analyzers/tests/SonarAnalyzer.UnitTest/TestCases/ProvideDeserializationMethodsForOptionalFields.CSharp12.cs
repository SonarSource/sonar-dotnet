using System;
using System.Runtime.Serialization;

class AttributeOnPrimaryConstructorParameter
{
    [Serializable]
    public class ParameterNotUsedInProperty([OptionalField] int optionalField) // Error [CS0592]
    {
    }

    [Serializable]
    public class ParameterUsedInProperty([OptionalField] int optionalField)    // Error [CS0592]
    {
        public int OptionalField => optionalField;
    }
}
