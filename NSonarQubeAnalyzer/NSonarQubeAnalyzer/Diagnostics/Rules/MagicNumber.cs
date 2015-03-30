using System.Collections.Immutable;
using System.Linq;
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
    [SqaleConstantRemediation("5min")]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class MagicNumber : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "MagicNumber";
        internal const string Description = "Magic number should not be used";
        internal const string MessageFormat = "Extract this magic number into a constant, variable declaration or an enum.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        [RuleParameter("exceptions", PropertyType.String, "Comma separated list of allowed values (excluding '-' and '+' signs)", "0,1,0x0,0x00,.0,.1,0.0,1.0")]
        public IImmutableSet<string> Exceptions { get; set; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var literalNode = (LiteralExpressionSyntax)c.Node;

                    if (!literalNode.IsPartOfStructuredTrivia() &&
                        !literalNode.Ancestors().Any(e =>
                          e.IsKind(SyntaxKind.VariableDeclarator) ||
                          e.IsKind(SyntaxKind.EnumDeclaration) ||
                          e.IsKind(SyntaxKind.Attribute)) &&
                        !Exceptions.Contains(literalNode.Token.Text))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, literalNode.GetLocation()));
                    }
                },
                SyntaxKind.NumericLiteralExpression);
        }
    }
}
