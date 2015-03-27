using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NoSqaleRemediation]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.IntegrationTestability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class ParameterAssignedTo : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "ParameterAssignedTo";
        internal const string Description = "Parameter variable should not be assigned to";
        internal const string MessageFormat = "Remove this assignment to the method parameter '{0}'.";
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
                    var assignmentNode = (AssignmentExpressionSyntax)c.Node;
                    var symbol = c.SemanticModel.GetSymbolInfo(assignmentNode.Left).Symbol;

                    if (symbol != null && AssignsToParameter(symbol))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, assignmentNode.GetLocation(), assignmentNode.Left.ToString()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression);
        }

        private static bool AssignsToParameter(ISymbol symbol)
        {
            var parameterSymbol = symbol as IParameterSymbol;

            if (parameterSymbol == null)
            {
                return false;
            }
            
            return parameterSymbol.RefKind == RefKind.None;
        }
    }
}
