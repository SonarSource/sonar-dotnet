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
    public sealed class OrderByRepeated : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3169";
        private const string MessageFormat = "Use 'ThenBy' instead.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var outerInvocation = (InvocationExpressionSyntax)c.Node;
                    if (outerInvocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is InvocationExpressionSyntax innerInvocation &&
                        IsMethodOrderByExtension(outerInvocation, c.Model) &&
                        IsOrderByOrThenBy(innerInvocation, c.Model))
                    {
                        c.ReportIssue(rule, memberAccess.Name);
                    }

                    static bool IsOrderByOrThenBy(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
                        IsMethodOrderByExtension(invocation, semanticModel) || IsMethodThenByExtension(invocation, semanticModel);
                },
                SyntaxKind.InvocationExpression);
        }
        private static bool IsMethodOrderByExtension(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.Expression.ToStringContains("OrderBy") &&
            semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
               methodSymbol.Name == "OrderBy" &&
               methodSymbol.MethodKind == MethodKind.ReducedExtension &&
               methodSymbol.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);

        private static bool IsMethodThenByExtension(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.Expression.ToStringContains("ThenBy") &&
            semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol &&
               methodSymbol.Name == "ThenBy" &&
               methodSymbol.MethodKind == MethodKind.ReducedExtension &&
               MethodIsOnIOrderedEnumerable(methodSymbol);

        private static bool MethodIsOnIOrderedEnumerable(IMethodSymbol methodSymbol) =>
            methodSymbol.ReceiverType is INamedTypeSymbol receiverType &&
               receiverType.ConstructedFrom.ContainingNamespace.ToString() == "System.Linq" &&
               receiverType.ConstructedFrom.MetadataName == "IOrderedEnumerable`1";
    }
}
