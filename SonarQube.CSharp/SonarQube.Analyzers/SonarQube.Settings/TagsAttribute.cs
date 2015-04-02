using System;

namespace SonarQube.Analyzers.SonarQube.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TagsAttribute : Attribute
    {
        public string[] Tags { get; set; }

        public TagsAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
