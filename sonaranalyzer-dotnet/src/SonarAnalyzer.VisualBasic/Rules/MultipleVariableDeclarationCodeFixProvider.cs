/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Rules.Common;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class MultipleVariableDeclarationCodeFixProvider : MultipleVariableDeclarationCodeFixProviderBase
    {
        protected override SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node)
        {
            if (!(node is ModifiedIdentifierSyntax identifier))
            {
                return root;
            }

            if (!(identifier.Parent is VariableDeclaratorSyntax declarator))
            {
                return root;
            }

            IEnumerable<SyntaxNode> newNodes;

            if (!(declarator.Parent is FieldDeclarationSyntax fieldDeclaration))
            {
                if (!(declarator.Parent is LocalDeclarationStatementSyntax localDeclaration))
                {
                    return root;
                }

                newNodes = localDeclaration.Declarators.SelectMany(decl =>
                    GetConvertedDeclarators(decl).Select(newDecl =>
                        SyntaxFactory.LocalDeclarationStatement(
                            localDeclaration.Modifiers,
                            SyntaxFactory.SeparatedList(new[] { newDecl }))));
            }
            else
            {
                newNodes = fieldDeclaration.Declarators.SelectMany(decl =>
                    GetConvertedDeclarators(decl).Select(newDecl =>
                        SyntaxFactory.FieldDeclaration(
                            fieldDeclaration.AttributeLists,
                            fieldDeclaration.Modifiers,
                            SyntaxFactory.SeparatedList(new[] { newDecl }))));
            }

            return root.ReplaceNode(declarator.Parent, newNodes);
        }

        private static IEnumerable<VariableDeclaratorSyntax> GetConvertedDeclarators(VariableDeclaratorSyntax declarator)
        {
            var declarators = declarator.Names.Select(n =>
                SyntaxFactory.VariableDeclarator(
                    SyntaxFactory.SeparatedList(new[] { n }),
                    declarator.AsClause,
                    null))
                    .ToList();

            if (declarator.Initializer != null)
            {
                var last = declarators.Last();
                last = last.WithInitializer(declarator.Initializer);
                declarators[declarators.Count - 1] = last;
            }

            return declarators.Select(d =>
                d.WithTrailingTrivia(SyntaxFactory.EndOfLineTrivia(Environment.NewLine))
                    .WithAdditionalAnnotations(Formatter.Annotation));
        }
    }
}
