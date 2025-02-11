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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DeclareTypesInNamespaces : DeclareTypesInNamespacesBase<SyntaxKind>
    {
        private static readonly HashSet<SyntaxKind> InnerTypeKinds =
            [
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKindEx.FileScopedNamespaceDeclaration
            ];

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
        };

        protected override SyntaxToken GetTypeIdentifier(SyntaxNode declaration) =>
            ((BaseTypeDeclarationSyntax)declaration).Identifier;

        protected override bool IsInnerTypeOrWithinNamespace(SyntaxNode declaration, SemanticModel semanticModel) =>
            declaration.Parent.IsAnyKind(InnerTypeKinds);

        protected override bool IsException(SyntaxNode node) =>
            IsTopLevelStatementPartialProgramClass(node);

        private static bool IsTopLevelStatementPartialProgramClass(SyntaxNode declaration) =>
            declaration is ClassDeclarationSyntax { Identifier.Text: "Program" } classDeclaration
            && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
            && declaration.Parent is CompilationUnitSyntax compilationUnit
            && compilationUnit.IsTopLevelMain();
    }
}
