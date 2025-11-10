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

namespace SonarAnalyzer.Core.Rules;

public abstract class CreatingHashAlgorithmsBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S4790";
    protected const string MessageFormat = "Make sure this weak hash algorithm is not used in a sensitive context here.";

    private const string CreateMethodName = "Create";
    private const string HashDataName = "HashData";
    private const string HashDataAsyncName = "HashDataAsync";
    private const string TryHashDataName = "TryHashData";

    private readonly KnownType[] algorithmTypes =
    [
        KnownType.System_Security_Cryptography_DSA,
        KnownType.System_Security_Cryptography_HMACMD5,
        KnownType.System_Security_Cryptography_HMACRIPEMD160,
        KnownType.System_Security_Cryptography_HMACSHA1,
        KnownType.System_Security_Cryptography_MD5,
        KnownType.System_Security_Cryptography_RIPEMD160,
        KnownType.System_Security_Cryptography_SHA1
    ];

    private readonly string[] unsafeAlgorithms =
    [
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
        "System.Security.Cryptography.SHA1",
    ];

    protected abstract bool IsUnsafeAlgorithm(SyntaxNode argumentNode, SemanticModel model);

    protected CreatingHashAlgorithmsBase(IAnalyzerConfiguration configuration)
        : base(configuration, DiagnosticId, MessageFormat) { }

    protected override void Initialize(TrackerInput input)
    {
        var oc = Language.Tracker.ObjectCreation;
        oc.Track(input, oc.WhenDerivesOrImplementsAny(algorithmTypes));

        var tracker = Language.Tracker.Invocation;
        tracker.Track(
            input,
            tracker.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Cryptography_DSA,       CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC,      CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_MD5,       CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160, CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1,      CreateMethodName)),
            tracker.MethodHasParameters(0));

        tracker.Track(
            input,
            tracker.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Cryptography_AsymmetricAlgorithm, CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptoConfig,        "CreateFromName"),
                new MemberDescriptor(KnownType.System_Security_Cryptography_DSA,                 CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_HashAlgorithm,       CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_HMAC,                CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_KeyedHashAlgorithm,  CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_MD5,                 CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_RIPEMD160,           CreateMethodName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1,                CreateMethodName)),
            tracker.ArgumentAtIndexIsAny(0, unsafeAlgorithms));

        tracker.Track(
            input,
            tracker.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Cryptography_MD5,  HashDataName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_MD5,  TryHashDataName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_MD5,  HashDataAsyncName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, HashDataName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, TryHashDataName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_SHA1, HashDataAsyncName)));

        tracker.Track(
            input,
            tracker.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptographicOperations, HashDataName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptographicOperations, "HmacData"),
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptographicOperations, TryHashDataName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptographicOperations, HashDataAsyncName),
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptographicOperations, "TryHmacData"),
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptographicOperations, "HmacDataAsync"),
                new MemberDescriptor(KnownType.System_Security_Cryptography_CryptographicOperations, TryHashDataName)),
            tracker.ArgumentAtIndexIs(0, IsUnsafeAlgorithm));

        tracker.Track(
            input,
            tracker.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, HashDataName)),
            tracker.ArgumentAtIndexIs(1, IsUnsafeAlgorithm));

        tracker.Track(
            input,
            tracker.MatchMethod(
                new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, TryHashDataName)),
            tracker.ArgumentAtIndexIs(2, IsUnsafeAlgorithm));

        tracker.Track(
            input,
            tracker.MatchMethod(new MemberDescriptor(KnownType.System_Security_Cryptography_DSA, HashDataName)),
            tracker.ArgumentAtIndexIs(3, IsUnsafeAlgorithm));
    }
}
