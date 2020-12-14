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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class NonStandardCryptographicAlgorithmsShouldNotBeUsedBase<TSyntaxKind, TTypeDeclarationSyntax> : HotspotDiagnosticAnalyzer
        where TTypeDeclarationSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S2257";

        protected const string MessageFormat = "Make sure using a non-standard cryptographic algorithm is safe here.";

        private readonly ImmutableArray<KnownType> nonInheritableClassesAndInterfaces = ImmutableArray.Create<KnownType>(
            new[]
            {
                KnownType.System_Security_Cryptography_AsymmetricAlgorithm,
                KnownType.System_Security_Cryptography_AsymmetricKeyExchangeDeformatter,
                KnownType.System_Security_Cryptography_AsymmetricKeyExchangeFormatter,
                KnownType.System_Security_Cryptography_AsymmetricSignatureDeformatter,
                KnownType.System_Security_Cryptography_AsymmetricSignatureFormatter,
                KnownType.System_Security_Cryptography_DeriveBytes,
                KnownType.System_Security_Cryptography_HashAlgorithm,
                KnownType.System_Security_Cryptography_ICryptoTransform,
                KnownType.System_Security_Cryptography_SymmetricAlgorithm,
            });

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract DiagnosticDescriptor Rule { get; }

        protected abstract Location GetLocation(TTypeDeclarationSyntax typeDeclarationSyntax);

        protected abstract bool DerivesOrImplementsAny(TTypeDeclarationSyntax typeDeclarationSyntax);

        protected abstract INamedTypeSymbol GetDeclaredSymbol(TTypeDeclarationSyntax typeDeclarationSyntax, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected NonStandardCryptographicAlgorithmsShouldNotBeUsedBase(IAnalyzerConfiguration analyzerConfiguration)
            : base(analyzerConfiguration)
        {
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer, AnalizeDeclaration, SyntaxKinds);

        private void AnalizeDeclaration(SyntaxNodeAnalysisContext analysisContext)
        {
            var declaration = (TTypeDeclarationSyntax)analysisContext.Node;
            if (!DerivesOrImplementsAny(declaration))
            {
                return;
            }

            var classSymbol = GetDeclaredSymbol(declaration, analysisContext.SemanticModel);
            if (classSymbol.DerivesOrImplementsAny(nonInheritableClassesAndInterfaces))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, GetLocation(declaration)));
            }
        }
    }
}
