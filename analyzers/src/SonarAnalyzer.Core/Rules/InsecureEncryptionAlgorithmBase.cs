/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Rules
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
