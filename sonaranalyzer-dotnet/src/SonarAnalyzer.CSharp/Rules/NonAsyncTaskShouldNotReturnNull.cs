/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NonAsyncTaskShouldNotReturnNull : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4586";
        private const string MessageFormat = "Do not return null from this method, instead return 'Task.FromResult<T>(null)', " +
            "'Task.CompletedTask' or 'Task.Delay(0)'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> TaskTypes =
            new HashSet<KnownType>
            {
                KnownType.System_Threading_Tasks_Task,
                KnownType.System_Threading_Tasks_Task_T
            };

        private static readonly ISet<SyntaxKind> TrackedNullLiteralLocations =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.ArrowExpressionClause,
                SyntaxKind.ReturnStatement,
            };

        private static readonly ISet<SyntaxKind> PossibleEnclosingSyntax =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.VariableDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.MethodDeclaration
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var nullLiteral = (LiteralExpressionSyntax)c.Node;

                    if (!nullLiteral.GetFirstNonParenthesizedParent().IsAnyKind(TrackedNullLiteralLocations))
                    {
                        return;
                    }

                    var enclosingMember = GetEnclosingMember(nullLiteral);
                    if (enclosingMember == null ||
                        enclosingMember.IsKind(SyntaxKind.VariableDeclaration))
                    {
                        return;
                    }

                    var enclosingMemberSymbol = c.SemanticModel.GetDeclaredSymbol(enclosingMember);

                    if (enclosingMemberSymbol == null ||
                        !IsTaskReturnType(enclosingMemberSymbol) ||
                        IsSafeTaskReturnType(enclosingMemberSymbol))
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, nullLiteral.GetLocation()));
                },
                SyntaxKind.NullLiteralExpression);
        }

        private static SyntaxNode GetEnclosingMember(LiteralExpressionSyntax literal)
        {
            foreach (var ancestor in literal.Ancestors())
            {
                if (ancestor.IsKind(SyntaxKind.ParenthesizedLambdaExpression) ||
                    ancestor.IsKind(SyntaxKindEx.LocalFunctionStatement))
                {
                    return null;
                }
                else if (ancestor.IsAnyKind(PossibleEnclosingSyntax))
                {
                    return ancestor;
                }
                else
                {
                    // do nothing
                }
            }

            return null;
        }

        private static bool IsTaskReturnType(ISymbol symbol) =>
            GetReturnType(symbol) is INamedTypeSymbol namedTypeSymbol &&
            namedTypeSymbol.ConstructedFrom.DerivesFromAny(TaskTypes);

        private static ITypeSymbol GetReturnType(ISymbol symbol) =>
            symbol is IMethodSymbol methodSymbol
            ? methodSymbol.ReturnType
            : symbol.GetSymbolType();

        private bool IsSafeTaskReturnType(ISymbol enclosingMemberSymbol) =>
            // IMethodSymbol also handles lambdas
            enclosingMemberSymbol is IMethodSymbol methodSymbol &&
            methodSymbol.IsAsync;
    }
}
