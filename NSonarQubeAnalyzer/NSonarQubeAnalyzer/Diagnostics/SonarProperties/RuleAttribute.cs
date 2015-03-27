using System;

namespace NSonarQubeAnalyzer.Diagnostics.SonarProperties
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RuleAttribute : Attribute
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public Severity Severity { get; set; }
        public bool IsActivatedByDefault { get; set; }
        public bool Template { get; set; }

        public RuleAttribute(string key, Severity severity, string description = null, bool isActivatedByDefault = true, bool template = false)
        {
            Key = key;
            Description = description;
            Severity = severity;
            IsActivatedByDefault = isActivatedByDefault;
            Template = template;
        }
    }
}