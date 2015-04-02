using System;

namespace SonarQube.Analyzers.SonarQube.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RuleAttribute : Attribute
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public Severity Severity { get; set; }
        public bool IsActivatedByDefault { get; set; }
        public bool Template { get; set; }

        public RuleAttribute(string key, Severity severity, string description, bool isActivatedByDefault, bool template)
        {
            Key = key;
            Description = description;
            Severity = severity;
            IsActivatedByDefault = isActivatedByDefault;
            Template = template;
        }
        public RuleAttribute(string key, Severity severity, string description, bool isActivatedByDefault)
            :this(key, severity, description, isActivatedByDefault, false)
        {
        }
        public RuleAttribute(string key, Severity severity, string description)
            : this(key, severity, description, true, false)
        {
        }
    }
}