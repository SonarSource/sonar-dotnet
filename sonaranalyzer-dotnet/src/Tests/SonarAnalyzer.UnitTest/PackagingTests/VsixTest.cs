/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace SonarAnalyzer.UnitTest.PackagingTests
{
    [TestClass]
    public class VsixTest
    {
        [TestMethod]
        [TestCategory("Vsix")]
        public void Size_Check()
        {
            const string vsixFileName = "SonarAnalyzer.vsix";
#if DEBUG
            const string pathEnding = @"bin\Debug";
            const int approxFileSize = 1621 * 1024;
#else
            const string pathEnding = @"bin\Release";
            const int approxFileSize = 551 * 1024;
#endif

            var currentDirectory = Directory.GetCurrentDirectory();
            var vsixDirectoryPath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\SonarAnalyzer.Vsix\", pathEnding));
            var vsixFile = new FileInfo(Path.Combine(vsixDirectoryPath, vsixFileName));

            vsixFile.Exists.Should().BeTrue("VSIX file doesn't exist");

            const double upperBound = approxFileSize * 1.1;
            vsixFile.Length
                .Should().BeLessThan((long)upperBound, "VSIX file has grown", upperBound, vsixFile.Length);

            const double lowerBound = approxFileSize * 0.9;
            vsixFile.Length
                .Should().BeGreaterThan((long)lowerBound, "VSIX file has shrunk", lowerBound, vsixFile.Length);
        }
    }
}
