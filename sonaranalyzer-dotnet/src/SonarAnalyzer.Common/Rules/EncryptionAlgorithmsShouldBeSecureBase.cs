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
    public abstract class EncryptionAlgorithmsShouldBeSecureBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S5542";
        protected const string MessageFormat = "Use secure mode and padding scheme.";

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "Encrypt"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "TryEncrypt")),
                Conditions.Or(
                    InvocationTracker.ArgumentIsBoolConstant("fOAEP", false),
                    HasPkcs1PaddingArgument()));

            // There exist no GCM mode with AesManaged, so any mode we set will be insecure. We do not raise
            // when inside an ObjectInitializerExpression, as the issue is already raised on the constructor
            PropertyAccessTracker.Track(context,
                PropertyAccessTracker.MatchProperty(
                    new MemberDescriptor(KnownType.System_Security_Cryptography_AesManaged, "Mode")),
                PropertyAccessTracker.MatchSetter(),
                Conditions.ExceptWhen(IsInsideObjectInitializer()));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(KnownType.System_Security_Cryptography_AesManaged));
        }

        protected abstract PropertyAccessCondition IsInsideObjectInitializer();

        protected abstract InvocationCondition HasPkcs1PaddingArgument();
    }
}
