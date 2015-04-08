using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleConstantRemediation("2min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Readability)]
    [LegacyKey("TabCharacter")]
    [Tags("convention")]
    public class TabCharacter : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S105";
        internal const string Description = "Tabulation character should not be used";
        internal const string MessageFormat = "Replace all tab characters in this file by sequences of white-spaces.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Minor; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category,
                RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault,
                helpLinkUri: "http://nemo.sonarqube.org/coding_rules#rule_key=csharpsquid%3ATabCharacter");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    var offset = c.Tree.GetText().ToString().IndexOf('\t');
                    if (offset < 0)
                    {
                        return;
                    }

                    var location = c.Tree.GetLocation(TextSpan.FromBounds(offset, offset));
                    c.ReportDiagnostic(Diagnostic.Create(Rule, location));
                });
        }
    }
}
