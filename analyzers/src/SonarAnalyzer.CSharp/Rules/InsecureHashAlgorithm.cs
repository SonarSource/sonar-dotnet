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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class InsecureHashAlgorithm : DoNotCallInsecureSecurityAlgorithm
    {
        internal const string DiagnosticId = "S2070";
        private const string MessageFormat = "Use a stronger hashing/asymmetric algorithm.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> algorithmTypes =
            ImmutableArray.Create(
                KnownType.System_Security_Cryptography_SHA1,
                KnownType.System_Security_Cryptography_MD5,
                KnownType.System_Security_Cryptography_DSA,
                KnownType.System_Security_Cryptography_RIPEMD160,
                KnownType.System_Security_Cryptography_HMACSHA1,
                KnownType.System_Security_Cryptography_HMACMD5,
                KnownType.System_Security_Cryptography_HMACRIPEMD160
            );
        internal override ImmutableArray<KnownType> AlgorithmTypes => algorithmTypes;

        private static readonly ISet<string> algorithmParameterlessFactoryMethods = new HashSet<string>
        {
            "System.Security.Cryptography.HMAC.Create"
        };
        protected override ISet<string> AlgorithmParameterlessFactoryMethods => algorithmParameterlessFactoryMethods;

        private static readonly ISet<string> algorithmParameteredFactoryMethods = new HashSet<string>
        {
            "System.Security.Cryptography.CryptoConfig.CreateFromName",
            "System.Security.Cryptography.HashAlgorithm.Create",
            "System.Security.Cryptography.KeyedHashAlgorithm.Create",
            "System.Security.Cryptography.AsymmetricAlgorithm.Create",
            "System.Security.Cryptography.HMAC.Create"
        };
        protected override ISet<string> AlgorithmParameteredFactoryMethods => algorithmParameteredFactoryMethods;

        private static readonly ISet<string> factoryParameterNames =
            new HashSet<string> { "SHA1", "MD5", "DSA", "HMACMD5", "HMACRIPEMD160", "RIPEMD160",
                "RIPEMD160Managed" };
        protected override ISet<string> FactoryParameterNames => factoryParameterNames;
    }
}
