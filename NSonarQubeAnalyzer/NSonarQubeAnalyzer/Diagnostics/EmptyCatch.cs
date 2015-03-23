using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class EmptyCatch : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2486";
        internal const string Description = @"Exceptions should not be ignored";
        internal const string MessageFormat = @"Handle the exception, rather than swallow it with an empty statement";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;


        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

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
