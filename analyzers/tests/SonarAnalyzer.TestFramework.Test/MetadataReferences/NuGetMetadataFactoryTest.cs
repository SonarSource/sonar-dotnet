﻿/*
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

using System.IO;

namespace SonarAnalyzer.Test.TestFramework.Tests.MetadataReferences;

[TestClass]
public class NuGetMetadataFactoryTest
{
    [TestMethod]
    public void EnsureInstalled_InstallsMissingPackage()
    {
        const string id = "PayBySquare.TextGenerator.NET";  // Small package that is not used for other UTs
        const string version = "1.0.0";
        var packagesFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Paths.AnalyzersRoot, "packages"); // Same as NuGetMetadataFactory.PackagesFolder
        var packageDir = Path.GetFullPath(Path.Combine(packagesFolder, id, "Sonar." + version, string.Empty));
        // We need to delete the package from local cache to force the factory to always download it. Otherwise, the code would (almost*) never be covered on CI runs.
        if (Directory.Exists(packageDir))
        {
            Directory.Delete(packageDir, true);
        }
        NuGetMetadataFactory.Create(id, version).Should().NotBeEmpty();
        Directory.Exists(packageDir).Should().BeTrue();
        // *Almost, because first run on of the day on a new VM after a release of any LATEST package would randomly mark it as covered.
    }
}
