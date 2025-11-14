/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
                        IsInvalidCall(invocation.Expression, c.Model) &&
                        SonarAnalyzer.CSharp.Syntax.Extensions.InvocationExpressionSyntaxExtensions.HasOverloadWithType(invocation, c.Model, stringComparisonType))
                    {
                        c.ReportIssue(rule, invocation, invocation.Expression.ToString());
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
