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

using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class CreatingHashAlgorithmsBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
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
            "System.Security.Cryptography.DSA",
            "HMACMD5",
            "System.Security.Cryptography.HMACMD5",
            "HMACRIPEMD160",
            "System.Security.Cryptography.HMACRIPEMD160",
            "HMACSHA1",
            "System.Security.Cryptography.HMACSHA1",
            "MD5",
            "System.Security.Cryptography.MD5",
            "RIPEMD160",
            "System.Security.Cryptography.RIPEMD160",
            "SHA1",
            "System.Security.Cryptography.SHA1"
        };

        protected CreatingHashAlgorithmsBase(IAnalyzerConfiguration configuration)
            : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var oc = Language.Tracker.ObjectCreation;
            oc.Track(input, oc.WhenDerivesOrImplementsAny(algorithmTypes));

            var t = Language.Tracker.Invocation;
            t.Track(input,
                t.MatchMethod(
                    new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_MD5, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, CreateMethodName)),
                t.MethodHasParameters(0));

            t.Track(input,
                t.MatchMethod(
                    new MemberDescriptor(KnownType.System_Security_Cryptography_AsymmetricAlgorithm, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_CryptoConfig, "CreateFromName"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_HashAlgorithm, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_KeyedHashAlgorithm, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_MD5, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160, CreateMethodName),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, CreateMethodName)),
                t.ArgumentAtIndexIsAny(0, unsafeAlgorithms));
        }
    }
}
