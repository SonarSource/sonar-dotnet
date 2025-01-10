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

using Microsoft.CodeAnalysis.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class UnchangedLocalVariablesShouldBeConstCodeFix : SonarCodeFix
{
    private const string Title = "Convert to constant.";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(UnchangedLocalVariablesShouldBeConst.DiagnosticId);

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        if (VariableDeclaration(root, context) is
            {
                Parent: LocalDeclarationStatementSyntax oldNode,
                Variables.Count: 1, // It is not guaranteed that all should be const.
            } variable)
        {
            context.RegisterCodeFix(
                Title,
                c => ChangeDocument(context.Document, root, variable, oldNode, c),
                context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    public static async Task<Document> ChangeDocument(
        Document document,
        SyntaxNode root,
        VariableDeclarationSyntax variable,
        LocalDeclarationStatementSyntax oldNode,
        CancellationToken cancel)
    {
        var declaration = variable.Type.IsVar
            ? WithExplictType(variable, await document.GetSemanticModelAsync(cancel).ConfigureAwait(false))
            : variable;

        var newNode = root.ReplaceNode(oldNode, ConstantDeclaration(declaration));

        return document.WithSyntaxRoot(newNode);
    }

    private static VariableDeclarationSyntax VariableDeclaration(SyntaxNode root, SonarCodeFixContext context) =>
        root.FindNode(context.Diagnostics.First().Location.SourceSpan)?.Parent as VariableDeclarationSyntax;

    private static LocalDeclarationStatementSyntax ConstantDeclaration(VariableDeclarationSyntax declaration)
    {
        var prefix = TokenList(Token(SyntaxKind.ConstKeyword));
        return LocalDeclarationStatement(prefix, declaration);
    }

    private static VariableDeclarationSyntax WithExplictType(VariableDeclarationSyntax declaration, SemanticModel semanticModel)
    {
        var typeSymbol = semanticModel.GetTypeInfo(declaration.Type).Type;
        var type = typeSymbol is IErrorTypeSymbol
            ? declaration.Type
            : IdentifierName(typeSymbol.ToMinimalDisplayString(semanticModel, declaration.GetLocation().SourceSpan.Start));
        return declaration.ReplaceNode(declaration.Type, type);
    }
}
