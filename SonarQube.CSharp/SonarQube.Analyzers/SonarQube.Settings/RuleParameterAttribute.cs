using System;

namespace SonarQube.Analyzers.SonarQube.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RuleParameterAttribute : Attribute
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public PropertyType Type { get; set; }
        public string DefaultValue { get; set; }

        public RuleParameterAttribute(string key, PropertyType type, string description, string defaultValue)
        {
            Key = key;
            Description = description;
            Type = type;
            DefaultValue = defaultValue;
        }
        public RuleParameterAttribute(string key, PropertyType type, string description)
            : this(key, type, description, null)
        {
        }
        public RuleParameterAttribute(string key, PropertyType type)
            : this(key, type, null, null)
        {
        }
    }
}