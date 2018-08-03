/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using NuGet;

namespace SonarAnalyzer.UnitTest
{
    internal class AssemblyReference : IEquatable<AssemblyReference>
    {
        internal class NuGetInfo : IEquatable<NuGetInfo>
        {
            internal const string LatestVersion = null;

            public string Name { get; }
            public SemanticVersion Version { get; }

            internal NuGetInfo(string id, string version = LatestVersion)
            {
                Name = id;
                Version = SemanticVersion.ParseOptionalVersion(version);
            }

            public override bool Equals(object obj) =>
                Equals(obj as NuGetInfo);

            public override int GetHashCode() =>
                $"{Name}|{Version}".GetHashCode();

            public bool Equals(NuGetInfo other) =>
                other != null &&
                other.Name == Name &&
                other.Version == Version;
        }

        public string Name { get; }
        public NuGetInfo NuGet { get; }

        private AssemblyReference(string assemblyName, NuGetInfo nuGetInfo = null)
        {
            Name = assemblyName;
            NuGet = nuGetInfo;
        }

        public static AssemblyReference FromFramework(string assemblyName) =>
            new AssemblyReference(assemblyName);

        public static AssemblyReference FromNuGet(string assemblyName, string packageId, string version) =>
            new AssemblyReference(assemblyName, new NuGetInfo(packageId, version));

        public override bool Equals(object obj) =>
            Equals(obj as AssemblyReference);

        public override int GetHashCode() =>
            $"{Name}|{NuGet?.GetHashCode()}".GetHashCode();

        public bool Equals(AssemblyReference other) =>
            other != null &&
            other.Name == Name &&
            ((NuGet == null && other.NuGet == null) || (NuGet.Equals(other.NuGet)));
    }
}
