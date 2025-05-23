﻿/*
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

namespace SonarAnalyzer.Rules
{
    public abstract class InsecureEncryptionAlgorithmBase<TSyntaxKind, TInvocationExpressionSyntax, TArgumentListSyntax, TArgumentSyntax>
        : DoNotCallInsecureSecurityAlgorithmBase<TSyntaxKind, TInvocationExpressionSyntax, TArgumentListSyntax, TArgumentSyntax>
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
        where TArgumentListSyntax : SyntaxNode
        where TArgumentSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S5547";
        private const string MessageFormat = "Use a strong cipher algorithm.";

        protected DiagnosticDescriptor Rule { get; }

        protected override ISet<string> AlgorithmParameterlessFactoryMethods { get; } =
            new HashSet<string>();

        protected override ISet<string> AlgorithmParameterizedFactoryMethods { get; } =
            new HashSet<string>
            {
                "System.Security.Cryptography.CryptoConfig.CreateFromName",
                "System.Security.Cryptography.SymmetricAlgorithm.Create"
            };

        protected override ISet<string> FactoryParameterNames { get; } =
            new HashSet<string>
            {
                "DES",
                "3DES",
                "TripleDES",
                "RC2"
            };

        private protected override ImmutableArray<KnownType> AlgorithmTypes { get; } =
            ImmutableArray.Create(
                KnownType.System_Security_Cryptography_DES,
                KnownType.System_Security_Cryptography_TripleDES,
                KnownType.System_Security_Cryptography_RC2,
                KnownType.Org_BouncyCastle_Crypto_Engines_AesFastEngine
            );

        protected InsecureEncryptionAlgorithmBase() =>
            Rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
    }
}
