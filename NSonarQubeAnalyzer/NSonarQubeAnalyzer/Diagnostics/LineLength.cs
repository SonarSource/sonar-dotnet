namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Xml.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LineLength : DiagnosticsRule
    {
        internal const string DiagnosticId = "LineLength";
        internal const string Description = "Lines should not be too long";
        internal const string MessageFormat = "Split this {1} characters long line (which is greater than {0} authorized).";
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
                return "LineLength";
            }
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
                           where "maximumLineLength".Equals(e.Elements("Key").Single().Value)
                           select e.Elements("Value").Single().Value).Single();

            this.Maximum = int.Parse(maximum);
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    foreach (var line in c.Tree.GetText().Lines)
                    {
                        if (line.Span.Length > this.Maximum)
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, c.Tree.GetLocation(line.Span), this.Maximum, line.Span.Length));
                        }
                    }
                });
        }
    }
}