using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Threading;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParameterAssignedTo : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "ParameterAssignedTo";
        internal const string Description = "Parameter variable should not be assigned to";
        internal const string MessageFormat = "Remove this assignment to the method parameter '{0}'.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

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

        private bool AssignsToParameter(ISymbol symbol)
        {
            if (!(symbol is IParameterSymbol))
            {
                return false;
            }

            var parameter = (IParameterSymbol)symbol;

            return parameter.RefKind == RefKind.None;
        }
    }
}
