/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeclareTypesInNamespaces : DeclareTypesInNamespacesBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SyntaxKind[] SyntaxKinds { get; } =
    [
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKind.EnumDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKindEx.RecordDeclaration,
        SyntaxKindEx.RecordStructDeclaration,
    ];

    protected override SyntaxToken ResolveTypeIdentifier(SyntaxNode declaration) =>
        ((BaseTypeDeclarationSyntax)declaration).Identifier;

    protected override bool IsInnerTypeOrWithinNamespace(SyntaxNode declaration, SemanticModel model) =>
        declaration.Parent is TypeDeclarationSyntax
        || declaration.Parent.Kind() is SyntaxKind.NamespaceDeclaration or SyntaxKindEx.FileScopedNamespaceDeclaration;

    protected override bool IsException(SyntaxNode node, SemanticModel model) =>
        IsTopLevelStatementPartialProgramClass(node, model);

    private static bool IsTopLevelStatementPartialProgramClass(SyntaxNode declaration, SemanticModel model) =>
        declaration is ClassDeclarationSyntax { Identifier.Text: "Program" } classDeclaration
        && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
        && model.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol symbol
        && symbol.IsTopLevelProgram;
}
