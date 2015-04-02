using System;

namespace SonarQube.Analyzers.SonarQube.Settings.Sqale
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class SqaleRemediationAttribute : Attribute
    {
    }
}