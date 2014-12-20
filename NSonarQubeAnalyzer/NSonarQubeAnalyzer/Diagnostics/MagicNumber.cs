namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Xml.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MagicNumber : DiagnosticsRule
    {
        internal const string DiagnosticId = "MagicNumber";
        internal const string Description = "Magic number should not be used";
        internal const string MessageFormat = "Extract this magic number into a constant, variable declaration or an enum.";
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
                return "MagicNumber";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public IImmutableSet<string> Exceptions { get; set; }

        public override void SetDefaultSettings()
        {
            this.Exceptions = ImmutableHashSet.Create("0", "1", "0x0", "0x00", ".0", ".1", "0.0", "1.0");
        }

        public override void UpdateParameters(Dictionary<string, string> parameters)
        {
            var set = new HashSet<string>();
            foreach (var exc in parameters["exceptions"].Split(','))
            {
                set.Add(exc);
            }

            Exceptions = set.ToImmutableHashSet();
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
            var exceptions = (from e in parameters
                              where "exceptions".Equals(e.Elements("Key").Single().Value)
                              select e.Elements("Value").Single().Value).Single();
            this.Exceptions =
                exceptions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToImmutableHashSet();
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    LiteralExpressionSyntax literalNode = (LiteralExpressionSyntax)c.Node;

                    if (!literalNode.IsPartOfStructuredTrivia() &&
                        !literalNode.Ancestors().Any(e =>
                          e.IsKind(SyntaxKind.VariableDeclarator) ||
                          e.IsKind(SyntaxKind.EnumDeclaration) ||
                          e.IsKind(SyntaxKind.Attribute)) &&
                        !this.Exceptions.Contains(literalNode.Token.Text))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, literalNode.GetLocation()));
                    }
                },
                SyntaxKind.NumericLiteralExpression);
        }
    }
}