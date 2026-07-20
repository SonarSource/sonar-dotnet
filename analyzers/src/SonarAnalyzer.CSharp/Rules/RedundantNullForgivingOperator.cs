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
public sealed class RedundantNullForgivingOperator : SonarDiagnosticAnalyzer
{
    internal const string RedundantDiagnosticId = "S8969";
    internal const string WarningsDisabledDiagnosticId = "S8970";
    private const string RedundantMessageFormat = "Remove this null-forgiving operator; the compiler already knows this expression is not null here.";
    private const string WarningsDisabledMessageFormat = "Remove this null-forgiving operator; nullable warnings are disabled here.";

    internal static readonly DiagnosticDescriptor RuleS8969 = DescriptorFactory.Create(RedundantDiagnosticId, RedundantMessageFormat);
    internal static readonly DiagnosticDescriptor RuleS8970 = DescriptorFactory.Create(WarningsDisabledDiagnosticId, WarningsDisabledMessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleS8969, RuleS8970);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var suppression = (PostfixUnaryExpressionSyntax)c.Node;
                // Warning state must be sampled exactly at the "!" because "#nullable" directives can appear between any tokens.
                if (!c.Model.GetNullableContext(suppression.OperatorToken.SpanStart).HasFlag(NullableContext.WarningsEnabled))
                {
                    c.ReportIssue(RuleS8970, suppression.OperatorToken);
                }
                else if (IsRedundant(c.Model, suppression))
                {
                    c.ReportIssue(RuleS8969, suppression.OperatorToken);
                }
            },
            SyntaxKindEx.SuppressNullableWarningExpression);

    // GetTypeInfo always reports NotNull because of the "!" operator.
    // We speculatively check the operand without the "!" to see if it was not null anyway and therefore redundant.
    private static bool IsRedundant(SemanticModel model, PostfixUnaryExpressionSyntax suppression)
    {
        var typeInfo = model.GetSpeculativeTypeInfo(suppression.Operand.SpanStart, suppression.Operand, SpeculativeBindingOption.BindAsExpression);
        return typeInfo.Nullability().FlowState == NullableFlowState.NotNull && !HasNestedNullableAnnotation(typeInfo.Type);
    }

    // NotNull FlowState only guarantees the top-level reference isn't null, not nested generic or array annotations. We keep "!" to prevent CS8619/CS8620.
    private static bool HasNestedNullableAnnotation(ITypeSymbol type) =>
        type switch
        {
            IArrayTypeSymbol array => array.ElementNullableAnnotation() == NullableAnnotation.Annotated || HasNestedNullableAnnotation(array.ElementType),
            INamedTypeSymbol { IsGenericType: true } named => named.TypeArguments.Zip(
                named.TypeArgumentNullableAnnotations(),
                (arg, ann) => ann == NullableAnnotation.Annotated || HasNestedNullableAnnotation(arg)).Contains(true),
            _ => false,
        };
}
