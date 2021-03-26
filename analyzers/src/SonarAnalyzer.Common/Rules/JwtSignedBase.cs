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

using System;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class JwtSignedBase<TSyntaxKind, TInvocationSyntax> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TInvocationSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S5659";
        protected const bool JwtBuilderConstructorIsSafe = false;
        private const string MessageFormat = "Use only strong cipher algorithms when verifying the signature of this JWT.";
        private const int ExtensionStaticCallParameters = 2;

        protected abstract BuilderPatternCondition<TSyntaxKind, TInvocationSyntax> CreateBuilderPatternCondition();

        protected JwtSignedBase(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var t = Language.Tracker.Invocation;
            t.Track(input,
                t.MatchMethod(
                    new MemberDescriptor(KnownType.JWT_IJwtDecoder, "Decode"),
                    new MemberDescriptor(KnownType.JWT_IJwtDecoder, "DecodeToObject")),
                t.Or(
                    t.ArgumentIsBoolConstant("verify", false),
                    t.MethodHasParameters(1)));

            t.Track(input,
                t.MatchMethod(
                    new MemberDescriptor(KnownType.JWT_JwtDecoderExtensions, "Decode"),
                    new MemberDescriptor(KnownType.JWT_JwtDecoderExtensions, "DecodeToObject")),
                t.Or(
                    t.ArgumentIsBoolConstant("verify", false),
                    t.MethodHasParameters(1),
                    t.MethodHasParameters(ExtensionStaticCallParameters)));

            t.Track(input,
                t.MatchMethod(new MemberDescriptor(KnownType.JWT_Builder_JwtBuilder, "Decode")),
                t.IsInvalidBuilderInitialization(CreateBuilderPatternCondition()));
        }

        protected BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>[] JwtBuilderDescriptors(Func<TInvocationSyntax, bool> singleArgumentIsNotFalseLiteral) =>
            new[]
            {
                new BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>(true, Language.Tracker.Invocation.MethodNameIs("MustVerifySignature")),
                new BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>(false, Language.Tracker.Invocation.MethodNameIs("DoNotVerifySignature")),
                new BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>(singleArgumentIsNotFalseLiteral, Language.Tracker.Invocation.MethodNameIs("WithVerifySignature"))
            };
    }
}
