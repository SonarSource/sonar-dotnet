/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Rules;

public abstract class EncryptionAlgorithmsShouldBeSecureBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S5542";

    protected abstract TrackerBase<TSyntaxKind, PropertyAccessContext>.Condition IsInsideObjectInitializer();
    protected abstract TrackerBase<TSyntaxKind, InvocationContext>.Condition HasPkcs1PaddingArgument();
    protected abstract SyntaxNode InvocationReceiver(SyntaxNode invocation);

    protected override string MessageFormat => "Use secure mode and padding scheme.{0}";

    protected EncryptionAlgorithmsShouldBeSecureBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        Initialize(new TrackerInput(context, Rule));

    private void Initialize(TrackerInput input)
    {
        var inv = Language.Tracker.Invocation;
        inv.Track(
            input,
            [string.Empty],
            inv.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "Encrypt"),
                new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "TryEncrypt")),
            inv.Or(
                inv.ArgumentIsBoolConstant("fOAEP", false),
                HasPkcs1PaddingArgument()));

        // There exist no GCM mode with AesManaged, so any mode we set will be insecure. We do not raise
        // when inside an ObjectInitializerExpression, as the issue is already raised on the constructor
        var pa = Language.Tracker.PropertyAccess;
        pa.Track(
            input,
            [string.Empty],
            pa.MatchProperty(new MemberDescriptor(KnownType.System_Security_Cryptography_AesManaged, "Mode")),
            pa.MatchSetter(),
            pa.ExceptWhen(IsInsideObjectInitializer()));

        var oc = Language.Tracker.ObjectCreation;
        oc.Track(input, [string.Empty], oc.MatchConstructor(KnownType.System_Security_Cryptography_AesManaged));

        var signData = inv.MatchMethod(
            new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "SignData"),
            new MemberDescriptor(KnownType.System_Security_Cryptography_RSA, "SignHash"));
        var receiverIsRSACryptoServiceProvider = ReceiverTypeIs(KnownType.System_Security_Cryptography_RSACryptoServiceProvider);

        inv.Track(
            input,
            [" RSACryptoServiceProvider does not support RSASSA-PSS, switch to RSACng."],
            signData,
            HasPkcs1PaddingArgument(),
            receiverIsRSACryptoServiceProvider);

        // For any other concrete receiver type (e.g. RSACng), RSASSA-PSS is a simple, reachable fix on any TFM.
        // For the abstract RSA type itself (RSA.Create(), or an RSA-typed parameter/field), the concrete
        // implementation is unknown, so we fall back to the documented per-TFM default: CAPI-only (no PSS) on
        // .NET Framework, PSS-capable on modern .NET.
        inv.Track(
            input,
            [string.Empty],
            signData,
            HasPkcs1PaddingArgument(),
            inv.ExceptWhen(receiverIsRSACryptoServiceProvider),
            inv.ExceptWhen(x => ReceiverTypeIs(KnownType.System_Security_Cryptography_RSA)(x) && x.Model.Compilation.IsNetFrameworkTarget));
    }

    private TrackerBase<TSyntaxKind, InvocationContext>.Condition ReceiverTypeIs(KnownType knownType) =>
        x => InvocationReceiver(x.Node) is { } receiver
                && x.Model.GetTypeInfo(receiver).Type.Is(knownType);
}
