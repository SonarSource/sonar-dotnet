/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Core.Rules
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
            [
                new BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>(true, Language.Tracker.Invocation.MethodNameIs("MustVerifySignature")),
                new BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>(false, Language.Tracker.Invocation.MethodNameIs("DoNotVerifySignature")),
                new BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>(singleArgumentIsNotFalseLiteral, Language.Tracker.Invocation.MethodNameIs("WithVerifySignature"))
            ];
    }
}
