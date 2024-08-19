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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DeclareTypesInNamespaces : DeclareTypesInNamespacesBase<SyntaxKind>
    {
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
            declaration.Parent.IsAnyKind(
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKindEx.FileScopedNamespaceDeclaration);

        protected override bool IsException(SyntaxNode node) =>
            IsTopLevelStatementPartialProgramClass(node);

        private static bool IsTopLevelStatementPartialProgramClass(SyntaxNode declaration) =>
            declaration is ClassDeclarationSyntax { Identifier.Text: "Program" } classDeclaration
            && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
            && declaration.Parent is CompilationUnitSyntax compilationUnit
            && compilationUnit.IsTopLevelMain();
    }
}
