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

namespace SonarAnalyzer.Rules
{
    public abstract class UsingNonstandardCryptographyBase<TSyntaxKind, TTypeDeclarationSyntax> : HotspotDiagnosticAnalyzer
        where TTypeDeclarationSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S2257";
        private const string MessageFormat = "Make sure using a non-standard cryptographic algorithm is safe here.";

        private readonly ImmutableArray<KnownType> nonInheritableClassesAndInterfaces = ImmutableArray.Create(
            KnownType.System_Security_Cryptography_AsymmetricAlgorithm,
            KnownType.System_Security_Cryptography_AsymmetricKeyExchangeDeformatter,
            KnownType.System_Security_Cryptography_AsymmetricKeyExchangeFormatter,
            KnownType.System_Security_Cryptography_AsymmetricSignatureDeformatter,
            KnownType.System_Security_Cryptography_AsymmetricSignatureFormatter,
            KnownType.System_Security_Cryptography_DeriveBytes,
            KnownType.System_Security_Cryptography_HashAlgorithm,
            KnownType.System_Security_Cryptography_ICryptoTransform,
            KnownType.System_Security_Cryptography_SymmetricAlgorithm);
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }
        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract Location Location(TTypeDeclarationSyntax typeDeclarationSyntax);
        protected abstract bool DerivesOrImplementsAny(TTypeDeclarationSyntax typeDeclarationSyntax);
        protected abstract INamedTypeSymbol DeclaredSymbol(TTypeDeclarationSyntax typeDeclarationSyntax, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected UsingNonstandardCryptographyBase(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    var declaration = (TTypeDeclarationSyntax)c.Node;
                    if (!c.IsRedundantPositionalRecordContext()
                        && IsEnabled(c.Options)
                        && DerivesOrImplementsAny(declaration)
                        && DeclaredSymbol(declaration, c.SemanticModel).DerivesOrImplementsAny(nonInheritableClassesAndInterfaces))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, Location(declaration)));
                    }
                },
                SyntaxKinds);
    }
}
