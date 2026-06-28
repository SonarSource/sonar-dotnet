/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisablingCsrfProtection : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4502";
    private const string MessageFormat = "Ensure CSRF protection is not disabled.";
    private const SyntaxKind ImplicitObjectCreationExpression = (SyntaxKind)8659;

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
         context.RegisterCompilationStartAction(
            c => c.RegisterNodeAction(
                    CheckIgnoreAntiforgeryTokenAttribute,
                    SyntaxKind.Attribute,
                    SyntaxKind.ObjectCreationExpression,
                    ImplicitObjectCreationExpression));

    private static void CheckIgnoreAntiforgeryTokenAttribute(SonarSyntaxNodeReportingContext c)
    {
        var shouldReport = c.Node switch
        {
            AttributeSyntax attributeSyntax => attributeSyntax.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute, c.Model),
            ObjectCreationExpressionSyntax objectCreation => objectCreation.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute, c.Model),
            _ => c.Node.IsKnownType(KnownType.Microsoft_AspNetCore_Mvc_IgnoreAntiforgeryTokenAttribute, c.Model)
        };

        if (shouldReport)
        {
            ReportDiagnostic(c);
        }
    }

    private static void ReportDiagnostic(SonarSyntaxNodeReportingContext context) =>
        context.ReportIssue(Rule, context.Node);
}
