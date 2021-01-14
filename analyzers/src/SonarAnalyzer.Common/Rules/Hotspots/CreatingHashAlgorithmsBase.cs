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

        private const string CreateMethodName = "Create";

        private readonly KnownType[] algorithmTypes =
        {
            KnownType.System_Security_Cryptography_DSA,
            KnownType.System_Security_Cryptography_HMACMD5,
            KnownType.System_Security_Cryptography_HMACRIPEMD160,
            KnownType.System_Security_Cryptography_HMACSHA1,
            KnownType.System_Security_Cryptography_MD5,
            KnownType.System_Security_Cryptography_RIPEMD160,
            KnownType.System_Security_Cryptography_SHA1
        };

        private readonly string[] unsafeAlgorithms =
        {
            "DSA",
            "HMACMD5",
            "HMACRIPEMD160",
            "HMACSHA1",
            "MD5",
            "System.Security.Cryptography.MD5",
            "RIPEMD160",
            "RIPEMD160Managed",
            "SHA1",
            "System.Security.Cryptography.SHA1"
        };

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            ObjectCreationTracker.Track(context, ObjectCreationTracker.WhenDerivesOrImplementsAny(algorithmTypes));

            InvocationTracker.Track(context,
                                    InvocationTracker.MatchMethod(new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_MD5, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, CreateMethodName)),
                                    InvocationTracker.MethodHasParameters(0));

            InvocationTracker.Track(context,
                                    InvocationTracker.MatchMethod(new MemberDescriptor(KnownType.System_Security_Cryptography_AsymmetricAlgorithm, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_CryptoConfig, "CreateFromName"),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_HashAlgorithm, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_KeyedHashAlgorithm, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_MD5, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160, CreateMethodName),
                                                                  new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, CreateMethodName)),
                                    InvocationTracker.ArgumentAtIndexIsAny(0, unsafeAlgorithms));
        }
    }
}
