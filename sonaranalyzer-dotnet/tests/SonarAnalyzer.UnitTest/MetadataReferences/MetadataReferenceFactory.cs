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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.UnitTest.MetadataReferences
{
    internal static class MetadataReferenceFactory
    {
        private static readonly string SystemAssembliesFolder = new FileInfo(typeof(object).Assembly.Location).Directory.FullName;

        public static IEnumerable<MetadataReference> Create(string assemblyName) =>
            ImmutableArray.Create(CreateReference(assemblyName));

        internal static MetadataReference CreateReference(string assemblyName) =>
            MetadataReference.CreateFromFile(Path.Combine(SystemAssembliesFolder, assemblyName));

        internal static MetadataReference CreateReference(string assemblyName, string subFolder) =>
            MetadataReference.CreateFromFile(Path.Combine(SystemAssembliesFolder, subFolder, assemblyName));
    }
}
