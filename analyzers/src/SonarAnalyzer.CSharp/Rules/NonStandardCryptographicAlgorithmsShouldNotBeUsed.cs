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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NonStandardCryptographicAlgorithmsShouldNotBeUsed : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2257";

        private const string MessageFormat = "Make sure using a non-standard cryptographic algorithm is safe here.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly ImmutableArray<KnownType> NonInheritableClassesAndInterfaces = ImmutableArray.Create<KnownType>(
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(AnalizeDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration);

        private void AnalizeDeclaration(SyntaxNodeAnalysisContext analysisContext)
        {
            var declaration = (BaseTypeDeclarationSyntax)analysisContext.Node;
            if (declaration.BaseList == null
                || !declaration.BaseList.Types.Any())
            {
                return;
            }

            var classSymbol = analysisContext.SemanticModel.GetDeclaredSymbol(declaration);
            if (classSymbol.DerivesOrImplementsAny(NonInheritableClassesAndInterfaces))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, declaration.Identifier.GetLocation()));
            }
        }
    }
}
