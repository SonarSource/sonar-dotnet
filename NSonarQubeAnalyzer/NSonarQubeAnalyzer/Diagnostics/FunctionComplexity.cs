using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;
using System.Text.RegularExpressions;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FunctionComplexity : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "FunctionComplexity";
        internal const string Description = "Method complexity should not be too high";
        internal const string MessageFormat = "Refactor this method that has a complexity of {1} (which is greater than {0} authorized).";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public int Maximum;

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var complexity = Metrics.Complexity(c.Node);
                    if (complexity > Maximum)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation(), Maximum, complexity));
                    }
                },
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);
        }
    }
}
