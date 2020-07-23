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

        public CtorParameterConditionalConstruct(string name) // Issue is raised only when using SonarScanner for MSBuild
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
    public class CtorWithConditionsAndMissingDeserializationCtor : ISerializable // Noncompliant (S3925)
    {
        private string name;

        public CtorWithConditionsAndMissingDeserializationCtor(string name) // Issue is raised only when using SonarScanner for MSBuild
        {
            this.name = name ?? string.Empty;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }

    [Serializable]
    public class DifferentConditionsInCtor : IDeserializationCallback
    {
        internal string Name { get; private set; }

        public DifferentConditionsInCtor(string name) // Issue is raised only when using SonarScanner for MSBuild
        {
            Name = name ?? string.Empty;
        }

        public void OnDeserialization(object sender)
        {
        }
    }
}
