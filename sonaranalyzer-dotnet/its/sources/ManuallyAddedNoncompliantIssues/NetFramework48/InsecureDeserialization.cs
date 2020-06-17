using System;
using System.Runtime.Serialization;

namespace NetFramework48
{
    [Serializable]
    public class CtorParameterIsNotInConditional
    {
        public string Name { get; set; }

        public CtorParameterIsNotInConditional(string name)
        {
            Name = name;
        }
    }

    [Serializable]
    public class CtorParameterConditionalConstruct
    {
        public string Name { get; set; }

        public CtorParameterConditionalConstruct(string name) // Noncompliant (S5766) {{Make sure not performing data validation after deserialization is safe here.}}
        {
            if (string.IsNullOrEmpty(name))
                Name = name;
        }
    }

    [Serializable]
    public class NoConditionals : ISerializable
    {
        protected NoConditionals(SerializationInfo info, StreamingContext context)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class CtorWithConditionsAndMissingDeserializationCtor : ISerializable
    {
        private string name;

        public CtorWithConditionsAndMissingDeserializationCtor(string name) // Noncompliant
        {
            this.name = name ?? string.Empty;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class DifferentConditionsInCtor : IDeserializationCallback
    {
        internal string Name { get; private set; }

        public DifferentConditionsInCtor(string name) // Noncompliant
        {
            Name = name ?? string.Empty;
        }

        public void OnDeserialization(object sender)
        {
        }
    }
}
