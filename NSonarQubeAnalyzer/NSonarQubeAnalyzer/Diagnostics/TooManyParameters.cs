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
    public class TooManyParameters : DiagnosticsRule
    {
        internal const string DiagnosticId = "S107";
        internal const string Description = "Functions should not have too many parameters";
        internal const string MessageFormat = "{2} has {1} parameters, which is greater than the {0} authorized.";
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
                return "S107";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public int Maximum { get; set; }

        public override void SetDefaultSettings()
        {
            this.Maximum = 7;
        }

        public override void UpdateParameters(Dictionary<string, string> parameters)
        {
            this.Maximum = int.Parse(parameters["max"]);
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
                 where "max".Equals(e.Elements("Key").Single().Value)
                 select e.Elements("Value").Single().Value).Single();

            this.Maximum = int.Parse(maximum);
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var parameterListNode = (ParameterListSyntax)c.Node;
                    int parameters = parameterListNode.Parameters.Count;

                    if (parameters > this.Maximum)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, parameterListNode.GetLocation(), this.Maximum, parameters, ExtractName(parameterListNode)));
                    }
                },
                SyntaxKind.ParameterList);
        }

        private static string ExtractName(SyntaxNode node)
        {
            string result;
            if (node.IsKind(SyntaxKind.ConstructorDeclaration))
            {
                result = "Constructor \"" + ((ConstructorDeclarationSyntax)node).Identifier + "\"";
            }
            else if (node.IsKind(SyntaxKind.MethodDeclaration))
            {
                result = "Method \"" + ((MethodDeclarationSyntax)node).Identifier + "\"";
            }
            else if (node.IsKind(SyntaxKind.DelegateDeclaration))
            {
                result = "Delegate";
            }
            else
            {
                result = "Lambda";
            }
            return result;
        }
    }
}