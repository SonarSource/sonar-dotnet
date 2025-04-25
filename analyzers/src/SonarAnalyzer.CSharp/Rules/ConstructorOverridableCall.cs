/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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
public sealed class ConstructorOverridableCall : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1699";
    private const string MessageFormat = "Remove this call from a constructor to the overridable '{0}' method.";

    private static readonly DiagnosticDescriptor Rule =
        DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSymbolStartAction(c =>
        {
            if (c.Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor, ContainingType.IsSealed: false } constructor
                && !constructor.ContainingType.DerivesFrom(KnownType.Nancy_NancyModule))
            {
                c.RegisterSyntaxNodeAction(cc => CheckOverridableCallInConstructor(cc, constructor), SyntaxKind.InvocationExpression);
            }
        }, SymbolKind.Method);

    private static void CheckOverridableCallInConstructor(SonarSyntaxNodeReportingContext context, IMethodSymbol constructor)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (invocationExpression.Expression is IdentifierNameSyntax or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax }
            && context.Model.GetEnclosingSymbol(invocationExpression.SpanStart).Equals(constructor)
            && context.Model.GetSymbolInfo(invocationExpression.Expression).Symbol is IMethodSymbol methodSymbol
            && IsMethodOverridable(methodSymbol))
        {
            context.ReportIssue(Rule, invocationExpression.Expression, methodSymbol.Name);
        }
    }

    private static bool IsMethodOverridable(IMethodSymbol methodSymbol) =>
        methodSymbol.IsVirtual
        || methodSymbol.IsAbstract
        || (methodSymbol.IsOverride && !methodSymbol.IsSealed);
}
