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
                        IsMethodOrderByExtension(outerInvocation, c.SemanticModel) &&
                        IsOrderByOrThenBy(innerInvocation, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, memberAccess.Name.GetLocation()));
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
