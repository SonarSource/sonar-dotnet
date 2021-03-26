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
    public abstract class EncryptionAlgorithmsShouldBeSecureBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S5542";
        private const string MessageFormat = "Use secure mode and padding scheme.";

        protected abstract TrackerBase<TSyntaxKind, PropertyAccessContext>.Condition IsInsideObjectInitializer();
        protected abstract TrackerBase<TSyntaxKind, InvocationContext>.Condition HasPkcs1PaddingArgument();

        protected EncryptionAlgorithmsShouldBeSecureBase(IAnalyzerConfiguration configuration)
            : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var inv = Language.Tracker.Invocation;
            inv.Track(input,
                inv.MatchMethod(
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "Encrypt"),
                    new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "TryEncrypt")),
                inv.Or(
                    inv.ArgumentIsBoolConstant("fOAEP", false),
                    HasPkcs1PaddingArgument()));

            // There exist no GCM mode with AesManaged, so any mode we set will be insecure. We do not raise
            // when inside an ObjectInitializerExpression, as the issue is already raised on the constructor
            var pa = Language.Tracker.PropertyAccess;
            pa.Track(input,
                pa.MatchProperty(new MemberDescriptor(KnownType.System_Security_Cryptography_AesManaged, "Mode")),
                pa.MatchSetter(),
                pa.ExceptWhen(IsInsideObjectInitializer()));

            var oc = Language.Tracker.ObjectCreation;
            oc.Track(input, oc.MatchConstructor(KnownType.System_Security_Cryptography_AesManaged));
        }
    }
}
