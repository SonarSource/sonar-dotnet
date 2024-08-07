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
    public sealed class UsingNonstandardCryptography : UsingNonstandardCryptographyBase<SyntaxKind, TypeDeclarationSyntax>
    {
        protected override ILanguageFacade Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.StructDeclaration
        };

        public UsingNonstandardCryptography() : this(AnalyzerConfiguration.Hotspot) { }

        public UsingNonstandardCryptography(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        protected override INamedTypeSymbol DeclaredSymbol(TypeDeclarationSyntax typeDeclarationSyntax, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(typeDeclarationSyntax);

        protected override Location Location(TypeDeclarationSyntax typeDeclarationSyntax) =>
            typeDeclarationSyntax.Identifier.GetLocation();

        protected override bool DerivesOrImplementsAny(TypeDeclarationSyntax typeDeclarationSyntax) =>
            typeDeclarationSyntax.BaseList != null && typeDeclarationSyntax.BaseList.Types.Any();
    }
}
