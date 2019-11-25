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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Rules.Common;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class MultipleVariableDeclarationCodeFixProvider : MultipleVariableDeclarationCodeFixProviderBase
    {
        protected override SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node)
        {
            if (!(node is VariableDeclaratorSyntax declarator))
            {
                return root;
            }

            if (!(declarator.Parent is VariableDeclarationSyntax declaration))
            {
                return root;
            }

            var newDeclarations = declaration.Variables.Select(variable =>
                SyntaxFactory.VariableDeclaration(
                    declaration.Type.WithoutTrailingTrivia(),
                    SyntaxFactory.SeparatedList(new[] { variable.WithLeadingTrivia(GetLeadingTriviaFor(variable)) })));

            IEnumerable<SyntaxNode> newNodes;

            if (!(declaration.Parent is FieldDeclarationSyntax fieldDeclaration))
            {
                if (!(declaration.Parent is LocalDeclarationStatementSyntax localDeclaration))
                {
                    return root;
                }

                newNodes = newDeclarations
                    .Select(decl =>
                        SyntaxFactory.LocalDeclarationStatement(
                            localDeclaration.Modifiers,
                            decl));
            }
            else
            {
                newNodes = newDeclarations
                    .Select(decl =>
                        SyntaxFactory.FieldDeclaration(
                            fieldDeclaration.AttributeLists,
                            fieldDeclaration.Modifiers,
                            decl));
            }

            return root.ReplaceNode(declaration.Parent, newNodes);
        }

        private static IEnumerable<SyntaxTrivia> GetLeadingTriviaFor(VariableDeclaratorSyntax variable)
        {
            var previousToken = variable.GetFirstToken().GetPreviousToken();
            return previousToken.TrailingTrivia
                .Concat(variable.GetLeadingTrivia());
        }
    }
}
