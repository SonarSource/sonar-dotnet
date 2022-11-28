/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
    public sealed class RedundantToCharArrayCall : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3456";
        private const string MessageFormat = "Remove this redundant 'ToCharArray' call.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ImmutableHashSet<string> MethodNames = ImmutableHashSet.Create("ToArray", "ToCharArray");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;

                    if ((invocation.Parent is ElementAccessExpressionSyntax || invocation.Parent is ForEachStatementSyntax)
                        && invocation.Expression is MemberAccessExpressionSyntax memberAccess
                        && MethodNames.Contains(memberAccess.Name.Identifier.ValueText)
                        && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                        && MethodNames.Contains(methodSymbol.Name)
                        && (methodSymbol.IsInType(KnownType.System_String) || methodSymbol.IsInType(KnownType.System_ReadOnlySpan_T))
                        && methodSymbol.Parameters.Length == 0)
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, memberAccess.Name.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);
    }
}
