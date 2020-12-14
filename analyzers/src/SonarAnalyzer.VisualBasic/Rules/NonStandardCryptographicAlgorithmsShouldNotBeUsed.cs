/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class NonStandardCryptographicAlgorithmsShouldNotBeUsed : NonStandardCryptographicAlgorithmsShouldNotBeUsedBase<SyntaxKind, TypeBlockSyntax>
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } = new SyntaxKind[] { SyntaxKind.ClassBlock, SyntaxKind.InterfaceBlock };

        protected override DiagnosticDescriptor Rule { get; } = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public NonStandardCryptographicAlgorithmsShouldNotBeUsed()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        public NonStandardCryptographicAlgorithmsShouldNotBeUsed(IAnalyzerConfiguration analyzerConfiguration)
            : base(analyzerConfiguration)
        {
        }

        protected override INamedTypeSymbol GetDeclaredSymbol(TypeBlockSyntax typeDeclarationSyntax, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(typeDeclarationSyntax);

        protected override Location GetLocation(TypeBlockSyntax typeDeclarationSyntax) =>
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
