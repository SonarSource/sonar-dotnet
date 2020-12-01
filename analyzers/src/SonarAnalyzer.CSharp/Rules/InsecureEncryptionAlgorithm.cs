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
    [Rule(S2278DiagnosticId)]
    [Rule(S5547DiagnosticId)]
    public sealed class InsecureEncryptionAlgorithm : DoNotCallInsecureSecurityAlgorithm
    {
        // S2278 was deprecated in favor of S5547. Technically, there is no difference in the C# analyzer between
        // the 2 rules, but to be coherent with all the other languages, we still replace it with the new one
        private const string S2278DiagnosticId = "S2278";
        private const string S2278MessageFormat = "Use the recommended AES (Advanced Encryption Standard) instead.";

        private const string S5547DiagnosticId = "S5547";
        private const string S5547MessageFormat = "Use a strong cipher algorithm.";

        private static readonly DiagnosticDescriptor S2278 =
            DiagnosticDescriptorBuilder.GetDescriptor(S2278DiagnosticId, S2278MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor S5547 =
            DiagnosticDescriptorBuilder.GetDescriptor(S5547DiagnosticId, S5547MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(S2278, S5547);

        private static readonly ImmutableArray<KnownType> algorithmTypes =
            ImmutableArray.Create(
                KnownType.System_Security_Cryptography_DES,
                KnownType.System_Security_Cryptography_TripleDES,
                KnownType.System_Security_Cryptography_RC2
            );
        internal override ImmutableArray<KnownType> AlgorithmTypes => algorithmTypes;

        private static readonly ISet<string> algorithmParameterlessFactoryMethods = new HashSet<string>();
        protected override ISet<string> AlgorithmParameterlessFactoryMethods => algorithmParameterlessFactoryMethods;

        private static readonly ISet<string> algorithmParameteredFactoryMethods = new HashSet<string>
        {
            "System.Security.Cryptography.CryptoConfig.CreateFromName",
            "System.Security.Cryptography.SymmetricAlgorithm.Create"
        };
        protected override ISet<string> AlgorithmParameteredFactoryMethods => algorithmParameteredFactoryMethods;

        private static readonly ISet<string> factoryParameterNames =
            new HashSet<string> { "DES", "3DES", "TripleDES", "RC2" };
        protected override ISet<string> FactoryParameterNames => factoryParameterNames;
    }
}
