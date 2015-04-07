using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [SqaleConstantRemediation("2min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [Tags("cert", "cwe", "misra", "pitfall")]
    public class UseCurlyBraces : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S121";
        internal const string Description = "Control structures should always use curly braces";
        internal const string MessageFormat = "Add curly braces around the nested statement(s) in this \"{0}\" block.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = false;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault);

        private sealed class CheckedKind
        {
            public SyntaxKind Kind;
            public string Value;
            public Func<SyntaxNode, bool> Validator;
        }

        private readonly ImmutableList<CheckedKind> checkedKinds = ImmutableList.Create(
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
                    var checkedKind = checkedKinds.Single(e => c.Node.IsKind(e.Kind));

                    if (!checkedKind.Validator(c.Node))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation(), checkedKind.Value));
                    }
                },
                checkedKinds.Select(e => e.Kind).ToArray());
        }
    }
}
