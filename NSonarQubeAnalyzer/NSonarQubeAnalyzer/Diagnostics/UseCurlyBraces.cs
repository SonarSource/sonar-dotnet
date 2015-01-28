namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseCurlyBraces : DiagnosticsRule
    {
        internal const string DiagnosticId = "S121";
        internal const string Description = "Control structures should always use curly braces";
        internal const string MessageFormat = "Add curly braces around the nested statement(s) in this \"{0}\" block.";
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
                return "S121";
            }
        }

        private class CheckedKind
        {
            public SyntaxKind Kind;
            public string Value;
            public Func<SyntaxNode, bool> Validator;
        }

        private readonly ImmutableList<CheckedKind> CheckedKinds = ImmutableList.Create(
            new CheckedKind
            {
                Kind = SyntaxKind.IfStatement,
                Value = "if",
                Validator = node => ((IfStatementSyntax)node).Statement.IsKind(SyntaxKind.Block)
            },
            new CheckedKind
            {
                Kind = SyntaxKind.ElseClause,
                Value = "else",
                Validator =
                    node =>
                    {
                        var statement = ((ElseClauseSyntax)node).Statement;
                        return statement.IsKind(SyntaxKind.IfStatement) || statement.IsKind(SyntaxKind.Block);
                    }
            },
            new CheckedKind
            {
                Kind = SyntaxKind.ForStatement,
                Value = "for",
                Validator = node => ((ForStatementSyntax)node).Statement.IsKind(SyntaxKind.Block)
            },
            new CheckedKind
            {
                Kind = SyntaxKind.ForEachStatement,
                Value = "foreach",
                Validator = node => ((ForEachStatementSyntax)node).Statement.IsKind(SyntaxKind.Block)
            },
            new CheckedKind
            {
                Kind = SyntaxKind.DoStatement,
                Value = "do",
                Validator = node => ((DoStatementSyntax)node).Statement.IsKind(SyntaxKind.Block)
            },
            new CheckedKind
            {
                Kind = SyntaxKind.WhileStatement,
                Value = "while",
                Validator = node => ((WhileStatementSyntax)node).Statement.IsKind(SyntaxKind.Block)
            });

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var checkedKind = this.CheckedKinds.Where(e => c.Node.IsKind(e.Kind)).Single();

                    if (!checkedKind.Validator(c.Node))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation(), checkedKind.Value));
                    }
                },
                this.CheckedKinds.Select(e => e.Kind).ToArray());
        }
    }
}