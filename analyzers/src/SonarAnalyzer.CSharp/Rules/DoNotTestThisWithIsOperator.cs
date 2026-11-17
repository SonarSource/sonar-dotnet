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
public sealed class DoNotTestThisWithIsOperator : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3060";
    private const string MessageFormat = "Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(AnalyzeIsExpression, SyntaxKind.IsExpression);
        context.RegisterNodeAction(AnalyzeIsPatternExpression, SyntaxKindEx.IsPatternExpression);
        context.RegisterNodeAction(AnalyzeSwitchExpression, SyntaxKindEx.SwitchExpression);
        context.RegisterNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
    }

    private static void AnalyzeIsExpression(SonarSyntaxNodeReportingContext context)
    {
        if (IsNonUnionThisExpression(context.Model, ((BinaryExpressionSyntax)context.Node).Left))
        {
            context.ReportIssue(Rule, context.Node);
        }
    }

    private static void AnalyzeIsPatternExpression(SonarSyntaxNodeReportingContext context)
    {
        if (IsNonUnionThisExpression(context.Model, ((IsPatternExpressionSyntaxWrapper)context.Node).Expression) && ContainsTypeCheckInPattern(context.Node))
        {
            context.ReportIssue(Rule, context.Node);
        }
    }

    private static void AnalyzeSwitchStatement(SonarSyntaxNodeReportingContext context)
    {
        var switchStatement = (SwitchStatementSyntax)context.Node;
        if (IsNonUnionThisExpression(context.Model, switchStatement.Expression)
            && CollectSecondaryLocations(switchStatement) is { Count: > 0 } secondaryLocations)
        {
            context.ReportIssue(Rule, switchStatement.Expression, secondaryLocations);
        }
    }

    private static void AnalyzeSwitchExpression(SonarSyntaxNodeReportingContext context)
    {
        var switchExpression = (SwitchExpressionSyntaxWrapper)context.Node;
        if (IsNonUnionThisExpression(context.Model, switchExpression.GoverningExpression)
            && CollectSecondaryLocations(switchExpression) is { Count: > 0 } secondaryLocations)
        {
            context.ReportIssue(Rule, switchExpression.GoverningExpression, secondaryLocations);
        }
    }

    private static IList<SecondaryLocation> CollectSecondaryLocations(SwitchStatementSyntax switchStatement) =>
        switchStatement.Sections
            .SelectMany(x => x.Labels
                                .Where(label => ContainsTypeCheckInPattern(label) || ContainsTypeCheckInCaseSwitchLabel(label))
                                .Select(label => new SecondaryLocation(TypeMatchLocation(label), string.Empty)))
            .ToList();

    private static IList<SecondaryLocation> CollectSecondaryLocations(SwitchExpressionSyntaxWrapper switchExpression) =>
        switchExpression.Arms.Where(x => ContainsTypeCheckInPattern(x.Pattern.WrappedInstance))
            .Select(x => new SecondaryLocation(x.Pattern.WrappedInstance.GetLocation(), string.Empty))
            .ToList();

    private static bool ContainsTypeCheckInCaseSwitchLabel(SwitchLabelSyntax switchLabel) =>
        switchLabel is CaseSwitchLabelSyntax caseSwitchLabel && caseSwitchLabel.Value.IsKind(SyntaxKind.IdentifierName);

    private static bool ContainsTypeCheckInPattern(SyntaxNode node) =>
        node.DescendantNodesAndSelf().Any(IsTopLevelTypeCheck);

    private static bool IsTopLevelTypeCheck(SyntaxNode pattern) =>
        pattern.Kind() switch
        {
            SyntaxKindEx.ConstantPattern => ((ConstantPatternSyntaxWrapper)pattern).Expression.IsKind(SyntaxKind.IdentifierName),
            SyntaxKindEx.RecursivePattern => ((RecursivePatternSyntaxWrapper)pattern).Type is not null,
            SyntaxKindEx.DeclarationPattern or SyntaxKindEx.ListPattern => true,
            _ => false
        }
        && !IsInSubPattern(pattern);

    private static bool IsInSubPattern(SyntaxNode node) =>
        node.FirstAncestorOrSelf<SyntaxNode>(x => x.Kind() is SyntaxKindEx.IsPatternExpression
                                                                or SyntaxKindEx.SwitchExpression
                                                                or SyntaxKind.SwitchStatement
                                                                or SyntaxKindEx.Subpattern)
            .IsKind(SyntaxKindEx.Subpattern);

    private static Location TypeMatchLocation(SwitchLabelSyntax label) =>
        label.Kind() switch
        {
            SyntaxKind.CaseSwitchLabel => ((CaseSwitchLabelSyntax)label).Value.GetLocation(),
            SyntaxKindEx.CasePatternSwitchLabel => ((CasePatternSwitchLabelSyntaxWrapper)label).Pattern.WrappedInstance.GetLocation(),
            _ => Location.None
        };

    private static bool IsNonUnionThisExpression(SemanticModel model, SyntaxNode node) =>
        node.RemoveParentheses() is ThisExpressionSyntax thisExpression
        && model.GetTypeInfo(thisExpression).Type is not { IsUnion: true };
}
