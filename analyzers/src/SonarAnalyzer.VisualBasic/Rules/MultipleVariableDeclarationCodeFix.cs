/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.VisualBasic.Rules;

[ExportCodeFixProvider(LanguageNames.VisualBasic)]
public sealed class MultipleVariableDeclarationCodeFix : MultipleVariableDeclarationCodeFixBase
{
    protected override SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node) =>
        node is ModifiedIdentifierSyntax { Parent: VariableDeclaratorSyntax declarator }
            ? root.ReplaceNode(declarator.Parent, CreateNewNodes(declarator))
            : root;

    private static IEnumerable<SyntaxNode> CreateNewNodes(VariableDeclaratorSyntax declarator) =>
        declarator.Parent switch
        {
            FieldDeclarationSyntax fieldDeclaration => CreateNewNodes(fieldDeclaration),
            LocalDeclarationStatementSyntax localDeclaration => CreateNewNodes(localDeclaration),
            _ => new[] { declarator.Parent }
        };

    private static IEnumerable<SyntaxNode> CreateNewNodes(FieldDeclarationSyntax declaration) =>
        declaration.Declarators.SelectMany(x => GetConvertedDeclarators(x).Select(declarator => CreateFieldDeclarationSyntax(declaration, declarator)));

    private static IEnumerable<SyntaxNode> CreateNewNodes(LocalDeclarationStatementSyntax declaration) =>
        declaration.Declarators.SelectMany(x => GetConvertedDeclarators(x).Select(declarator => CreateLocalDeclarationStatementSyntax(declaration, declarator)));

    private static FieldDeclarationSyntax CreateFieldDeclarationSyntax(FieldDeclarationSyntax declaration, VariableDeclaratorSyntax declarator) =>
        SyntaxFactory.FieldDeclaration(declaration.AttributeLists, declaration.Modifiers, SyntaxFactory.SeparatedList(new[] { declarator }));

    private static LocalDeclarationStatementSyntax CreateLocalDeclarationStatementSyntax(LocalDeclarationStatementSyntax declaration, VariableDeclaratorSyntax declarator) =>
        SyntaxFactory.LocalDeclarationStatement(declaration.Modifiers, SyntaxFactory.SeparatedList(new[] { declarator }));

    private static IEnumerable<VariableDeclaratorSyntax> GetConvertedDeclarators(VariableDeclaratorSyntax declarator)
    {
        var declarators = declarator.Names.Select(x => SyntaxFactory.VariableDeclarator(SyntaxFactory.SeparatedList(new[] { x }), declarator.AsClause, null)).ToList();
        if (declarator.Initializer != null)
        {
            var last = declarators.Last();
            last = last.WithInitializer(declarator.Initializer);
            declarators[declarators.Count - 1] = last;
        }

        return declarators.Select(x => x.WithTrailingTrivia(SyntaxFactory.EndOfLineTrivia(Environment.NewLine)).WithAdditionalAnnotations(Formatter.Annotation));
    }
}
