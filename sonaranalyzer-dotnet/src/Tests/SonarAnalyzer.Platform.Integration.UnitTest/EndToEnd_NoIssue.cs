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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Runner;
using System.IO;
using SonarAnalyzer.Protobuf;
using FluentAssertions;

namespace SonarAnalyzer.Integration.UnitTest
{
    [TestClass]
    public class EndToEnd_NoIssue
    {
        public TestContext TestContext { get; set; }

        internal const string OutputFolderName = "Output";
        internal const string TestResourcesFolderName = "TestResources";
        internal const string TestInputFileName = "TestInput";
        internal const string TestInputPath = TestResourcesFolderName + "\\" + TestInputFileName;
        [TestMethod]
        public void IssuesFileIsEmpty()
        {
            var tempInputFilePath = Path.Combine(TestContext.DeploymentDirectory, ParameterLoader.ParameterConfigurationFileName);
            File.Copy(Path.Combine(TestResourcesFolderName, "ConfigurationTest.Empty.Cs.xml"), tempInputFilePath, true);

            Program.RunAnalysis(new ScannerAnalyzerConfiguration
            {
                InputConfigurationPath = tempInputFilePath,
                OutputFolderPath = OutputFolderName,
                Language = AnalyzerLanguage.CSharp.ToString(),
                WorkDirectoryConfigFilePath = Path.Combine(TestResourcesFolderName, "ProjectOutFolderPath.txt")
            });

            var fileIssues = EndToEnd_CSharp.GetDeserializedData<FileIssues>(Path.Combine(OutputFolderName, Program.IssuesFileName));
            fileIssues.Should().BeEmpty();
        }
    }
}
