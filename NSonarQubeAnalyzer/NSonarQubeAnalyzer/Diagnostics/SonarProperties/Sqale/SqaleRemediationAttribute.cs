using System;

namespace NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class SqaleRemediationAttribute : Attribute
    {
    }
}