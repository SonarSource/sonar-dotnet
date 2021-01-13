/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class CreatingHashAlgorithmsBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4790";
        protected const string MessageFormat = "Make sure this weak hash algorithm is not used in a sensitive context here.";

        private const string Create = "Create";

        private readonly KnownType[] algorithmTypes =
        {
            KnownType.System_Security_Cryptography_SHA1,
            KnownType.System_Security_Cryptography_MD5,
            KnownType.System_Security_Cryptography_DSA,
            KnownType.System_Security_Cryptography_RIPEMD160,
            KnownType.System_Security_Cryptography_HMACSHA1,
            KnownType.System_Security_Cryptography_HMACMD5,
            KnownType.System_Security_Cryptography_HMACRIPEMD160
        };

        private readonly string[] unsafeAlgorithms = { "SHA1", "MD5", "DSA", "HMACMD5", "HMACRIPEMD160", "RIPEMD160", "RIPEMD160Managed" };

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected InvocationTracker<TSyntaxKind> ParameterlessFactoryInvocationTracker { get; set; }

        protected InvocationTracker<TSyntaxKind> ParameterizedFactoryInvocationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            ObjectCreationTracker.Track(context, ObjectCreationTracker.WhenDerivesOrImplementsAny(algorithmTypes));

            ParameterlessFactoryInvocationTracker.Track(context,
                                                        ParameterlessFactoryInvocationTracker.MatchMethod(new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_MD5, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, Create)),
                                                        ParameterlessFactoryInvocationTracker.MethodHasParameters(0));

            ParameterizedFactoryInvocationTracker.Track(context,
                                                        ParameterizedFactoryInvocationTracker.MatchMethod(new MemberDescriptor(KnownType.System_Security_Cryptography_AsymmetricAlgorithm, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_CryptoConfig, "CreateFromName"),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_HashAlgorithm, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_KeyedHashAlgorithm, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_MD5, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160, Create),
                                                                                                          new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, Create)),
                                                        ParameterizedFactoryInvocationTracker.ArgumentAtIndexIsOneOf(0, unsafeAlgorithms));
        }
    }
}
