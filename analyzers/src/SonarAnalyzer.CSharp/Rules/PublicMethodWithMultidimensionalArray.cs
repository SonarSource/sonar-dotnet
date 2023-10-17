/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
    public sealed class PublicMethodWithMultidimensionalArray : PublicMethodWithMultidimensionalArrayBase<SyntaxKind, MethodDeclarationSyntax>
    {
        private static readonly ImmutableArray<DiagnosticDescriptor> Rule = ImmutableArray.Create(DescriptorFactory.Create(DiagnosticId, MessageFormat));
        private static readonly ImmutableArray<SyntaxKind> KindsOfInterest = ImmutableArray.Create(
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordClassDeclaration,
            SyntaxKindEx.RecordStructDeclaration);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rule;
        public override ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => KindsOfInterest;
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;
        protected override ILanguageFacade<SyntaxKind> LanguageFacade => CSharpFacade.Instance;

        protected override IMethodSymbol MethodSymbolOfNode(SemanticModel semanticModel, SyntaxNode node)
        {
            return node switch
            {
                { RawKind: (int)SyntaxKind.MethodDeclaration or (int)SyntaxKind.ConstructorDeclaration } => base.MethodSymbolOfNode(semanticModel, node),
                TypeDeclarationSyntax typeDeclaration => typeDeclaration.PrimaryConstructor(semanticModel),
            };
        }
    }
}
