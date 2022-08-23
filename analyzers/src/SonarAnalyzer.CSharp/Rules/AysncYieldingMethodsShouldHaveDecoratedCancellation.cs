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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncYieldingMethodsShouldHaveDecoratedCancellation : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4581"; // TODO
    private const string MessageFormat = "Provide a cancellation token that is decorated with the EnumeratorCancellation attribute.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSyntaxNodeActionInNonGenerated(
            c =>
            {
                var method = (MethodDeclarationSyntax)c.Node;
                if (method.Modifiers.AnyOfKind(SyntaxKind.AsyncKeyword)
                    && BodyYields(method)
                    && c.SemanticModel.GetDeclaredSymbol(c.Node) is IMethodSymbol methodSymbol
                    && methodSymbol.ReturnType.Is(KnownType.System_Collections_Generic_IAsyncEnumerable_T)
                    && !HasDecoratedCancelationToken(methodSymbol))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            SyntaxKind.MethodDeclaration);

    private static bool BodyYields(MethodDeclarationSyntax method) =>
        method.Body?.DescendantTokens().Any(x => x.IsKind(SyntaxKind.YieldKeyword)) == true;

    private static bool HasDecoratedCancelationToken(IMethodSymbol method)
        => method.Parameters.Any(x => x.IsType(KnownType.System_Threading_CancellationToken)
        && x.HasAttribute(KnownType.System_Runtime_CompilerServices_EnumeratorCancellationAttribute));
}
