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

namespace SonarAnalyzer.Core.Semantics;

public sealed partial class KnownAssembly
{
    private const StringComparison AssemblyNameComparison = StringComparison.OrdinalIgnoreCase;

    internal static class Predicates
    {
        internal static Func<AssemblyIdentity, bool> NameIs(string name) =>
            x => x.Name.Equals(name, AssemblyNameComparison);

        internal static Func<AssemblyIdentity, bool> StartsWith(string name) =>
            x => x.Name.StartsWith(name, AssemblyNameComparison);

        internal static Func<AssemblyIdentity, bool> EndsWith(string name) =>
            x => x.Name.EndsWith(name, AssemblyNameComparison);

        internal static Func<AssemblyIdentity, bool> Contains(string name) =>
            x => x.Name.IndexOf(name, 0, AssemblyNameComparison) >= 0;

        internal static Func<AssemblyIdentity, bool> VersionLowerThen(string version) =>
            VersionLowerThen(Version.Parse(version));

        internal static Func<AssemblyIdentity, bool> VersionLowerThen(Version version) =>
            x => x.Version < version;

        internal static Func<AssemblyIdentity, bool> VersionGreaterOrEqual(string version) =>
            VersionGreaterOrEqual(Version.Parse(version));

        internal static Func<AssemblyIdentity, bool> VersionGreaterOrEqual(Version version) =>
            x => x.Version >= version;

        internal static Func<AssemblyIdentity, bool> VersionBetween(string from, string to) =>
            VersionBetween(Version.Parse(from), Version.Parse(to));

        internal static Func<AssemblyIdentity, bool> VersionBetween(Version from, Version to) =>
            x => x.Version >= from && x.Version <= to;

        internal static Func<AssemblyIdentity, bool> OptionalPublicKeyTokenIs(string key) =>
            x => !x.HasPublicKey || PublicKeyEqualHex(x, key);

        internal static Func<AssemblyIdentity, bool> PublicKeyTokenIs(string key) =>
            x => x.HasPublicKey && PublicKeyEqualHex(x, key);

        internal static Func<AssemblyIdentity, bool> PublicKeyTokenIsAny(params string[] keys) =>
            x => x.HasPublicKey && Array.Exists(keys, key => PublicKeyEqualHex(x, key));

        internal static Func<AssemblyIdentity, bool> NameAndPublicKeyIs(string name, string key) =>
            NameIs(name).And(PublicKeyTokenIs(key));

        private static bool PublicKeyEqualHex(AssemblyIdentity identity, string hexString)
        {
            var normalizedHexString = hexString.Replace("-", string.Empty);
            return ArraysEqual(identity.PublicKeyToken.ToArray(), normalizedHexString) || ArraysEqual(identity.PublicKey.ToArray(), normalizedHexString);

            static bool ArraysEqual(byte[] key, string hexString) =>
                BitConverter.ToString(key).Replace("-", string.Empty).Equals(hexString, StringComparison.OrdinalIgnoreCase);
        }
    }
}
