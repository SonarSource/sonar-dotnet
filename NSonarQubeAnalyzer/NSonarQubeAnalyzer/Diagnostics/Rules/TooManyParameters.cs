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
    [SqaleSubCharacteristic(SqaleSubCharacteristic.UnitTestability)]
    [SqaleConstantRemediation("20min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [Tags("brain-overload")]
    public class TooManyParameters : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S107";
        internal const string Description = "Functions should not have too many parameters";
        internal const string MessageFormat = "{2} has {1} parameters, which is greater than the {0} authorized.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        [RuleParameter("max", PropertyType.Integer, "Maximum authorized number of parameters", "7")]
        public int Maximum { get; set; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var parameterListNode = (ParameterListSyntax)c.Node;
                    var parameters = parameterListNode.Parameters.Count;

                    if (parameters > Maximum)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, parameterListNode.GetLocation(), Maximum, parameters, ExtractName(parameterListNode)));
                    }
                },
                SyntaxKind.ParameterList);
        }

        private static string ExtractName(SyntaxNode node)
        {
            string result;
            if (node.IsKind(SyntaxKind.ConstructorDeclaration))
            {
                result = "Constructor \"" + ((ConstructorDeclarationSyntax)node).Identifier + "\"";
            }
            else if (node.IsKind(SyntaxKind.MethodDeclaration))
            {
                result = "Method \"" + ((MethodDeclarationSyntax)node).Identifier + "\"";
            }
            else if (node.IsKind(SyntaxKind.DelegateDeclaration))
            {
                result = "Delegate";
            }
            else
            {
                result = "Lambda";
            }
            return result;
        }
    }
}
