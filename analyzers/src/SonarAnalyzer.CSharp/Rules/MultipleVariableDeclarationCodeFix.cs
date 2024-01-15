/*
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

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class MultipleVariableDeclarationCodeFix : MultipleVariableDeclarationCodeFixBase
{
    protected override SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node) =>
        node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration }
            ? root.ReplaceNode(declaration.Parent, CreateNewNodes(declaration))
            : root;

    private static IEnumerable<SyntaxNode> CreateNewNodes(VariableDeclarationSyntax declaration)
    {
        var newDeclarations = declaration.Variables.Select(x => CreateNewDeclaration(x, declaration));
        return declaration.Parent switch
               {
                   FieldDeclarationSyntax fieldDeclaration => newDeclarations.Select(x => SyntaxFactory.FieldDeclaration(fieldDeclaration.AttributeLists, fieldDeclaration.Modifiers, x)),
                   LocalDeclarationStatementSyntax localDeclaration => newDeclarations.Select(x => SyntaxFactory.LocalDeclarationStatement(localDeclaration.Modifiers, x)),
                   _ => new[] { declaration.Parent }
               };
    }

    private static VariableDeclarationSyntax CreateNewDeclaration(VariableDeclaratorSyntax variable, VariableDeclarationSyntax declaration) =>
        SyntaxFactory.VariableDeclaration(
            declaration.Type.WithoutTrailingTrivia(),
            SyntaxFactory.SeparatedList(new[] { variable.WithLeadingTrivia(GetLeadingTriviaFor(variable)) }));

    private static IEnumerable<SyntaxTrivia> GetLeadingTriviaFor(VariableDeclaratorSyntax variable) =>
        variable.GetFirstToken().GetPreviousToken().TrailingTrivia.Concat(variable.GetLeadingTrivia());
}
