/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodShouldBeNamedAccordingToSynchronicity : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4261";
        private const string MessageFormat = "{0}";
        private const string AddAsyncSuffixMessage = "Add the 'Async' suffix to the name of this method.";
        private const string RemoveAsyncSuffixMessage = "Remove the 'Async' suffix to the name of this method.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> asyncReturnTypes =
            ImmutableArray.Create(
                KnownType.System_Threading_Tasks_Task,
                KnownType.System_Threading_Tasks_Task_T,
                KnownType.System_Threading_Tasks_ValueTask, // NetCore 2.2+
                KnownType.System_Threading_Tasks_ValueTask_TResult,
                KnownType.System_Collections_Generic_IAsyncEnumerable_T
            );

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    if (methodDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                    if (methodSymbol == null ||
                        methodSymbol.IsMainMethod() ||
                        methodSymbol.GetInterfaceMember() != null ||
                        methodSymbol.GetOverriddenMember() != null ||
                        methodSymbol.IsTestMethod() ||
                        methodSymbol.IsControllerMethod() ||
                        IsSignalRHubMethod(methodSymbol))
                    {
                        return;
                    }

                    var hasAsyncReturnType = HasAsyncReturnType(methodSymbol);
                    var hasAsyncSuffix = HasAsyncSuffix(methodDeclaration);

                    if (hasAsyncSuffix && !hasAsyncReturnType)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation(), RemoveAsyncSuffixMessage));
                    }
                    else if (!hasAsyncSuffix && hasAsyncReturnType)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation(), AddAsyncSuffixMessage));
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static bool HasAsyncReturnType(IMethodSymbol methodSymbol) =>
            (methodSymbol.ReturnType as INamedTypeSymbol)?.ConstructedFrom.DerivesFromAny(asyncReturnTypes) ?? false;

        private static bool HasAsyncSuffix(MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.Identifier.ValueText.ToUpper().EndsWith("ASYNC");

        private static bool IsSignalRHubMethod(ISymbol methodSymbol) =>
            methodSymbol.GetEffectiveAccessibility() == Accessibility.Public &&
            IsSignalRHubMethod(methodSymbol.ContainingType);

        private static bool IsSignalRHubMethod(ITypeSymbol typeSymbol) =>
            typeSymbol.DerivesFrom(KnownType.Microsoft_AspNet_SignalR_Hub);
    }
}
