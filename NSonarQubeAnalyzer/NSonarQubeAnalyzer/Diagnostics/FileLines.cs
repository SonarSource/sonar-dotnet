namespace NSonarQubeAnalyzer.Diagnostics
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Xml.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileLines : DiagnosticsRule
    {
        internal const string DiagnosticId = "FileLoc";
        internal const string Description = "File should not have too many lines";
        internal const string MessageFormat = "This file has {1} lines, which is greater than {0} authorized. Split it into smaller files.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        /// <summary>
        /// Rule ID
        /// </summary>
        public override string RuleId
        {
            get
            {
                return "FileLoc";
            }
        }

        public override void SetDefaultSettings()
        {
            this.Maximum = 1000;
        }

        public override void UpdateParameters(Dictionary<string, string> parameters)
        {
            this.Maximum = int.Parse(parameters["maximumFileLocThreshold"]);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public int Maximum { get; set; }

        /// <summary>
        /// Configure the rule from the supplied settings
        /// </summary>
        /// <param name="settings">XML settings</param>
        public override void Configure(XDocument settings)
        {
            var parameters = from e in settings.Descendants("Rule")
                             where this.RuleId.Equals(e.Elements("Key").Single().Value)
                             select e.Descendants("Parameter");
            var maximum = (from e in parameters
                           where "maximumFileLocThreshold".Equals(e.Elements("Key").Single().Value)
                           select e.Elements("Value").Single().Value).Single();

            this.Maximum = int.Parse(maximum);
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    int lines = c.Node.GetLocation().GetLineSpan().EndLinePosition.Line + 1;

                    if (lines > this.Maximum)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, Location.None, this.Maximum, lines));
                    }
                },
                SyntaxKind.CompilationUnit);
        }
    }
}