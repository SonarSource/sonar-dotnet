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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GenericInheritanceShouldNotBeRecursive : GenericInheritanceShouldNotBeRecursiveBase<SyntaxKind, TypeDeclarationSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKindEx.RecordDeclaration,
        };

        protected override SyntaxToken GetKeyword(TypeDeclarationSyntax declaration) =>
            declaration.Keyword;

        protected override Location GetLocation(TypeDeclarationSyntax declaration) =>
            declaration.Identifier.GetLocation();

        protected override INamedTypeSymbol GetNamedTypeSymbol(TypeDeclarationSyntax declaration, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(declaration);
    }
}
