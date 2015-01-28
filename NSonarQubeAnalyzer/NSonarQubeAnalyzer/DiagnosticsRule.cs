namespace NSonarQubeAnalyzer
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Xml.Linq;

    /// <summary>
    /// Base class for Diagnostics Rules
    /// </summary>
    public abstract class DiagnosticsRule : DiagnosticAnalyzer
    {
        /// <summary>
        /// Rule ID
        /// </summary>
        public abstract string RuleId { get; }

        /// <summary>
        /// Configure the rule from the supplied settings
        /// </summary>
        /// <param name="settings">XML settings</param>
        public virtual void Configure(XDocument settings)
        {
        }
    }
}