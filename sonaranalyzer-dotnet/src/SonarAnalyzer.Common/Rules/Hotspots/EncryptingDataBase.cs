/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
    public abstract class EncryptingDataBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4787";
        protected const string MessageFormat = "Make sure that encrypting data is safe here.";

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected BaseTypeTracker<TSyntaxKind> BaseTypeTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    // "RSA" is the base class for all RSA algorithm implementations
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "Encrypt"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "EncryptValue"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "Decrypt"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "DecryptValue"),

                    // RSA methods added in NET Core 2.1
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "TryEncrypt"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "TryDecrypt"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_SymmetricAlgorithm, "CreateEncryptor"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_SymmetricAlgorithm, "CreateDecryptor")));

            BaseTypeTracker.Track(context,
                BaseTypeTracker.MatchSubclassesOf(
                    KnownType.System_Security_Cryptography_AsymmetricAlgorithm,
                    KnownType.System_Security_Cryptography_SymmetricAlgorithm));
        }
    }
}

