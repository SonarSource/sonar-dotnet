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
    public sealed class SpecifyStringComparison : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4058";
        private const string MessageFormat = "Change this call to '{0}' to an overload that accepts a " +
            "'StringComparison' as a parameter.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> stringComparisonType = ImmutableArray.Create(KnownType.System_StringComparison);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;

                    if (invocation.Expression != null &&
                        IsInvalidCall(invocation.Expression, c.SemanticModel) &&
                        CSharpOverloadHelper.HasOverloadWithType(invocation, c.SemanticModel, stringComparisonType))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, invocation.GetLocation(), invocation.Expression));
                    }
                }, SyntaxKind.InvocationExpression);
        }

        private static bool IsInvalidCall(ExpressionSyntax expression, SemanticModel semanticModel)
        {

            return semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol methodSymbol &&
                !HasAnyStringComparisonParameter(methodSymbol) &&
                methodSymbol.GetParameters().Any(parameter => parameter.Type.Is(KnownType.System_String)) &&
                !SpecifyIFormatProviderOrCultureInfo.HasAnyFormatOrCultureParameter(methodSymbol);
        }

        public static bool HasAnyStringComparisonParameter(IMethodSymbol method)
        {
            return method.GetParameters()
                .Any(parameter => parameter.Type.Is(KnownType.System_StringComparison));
        }
    }
}
