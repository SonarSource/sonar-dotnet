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
    public class ClassName : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S101";
        internal const string Description = "Class name should comply with a naming convention";
        internal const string MessageFormat = "Rename this class to match the regular expression: {0}";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public string Convention;

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var identifier = ((ClassDeclarationSyntax)c.Node).Identifier;

                    if (!Regex.IsMatch(identifier.Text, Convention))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), Convention));
                    }
                },
                SyntaxKind.ClassDeclaration);
        }
    }
}
