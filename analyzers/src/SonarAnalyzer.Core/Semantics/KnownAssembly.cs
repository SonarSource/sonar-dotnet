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

using static SonarAnalyzer.Core.Semantics.KnownAssembly.Predicates;

namespace SonarAnalyzer.Core.Semantics;

public sealed partial class KnownAssembly
{
    private readonly Func<IEnumerable<AssemblyIdentity>, bool> predicate;

    public static KnownAssembly XUnit_Assert { get; } = new(And(
        NameIs("xunit.assert")
            .Or(NameIs("xunit").And(VersionLowerThen("2.0")))
            .Or(NameIs("xunit.v3.assert")),
        PublicKeyTokenIs("8d05b1bb7a6fdb6c")));

    /// <summary>
    /// Any MSTest framework either referenced via
    /// <see href="https://www.nuget.org/packages/MicrosoftVisualStudioQualityToolsUnitTestFramework">nuget.org/MicrosoftVisualStudioQualityToolsUnitTestFramework</see> (MSTest V1)
    /// or <see href="https://www.nuget.org/packages/MSTest.TestFramework">nuget.org/MSTest.TestFramework</see> (MSTest V2).
    /// </summary>
    public static KnownAssembly MSTest { get; } =
        new(And(NameIs("Microsoft.VisualStudio.QualityTools.UnitTestFramework").Or(NameIs("Microsoft.VisualStudio.TestPlatform.TestFramework")),
                PublicKeyTokenIs("b03f5f7f11d50a3a")));

    public static KnownAssembly NFluent { get; } = new(NameIs("NFluent").And(OptionalPublicKeyTokenIs("18828b37b84b1437")));
    public static KnownAssembly FluentAssertions { get; } = new(NameAndPublicKeyIs("FluentAssertions", "33f2691a05b67b6a"));
    public static KnownAssembly NSubstitute { get; } = new(NameAndPublicKeyIs("NSubstitute", "92dd2e9066daa5ca"));
    // Logging assemblies
    public static KnownAssembly MicrosoftExtensionsLoggingAbstractions { get; } = new(NameAndPublicKeyIs("Microsoft.Extensions.Logging.Abstractions", "adb9793829ddae60"));
    public static KnownAssembly Serilog { get; } = new(NameAndPublicKeyIs("Serilog", "24c2f752a8e58a10"));
    public static KnownAssembly MicrosoftAspNetCoreMvcCore { get; } = new(NameAndPublicKeyIs("Microsoft.AspNetCore.Mvc.Core", "adb9793829ddae60"));
    public static KnownAssembly SwashbuckleAspNetCoreSwagger { get; } = new(NameAndPublicKeyIs("Swashbuckle.AspNetCore.Swagger", "62657d7474907593"));
    public static KnownAssembly NLog { get; } = new(NameAndPublicKeyIs("NLog", "5120e14c03d0593c"));
    public static KnownAssembly Log4Net { get; } = new(NameIs("log4net").And(PublicKeyTokenIsAny("669e0ddf0bb1aa2a", "1b44e1d426115821")));
    public static KnownAssembly CommonLoggingCore { get; } = new(NameAndPublicKeyIs("Common.Logging.Core", "af08829b84f0328e"));
    public static KnownAssembly CastleCore { get; } = new(NameAndPublicKeyIs("Castle.Core", "407dd0808d44fbdc"));

    internal KnownAssembly(Func<AssemblyIdentity, bool> predicate, params Func<AssemblyIdentity, bool>[] or)
        : this(predicate is null || Array.Exists(or, x => x is null)
              ? throw new ArgumentNullException(nameof(predicate), "All predicates must be non-null.")
              : x => x.Any(y => predicate(y) || Array.Exists(or, orPredicate => orPredicate(y))))
    { }

    internal KnownAssembly(Func<IEnumerable<AssemblyIdentity>, bool> predicate) =>
        this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

    public bool IsReferencedBy(Compilation compilation) =>
        predicate(compilation.ReferencedAssemblyNames);

    internal static Func<AssemblyIdentity, bool> And(Func<AssemblyIdentity, bool> left, Func<AssemblyIdentity, bool> right) =>
        KnownAssemblyExtensions.And(left, right);
}
