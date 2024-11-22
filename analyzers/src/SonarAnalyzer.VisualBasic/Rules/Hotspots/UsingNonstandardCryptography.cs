/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class UsingNonstandardCryptography : UsingNonstandardCryptographyBase<SyntaxKind, TypeBlockSyntax>
    {
        protected override ILanguageFacade Language => VisualBasicFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } = { SyntaxKind.ClassBlock, SyntaxKind.InterfaceBlock };

        public UsingNonstandardCryptography() : this(AnalyzerConfiguration.Hotspot) { }

        public UsingNonstandardCryptography(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        protected override INamedTypeSymbol DeclaredSymbol(TypeBlockSyntax typeDeclarationSyntax, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(typeDeclarationSyntax);

        protected override Location Location(TypeBlockSyntax typeDeclarationSyntax) =>
            typeDeclarationSyntax switch
            {
                ClassBlockSyntax c => c.ClassStatement.Identifier.GetLocation(),
                InterfaceBlockSyntax i => i.InterfaceStatement.Identifier.GetLocation(),
                _ => null,
            };

        protected override bool DerivesOrImplementsAny(TypeBlockSyntax typeDeclarationSyntax) =>
            typeDeclarationSyntax.Implements.Any() || typeDeclarationSyntax.Inherits.Any();
    }
}
