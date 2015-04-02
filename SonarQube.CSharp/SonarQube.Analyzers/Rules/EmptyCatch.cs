using System.Collections.Immutable;
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
    [SqaleSubCharacteristic(SqaleSubCharacteristic.ExceptionHandling)]
    [SqaleConstantRemediation("1h")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class EmptyCatch : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2486";
        internal const string Description = @"Exceptions should not be ignored";
        internal const string MessageFormat = @"Handle the exception, rather than swallow it with an empty statement";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;

                    if (!catchClause.Block.Statements.Any())
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation()));
                    }
                },
                SyntaxKind.CatchClause);
        }
    }
}
