/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using static SonarAnalyzer.Helpers.KnownAssembly.Predicates;

namespace SonarAnalyzer.Helpers;

public sealed partial class KnownAssembly
{
    private readonly Func<IEnumerable<AssemblyIdentity>, bool> predicate;

    public static KnownAssembly XUnit_Assert { get; } = new(
        And(NameIs("xunit.assert").Or(NameIs("xunit").And(VersionLowerThen("2.0"))),
            PublicKeyTokenIs("8d05b1bb7a6fdb6c")));

    /// <summary>
    /// Any MSTest framework either referenced via
    /// <see href="https://www.nuget.org/packages/MicrosoftVisualStudioQualityToolsUnitTestFramework">nuget.org/MicrosoftVisualStudioQualityToolsUnitTestFramework</see> (MSTest V1)
    /// or <see href="https://www.nuget.org/packages/MSTest.TestFramework">nuget.org/MSTest.TestFramework</see> (MSTest V2).
    /// </summary>
    public static KnownAssembly MSTest { get; } =
        new(And(NameIs("Microsoft.VisualStudio.QualityTools.UnitTestFramework").Or(NameIs("Microsoft.VisualStudio.TestPlatform.TestFramework")),
                PublicKeyTokenIs("b03f5f7f11d50a3a")));

    public static KnownAssembly FluentAssertions { get; } = new(NameIs("FluentAssertions").And(PublicKeyTokenIs("33f2691a05b67b6a")));
    public static KnownAssembly NFluent { get; } = new(NameIs("NFluent").And(OptionalPublicKeyTokenIs("18828b37b84b1437")));
    public static KnownAssembly NSubstitute { get; } = new(NameIs("NSubstitute").And(PublicKeyTokenIs("92dd2e9066daa5ca")));

    internal KnownAssembly(Func<AssemblyIdentity, bool> predicate, params Func<AssemblyIdentity, bool>[] or)
        : this(predicate is null || or.Any(x => x is null)
              ? throw new ArgumentNullException(nameof(predicate), "All predicates must be non-null.")
              : identities => identities.Any(identitiy => predicate(identitiy) || or.Any(orPredicate => orPredicate(identitiy))))
    {
    }

    internal KnownAssembly(Func<IEnumerable<AssemblyIdentity>, bool> predicate) =>
        this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

    public bool IsReferencedBy(Compilation compilation) =>
        predicate(compilation.ReferencedAssemblyNames);

    internal static Func<AssemblyIdentity, bool> And(Func<AssemblyIdentity, bool> left, Func<AssemblyIdentity, bool> right) =>
        KnownAssemblyExtensions.And(left, right);
}
