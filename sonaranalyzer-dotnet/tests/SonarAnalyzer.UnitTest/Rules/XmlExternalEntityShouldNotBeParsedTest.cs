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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;
using System.Linq;
using Moq;
using SonarAnalyzer.Helpers;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class XmlExternalEntityShouldNotBeParsedTest
    {
        [DataRow(NetFrameworkVersion.After45, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlDocument.cs")]
        [DataRow(NetFrameworkVersion.Below4, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlDocument_Net35.cs")]
        [DataRow(NetFrameworkVersion.Between4And45, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlDocument_Net4.cs")]
        [DataRow(NetFrameworkVersion.Unknown, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlDocument_UnknownFrameworkVersion.cs")]
        [DataTestMethod]
        [TestCategory("Rule")]
        public void XmlExternalEntityShouldNotBeParsed_XmlDocument(NetFrameworkVersion version, string testFilePath)
        {
            var versionProviderMock = new Mock<INetFrameworkVersionProvider>();
            versionProviderMock
                .Setup(vp => vp.GetDotNetFrameworkVersion(It.IsAny<Compilation>()))
                .Returns(version);

            Verifier.VerifyAnalyzer(testFilePath,
                new XmlExternalEntityShouldNotBeParsed(versionProviderMock.Object),
                additionalReferences: FrameworkMetadataReference.SystemXml
                    .Concat(FrameworkMetadataReference.SystemData)
                    .Concat(FrameworkMetadataReference.SystemXmlLinq)
                    .Concat(NuGetMetadataReference.MicrosoftWebXdt())
                    .ToArray());
        }

        [DataRow(NetFrameworkVersion.After45, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlTextReader.cs")]
        [DataRow(NetFrameworkVersion.Below4, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlTextReader_Net35.cs")]
        [DataRow(NetFrameworkVersion.Between4And45, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlTextReader_Net4.cs")]
        [DataRow(NetFrameworkVersion.Unknown, @"TestCases\XmlExternalEntityShouldNotBeParsed_XmlTextReader_UnknownFrameworkVersion.cs")]
        [DataTestMethod]
        [TestCategory("Rule")]
        public void XmlExternalEntityShouldNotBeParsed_XmlTextReader(NetFrameworkVersion version, string testFilePath)
        {
            var versionProviderMock = new Mock<INetFrameworkVersionProvider>();
            versionProviderMock
                .Setup(vp => vp.GetDotNetFrameworkVersion(It.IsAny<Compilation>()))
                .Returns(version);

            Verifier.VerifyAnalyzer(testFilePath,
                new XmlExternalEntityShouldNotBeParsed(versionProviderMock.Object),
                additionalReferences: FrameworkMetadataReference.SystemXml.ToArray());
        }

        // FIXME: add tests for the following APIs

        // public void XmlExternalEntityShouldNotBeParsed_DotNetFramework_XmlPathNavigator()

        // public void XmlExternalEntityShouldNotBeParsed_DotNetFramework_XmlReader()

        // public void XmlExternalEntityShouldNotBeParsed_DotNetFramework_AlwaysSafe()

        // The NetCore test should be a smoke test to be sure we raise issues on those libraries
        // public void XmlExternalEntityShouldNotBeParsed_NetCore()
    }
}

