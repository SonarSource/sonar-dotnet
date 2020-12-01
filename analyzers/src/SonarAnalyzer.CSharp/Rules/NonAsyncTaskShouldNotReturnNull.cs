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
    public sealed class NonAsyncTaskShouldNotReturnNull : NonAsyncTaskShouldNotReturnNullBase
    {
        private const string MessageFormat = "Do not return null from this method, instead return 'Task.FromResult<T>(null)', " +
            "'Task.CompletedTask' or 'Task.Delay(0)'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> TrackedNullLiteralLocations =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.ArrowExpressionClause,
                SyntaxKind.ReturnStatement
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
                    if (enclosingMember != null &&
                        !enclosingMember.IsKind(SyntaxKind.VariableDeclaration) &&
                        IsInvalidEnclosingSymbolContext(enclosingMember, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, nullLiteral.GetLocation()));
                    }
                },
                SyntaxKind.NullLiteralExpression);
        }

        private static SyntaxNode GetEnclosingMember(LiteralExpressionSyntax literal)
        {
            foreach (var ancestor in literal.Ancestors())
            {
                switch (ancestor.Kind())
                {
                    case SyntaxKind.ParenthesizedLambdaExpression:
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.VariableDeclaration:
                    case SyntaxKind.PropertyDeclaration:
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKindEx.LocalFunctionStatement:
                        return ancestor;
                }
            }

            return null;
        }
    }
}
