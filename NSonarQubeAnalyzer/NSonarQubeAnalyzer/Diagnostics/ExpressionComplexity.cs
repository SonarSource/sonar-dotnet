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
    public class ExpressionComplexity : DiagnosticsRule
    {
        internal const string DiagnosticId = "S1067";
        internal const string Description = "Expressions should not be too complex";
        internal const string MessageFormat = "Reduce the number of conditional operators ({1}) used in the expression (maximum allowed {0}).";
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
                return "S1067";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public int Maximum { get; set; }

        public override void SetDefaultSettings()
        {
            this.Maximum = 3;
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

        private IImmutableSet<SyntaxKind> CompoundExpressionKinds = ImmutableHashSet.Create(new SyntaxKind[] {
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ArrayInitializerExpression,
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ObjectInitializerExpression,
            SyntaxKind.InvocationExpression});

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var rootExpressions =
                        c.Node
                        .DescendantNodes(e2 => !(e2 is ExpressionSyntax))
                        .Where(
                            e =>
                                e is ExpressionSyntax &&
                                !this.IsCompoundExpression(e));

                    var compoundExpressionsDescendants =
                        c.Node
                        .DescendantNodes()
                        .Where(e => this.IsCompoundExpression(e))
                        .SelectMany(
                            e =>
                                e
                                .DescendantNodes(
                                    e2 =>
                                        e == e2 ||
                                        !(e2 is ExpressionSyntax))
                                .Where(
                                    e2 =>
                                        e2 is ExpressionSyntax &&
                                        !this.IsCompoundExpression(e2)));

                    var expressionsToCheck = rootExpressions.Concat(compoundExpressionsDescendants);

                    var complexExpressions =
                        expressionsToCheck
                        .Select(
                            e =>
                            new
                            {
                                Expression = e,
                                Complexity =
                                    e
                                    .DescendantNodesAndSelf(e2 => !this.IsCompoundExpression(e2))
                                    .Where(
                                        e2 =>
                                            e2.IsKind(SyntaxKind.ConditionalExpression) ||
                                            e2.IsKind(SyntaxKind.LogicalAndExpression) ||
                                            e2.IsKind(SyntaxKind.LogicalOrExpression))
                                    .Count()
                            })
                        .Where(e => e.Complexity > this.Maximum);

                    foreach (var complexExpression in complexExpressions)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, complexExpression.Expression.GetLocation(), this.Maximum, complexExpression.Complexity));
                    }
                },
                SyntaxKind.CompilationUnit);
        }

        private bool IsCompoundExpression(SyntaxNode node)
        {
            return this.CompoundExpressionKinds.Any(k => node.IsKind(k));
        }
    }
}