/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.CSharp
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

            var enclosingSymbol = context.SemanticModel.GetEnclosingSymbol(invocationExpression.SpanStart) as IMethodSymbol;
            if (!IsMethodConstructor(enclosingSymbol))
            {
                return;
            }


            if (context.SemanticModel.GetSymbolInfo(invocationExpression.Expression).Symbol is IMethodSymbol methodSymbol &&
                IsMethodOverridable(methodSymbol) &&
                enclosingSymbol.IsInType(methodSymbol.ContainingType))
            {
                context.ReportIssue(CreateDiagnostic(rule, invocationExpression.Expression.GetLocation(),
                    methodSymbol.Name));
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
