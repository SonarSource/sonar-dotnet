/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class UseFindSystemTimeZoneByIdBase<TSyntaxKind, TInvocation> : DoNotCallMethodsBase<TSyntaxKind, TInvocation>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private const string DiagnosticId = "S6575";

    protected override string MessageFormat => "Use \"TimeZoneInfo.FindSystemTimeZoneById\" directly instead of \"{0}\"";

    protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } = new List<MemberDescriptor>
        {
            new(KnownType.TimeZoneConverter_TZConvert, "IanaToWindows"),
            new(KnownType.TimeZoneConverter_TZConvert, "WindowsToIana"),
            new(KnownType.TimeZoneConverter_TZConvert, "TryIanaToWindows"),
            new(KnownType.TimeZoneConverter_TZConvert, "TryWindowsToIana"),
        };

    protected UseFindSystemTimeZoneByIdBase() : base(DiagnosticId) { }

    protected override bool ShouldRegisterAction(Compilation compilation) =>
        compilation.GetTypeByMetadataName(KnownType.System_TimeOnly) is not null
        && compilation.GetTypeByMetadataName(KnownType.TimeZoneConverter_TZConvert) is not null;
}
