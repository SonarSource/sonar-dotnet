using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace NSonarQubeAnalyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ObjectCreatedDropped : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1848";
        internal const string Description = "Objects should not be created to be dropped immediately without being used";
        internal const string MessageFormat = @"Either remove this useless object instantiation of class ""{0}"" or use it";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var objectCreation = c.Node as ObjectCreationExpressionSyntax;
                    var parent = c.Node.Parent as ExpressionStatementSyntax;
                    if (objectCreation!= null && parent != null)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation(), objectCreation.Type));
                    }
                },
                SyntaxKind.ObjectCreationExpression);
        }
    }
}
