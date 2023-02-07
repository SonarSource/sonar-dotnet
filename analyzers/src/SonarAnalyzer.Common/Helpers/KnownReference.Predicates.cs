﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Helpers;

public sealed partial class KnownReference
{
    internal static Func<AssemblyIdentity, bool> NameIs(string name) =>
        new(x => x.Name.Equals(name));

    internal static Func<AssemblyIdentity, bool> StartsWith(string name) =>
        new(x => x.Name.StartsWith(name));

    internal static Func<AssemblyIdentity, bool> EndsWith(string name) =>
        new(x => x.Name.EndsWith(name));

    internal static Func<AssemblyIdentity, bool> Contains(string name) =>
        new(x => x.Name.Contains(name));

    internal static Func<AssemblyIdentity, bool> VersionLowerThen(string version) =>
        VersionLowerThen(Version.Parse(version));

    internal static Func<AssemblyIdentity, bool> VersionLowerThen(Version version) =>
        new(x => x.Version < version);

    internal static Func<AssemblyIdentity, bool> VersionGreaterOrEqual(string version) =>
        VersionGreaterOrEqual(Version.Parse(version));

    internal static Func<AssemblyIdentity, bool> VersionGreaterOrEqual(Version version) =>
        new(x => x.Version >= version);

    internal static Func<AssemblyIdentity, bool> VersionBetween(string from, string to) =>
        VersionBetween(Version.Parse(from), Version.Parse(to));

    internal static Func<AssemblyIdentity, bool> VersionBetween(Version from, Version to) =>
        new(x => x.Version >= from && x.Version <= to);

    internal static Func<AssemblyIdentity, bool> OptionalPublicKeyTokenIs(string key) =>
        new(x => !x.HasPublicKey || PublicKeyEqualHex(x, key));

    internal static Func<AssemblyIdentity, bool> PublicKeyTokenIs(string key) =>
        new(x => x.HasPublicKey && PublicKeyEqualHex(x, key));

    private static bool PublicKeyEqualHex(AssemblyIdentity identity, string hexString)
    {
        var normalizedHexString = hexString.Replace("-", string.Empty);
        return ArraysEqual(identity.PublicKeyToken.ToArray(), normalizedHexString) || ArraysEqual(identity.PublicKey.ToArray(), normalizedHexString);

        static bool ArraysEqual(byte[] key, string hexString) =>
            BitConverter.ToString(key).Replace("-", string.Empty).Equals(hexString, StringComparison.OrdinalIgnoreCase);
    }

    internal static Func<IEnumerable<AssemblyIdentity>, bool> Any(Func<AssemblyIdentity, bool> predicate) =>
        new(identities => identities.Any(predicate));

}
