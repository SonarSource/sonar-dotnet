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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReferenceEqualsOnValueType : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2995";
        private const string MessageFormat = "Use a different kind of comparison for these value types.";

        private const string ReferenceEqualsName = "ReferenceEquals";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax) c.Node;

                    var methodSymbol = c.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (methodSymbol.IsInType(KnownType.System_Object) &&
                        methodSymbol.Name == ReferenceEqualsName &&
                        AnyArgumentIsValueType(invocation.ArgumentList, c.SemanticModel))
                    {
                        c.ReportIssue(rule, invocation.Expression);
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static bool AnyArgumentIsValueType(ArgumentListSyntax argumentList, SemanticModel semanticModel)
        {
            return argumentList.Arguments.Any(argument =>
            {
                var type = semanticModel.GetTypeInfo(argument.Expression).Type;
                return type != null && type.IsValueType;
            });
        }
    }
}
