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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class EncryptionAlgorithmsShouldBeSecureBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S5542";
        private const string MessageFormat = "Use secure mode and padding scheme.";

        private readonly IAnalyzerConfiguration configuration;
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract TrackerBase<TSyntaxKind, PropertyAccessContext>.Condition IsInsideObjectInitializer();
        protected abstract TrackerBase<TSyntaxKind, InvocationContext>.Condition HasPkcs1PaddingArgument();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected EncryptionAlgorithmsShouldBeSecureBase(IAnalyzerConfiguration configuration, System.Resources.ResourceManager rspecResources)
        {
            this.configuration = configuration;
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            var input = new TrackerInput(context, configuration, rule);
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
