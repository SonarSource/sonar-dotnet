namespace NSonarQubeAnalyzer.Diagnostics
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Xml.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TooManyLabelsInSwitch : DiagnosticsRule
    {
        internal const string DiagnosticId = "S1479";
        internal const string Description = "\"switch\" statements should not have too many \"case\" clauses";
        internal const string MessageFormat = "Reduce the number of switch cases from {1} to at most {0}.";
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
                return "S1479";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public int Maximum { get; set; }

        public override void SetDefaultSettings()
        {
            this.Maximum = 30;
        }

        public override void UpdateParameters(Dictionary<string, string> parameters)
        {
            this.Maximum = int.Parse(parameters["maximum"]);
        }

        /// <summary>
        /// Configure the rule from the supplied settings
        /// </summary>
        /// <param name="settings">XML settings</param>
        public override void Configure(XDocument settings)
        {
            var parameters = from e in settings.Descendants("Rule")
                             where this.RuleId.Equals(e.Elements("Key").Single().Value)
                             select e.Descendants("Parameter");
            var maximum =
                (from e in parameters
                 where "maximum".Equals(e.Elements("Key").Single().Value)
                 select e.Elements("Value").Single().Value).Single();

            this.Maximum = int.Parse(maximum);
        }

        public override void Initialize(AnalysisContext context)
        {
            if (!Status)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    SwitchStatementSyntax switchNode = (SwitchStatementSyntax)c.Node;
                    int labels = NumberOfLabels(switchNode);

                    if (labels > this.Maximum)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, switchNode.GetLocation(), this.Maximum, labels));
                    }
                },
                SyntaxKind.SwitchStatement);
        }

        private static int NumberOfLabels(SwitchStatementSyntax node)
        {
            return node.Sections.Sum(e => e.Labels.Count);
        }
    }
}