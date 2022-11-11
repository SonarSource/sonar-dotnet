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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class MultipleVariableDeclarationCodeFix : MultipleVariableDeclarationCodeFixBase
    {
        protected override SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node)
        {
            if (node is not VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration })
            {
                return root;
            }

            var newDeclarations = declaration.Variables.Select(variable => CreateNewDeclaration(variable, declaration));

            IEnumerable<SyntaxNode> newNodes;

            if (declaration.Parent is not FieldDeclarationSyntax fieldDeclaration)
            {
                if (declaration.Parent is not LocalDeclarationStatementSyntax localDeclaration)
                {
                    return root;
                }

                newNodes = newDeclarations.Select(decl => SyntaxFactory.LocalDeclarationStatement(localDeclaration.Modifiers, decl));
            }
            else
            {
                newNodes = newDeclarations.Select(decl => SyntaxFactory.FieldDeclaration(fieldDeclaration.AttributeLists, fieldDeclaration.Modifiers, decl));
            }

            return root.ReplaceNode(declaration.Parent, newNodes);
        }

        private static VariableDeclarationSyntax CreateNewDeclaration(VariableDeclaratorSyntax variable, VariableDeclarationSyntax declaration) =>
            SyntaxFactory.VariableDeclaration(
                declaration.Type.WithoutTrailingTrivia(),
                SyntaxFactory.SeparatedList(new[] { variable.WithLeadingTrivia(GetLeadingTriviaFor(variable)) }));

        private static IEnumerable<SyntaxTrivia> GetLeadingTriviaFor(VariableDeclaratorSyntax variable) =>
            variable.GetFirstToken().GetPreviousToken().TrailingTrivia.Concat(variable.GetLeadingTrivia());
    }
}
