﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class ArrayDesignatorOnVariableCodeFix : SonarCodeFix
    {
        internal const string Title = "Move the array designator to the type";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ArrayDesignatorOnVariable.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var name = root.FindNode(diagnosticSpan) as ModifiedIdentifierSyntax;

            if (!(name?.Parent is VariableDeclaratorSyntax variableDeclarator) ||
                variableDeclarator.Names.Count != 1)
            {
                return Task.CompletedTask;
            }

            if (!(variableDeclarator.AsClause is SimpleAsClauseSyntax simpleAsClause))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var type = simpleAsClause.Type.WithoutTrivia();
                    var rankSpecifiers = name.ArrayRankSpecifiers.Select(rank => rank.WithoutTrivia());
                    var newType = !(type is ArrayTypeSyntax typeAsArrayType)
                        ? SyntaxFactory.ArrayType(
                                    type,
                                    SyntaxFactory.List(rankSpecifiers))
                        : typeAsArrayType.AddRankSpecifiers(rankSpecifiers.ToArray());

                    newType = newType.WithTriviaFrom(simpleAsClause.Type);

                    var newVariableDeclarator = variableDeclarator
                        .WithNames(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.ModifiedIdentifier(name.Identifier, name.ArrayBounds).WithTriviaFrom(name)
                        }))
                        .WithAsClause(simpleAsClause.WithType(newType));

                    var newRoot = root.ReplaceNode(
                        variableDeclarator,
                        newVariableDeclarator);

                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
