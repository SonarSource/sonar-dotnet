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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConstructorOverridableCall : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1699";
        private const string MessageFormat = "Remove this call from a constructor to the overridable '{0}' method.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                CheckOverridableCallInConstructor,
                SyntaxKind.InvocationExpression);
        }

        private static void CheckOverridableCallInConstructor(SonarSyntaxNodeReportingContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            var calledOn = (invocationExpression.Expression as MemberAccessExpressionSyntax)?.Expression;
            var isCalledOnThis = calledOn == null || calledOn is ThisExpressionSyntax;
            if (!isCalledOnThis)
            {
                return;
            }

            var enclosingSymbol = context.Model.GetEnclosingSymbol(invocationExpression.SpanStart) as IMethodSymbol;
            if (!IsMethodConstructor(enclosingSymbol))
            {
                return;
            }


            if (context.Model.GetSymbolInfo(invocationExpression.Expression).Symbol is IMethodSymbol methodSymbol &&
                IsMethodOverridable(methodSymbol) &&
                enclosingSymbol.IsInType(methodSymbol.ContainingType))
            {
                context.ReportIssue(rule, invocationExpression.Expression, methodSymbol.Name);
            }
        }

        private static bool IsMethodOverridable(IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsVirtual || methodSymbol.IsAbstract;
        }

        private static bool IsMethodConstructor(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.MethodKind == MethodKind.Constructor;
        }
    }
}
