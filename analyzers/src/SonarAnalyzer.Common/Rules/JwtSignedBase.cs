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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class JwtSignedBase<TSyntaxKind, TInvocationSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S5659";
        protected const bool JwtBuilderConstructorIsSafe = false;
        private const string MessageFormat = "Use only strong cipher algorithms when {0} this JWT.";
        private const string MessageVerifying = "verifying the signature of";
        private const int ExtensionStaticCallParameters = 2;

        protected abstract BuilderPatternCondition<TInvocationSyntax> CreateBuilderPatternCondition();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(VerifyingRule);
        protected DiagnosticDescriptor VerifyingRule { get; }
        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected JwtSignedBase(System.Resources.ResourceManager rspecResources) =>
            VerifyingRule = DiagnosticDescriptorBuilder
                .GetDescriptor(DiagnosticId, string.Format(MessageFormat, MessageVerifying), rspecResources)
                .WithNotConfigurable();

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.JWT_IJwtDecoder, "Decode"),
                    new MemberDescriptor(KnownType.JWT_IJwtDecoder, "DecodeToObject")),
                Conditions.Or(
                    InvocationTracker.ArgumentIsBoolConstant("verify", false),
                    InvocationTracker.MethodHasParameters(1)
                    ));

            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.JWT_JwtDecoderExtensions, "Decode"),
                    new MemberDescriptor(KnownType.JWT_JwtDecoderExtensions, "DecodeToObject")),
                Conditions.Or(
                    InvocationTracker.ArgumentIsBoolConstant("verify", false),
                    InvocationTracker.MethodHasParameters(1),
                    InvocationTracker.MethodHasParameters(ExtensionStaticCallParameters)
                    ));

            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(new MemberDescriptor(KnownType.JWT_Builder_JwtBuilder, "Decode")),
                InvocationTracker.IsInvalidBuilderInitialization(CreateBuilderPatternCondition())
                );
        }

        protected BuilderPatternDescriptor<TInvocationSyntax>[] JwtBuilderDescriptors(Func<TInvocationSyntax, bool> singleArgumentIsNotFalseLiteral) =>
            new[]
            {
                new BuilderPatternDescriptor<TInvocationSyntax>(true, InvocationTracker.MethodNameIs("MustVerifySignature")),
                new BuilderPatternDescriptor<TInvocationSyntax>(false, InvocationTracker.MethodNameIs("DoNotVerifySignature")),
                new BuilderPatternDescriptor<TInvocationSyntax>(singleArgumentIsNotFalseLiteral, InvocationTracker.MethodNameIs("WithVerifySignature"))
            };
    }
}
