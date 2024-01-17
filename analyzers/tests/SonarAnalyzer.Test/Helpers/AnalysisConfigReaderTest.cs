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

using System.IO;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class AnalysisConfigReaderTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void MissingFile_Throws()
        {
            var path = Path.GetFullPath("ThisFileDoesNotExist.xml");
            ((Func<AnalysisConfigReader>)(() => new AnalysisConfigReader(path))).Should().Throw<InvalidOperationException>().WithMessage($"File '{path}' could not be parsed.");
        }

        [TestMethod]
        [DataRow(@"<?xml that is invalid")]
        [DataRow(@"<ValidXml><UnexpectedContent /></ValidXml>")]
        public void UnexpectedXml_Throws(string xml)
        {
            var path = TestHelper.WriteFile(TestContext, "SonarQubeAnalysisConfig.xml", xml);
            ((Func<AnalysisConfigReader>)(() => new AnalysisConfigReader(path))).Should().Throw<InvalidOperationException>().WithMessage($"File '{path}' could not be parsed.");
        }

        [TestMethod]
        public void UnchangedFiles_NotPresent_ReturnsEmpty()
        {
            var path = AnalysisScaffolding.CreateAnalysisConfig(TestContext, "SomeOtherProperty", "This is not UnchangedFilesPath");
            var sut = new AnalysisConfigReader(path);
            sut.UnchangedFiles().Should().BeEmpty();
        }

        [TestMethod]
        public void UnchangedFiles_Empty_ReturnsEmpty()
        {
            var path = AnalysisScaffolding.CreateAnalysisConfig(TestContext, Array.Empty<string>());
            var sut = new AnalysisConfigReader(path);
            sut.UnchangedFiles().Should().BeEmpty();
        }

        [TestMethod]
        public void UnchangedFiles_Present_ReturnsContent()
        {
            var path = AnalysisScaffolding.CreateAnalysisConfig(TestContext, new[] { @"C:\File1.cs", @"C:\File2.cs", "Any other string" });
            var sut = new AnalysisConfigReader(path);
            sut.UnchangedFiles().Should().BeEquivalentTo(@"C:\File1.cs", @"C:\File2.cs", "Any other string");
        }
    }
}
