/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using NSubstitute;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class XmlExternalEntityShouldNotBeParsedTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder()
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(MetadataReferenceFacade.SystemXmlLinq)
            .AddReferences(NuGetMetadataReference.MicrosoftWebXdt());

        [DataRow(NetFrameworkVersion.After452, "XmlExternalEntityShouldNotBeParsed_XmlDocument.cs")]
        [DataRow(NetFrameworkVersion.Probably35, "XmlExternalEntityShouldNotBeParsed_XmlDocument_Net35.cs")]
        [DataRow(NetFrameworkVersion.Between4And451, "XmlExternalEntityShouldNotBeParsed_XmlDocument_Net4.cs")]
        [DataRow(NetFrameworkVersion.Unknown, "XmlExternalEntityShouldNotBeParsed_XmlDocument_UnknownFrameworkVersion.cs")]
        [DataTestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlDocument(NetFrameworkVersion version, string testFilePath) =>
            WithAnalyzer(version).AddPaths(testFilePath).Verify();

#if NET

        [TestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlDocument_CSharp9() =>
            WithAnalyzer(NetFrameworkVersion.After452).AddPaths("XmlExternalEntityShouldNotBeParsed_XmlDocument_CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlDocument_CSharp10() =>
            WithAnalyzer(NetFrameworkVersion.After452)
                .AddPaths("XmlExternalEntityShouldNotBeParsed_XmlDocument_CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .WithTopLevelStatements()
                .Verify();

#endif

        [DataRow(NetFrameworkVersion.After452, "XmlExternalEntityShouldNotBeParsed_XmlTextReader.cs")]
        [DataRow(NetFrameworkVersion.Probably35, "XmlExternalEntityShouldNotBeParsed_XmlTextReader_Net35.cs")]
        [DataRow(NetFrameworkVersion.Between4And451, "XmlExternalEntityShouldNotBeParsed_XmlTextReader_Net4.cs")]
        [DataRow(NetFrameworkVersion.Unknown, "XmlExternalEntityShouldNotBeParsed_XmlTextReader_UnknownFrameworkVersion.cs")]
        [DataTestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlTextReader(NetFrameworkVersion version, string testFilePath) =>
            WithAnalyzer(version).AddPaths(testFilePath).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlTextReader_CSharp9() =>
            WithAnalyzer(NetFrameworkVersion.After452).AddPaths("XmlExternalEntityShouldNotBeParsed_XmlTextReader_CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlTextReader_CSharp10() =>
            WithAnalyzer(NetFrameworkVersion.After452).AddPaths("XmlExternalEntityShouldNotBeParsed_XmlTextReader_CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

        [DataRow(NetFrameworkVersion.After452, "XmlExternalEntityShouldNotBeParsed_AlwaysSafe.cs")]
        [DataRow(NetFrameworkVersion.Unknown, "XmlExternalEntityShouldNotBeParsed_AlwaysSafe.cs")]
        [DataTestMethod]
        public void XmlExternalEntityShouldNotBeParsed_AlwaysSafe(NetFrameworkVersion version, string testFilePath) =>
            WithAnalyzer(version).AddPaths(testFilePath).Verify();

        [DataRow(NetFrameworkVersion.Probably35, "XmlExternalEntityShouldNotBeParsed_XmlReader_Net35.cs")]
        [DataRow(NetFrameworkVersion.Between4And451, "XmlExternalEntityShouldNotBeParsed_XmlReader_Net4.cs")]
        [DataRow(NetFrameworkVersion.After452, "XmlExternalEntityShouldNotBeParsed_XmlReader_Net452.cs")]
        [DataRow(NetFrameworkVersion.Unknown, "XmlExternalEntityShouldNotBeParsed_XmlReader_Net452.cs")]
        [DataTestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlReader(NetFrameworkVersion version, string testFilePath) =>
            WithAnalyzer(version).AddPaths(testFilePath).Verify();

#if NET

        [TestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XmlReader_CSharp9() =>
            WithAnalyzer(NetFrameworkVersion.After452).AddPaths("XmlExternalEntityShouldNotBeParsed_XmlReader_CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XPathDocument_CSharp9() =>
            WithAnalyzer(NetFrameworkVersion.After452).AddPaths("XmlExternalEntityShouldNotBeParsed_XPathDocument_CSharp9.cs").WithTopLevelStatements().Verify();

#endif

        [DataRow(NetFrameworkVersion.Probably35, "XmlExternalEntityShouldNotBeParsed_XPathDocument_Net35.cs")]
        [DataRow(NetFrameworkVersion.Between4And451, "XmlExternalEntityShouldNotBeParsed_XPathDocument_Net4.cs")]
        [DataRow(NetFrameworkVersion.After452, "XmlExternalEntityShouldNotBeParsed_XPathDocument_Net452.cs")]
        [DataRow(NetFrameworkVersion.Unknown, "XmlExternalEntityShouldNotBeParsed_XPathDocument_Net452.cs")]
        [DataTestMethod]
        public void XmlExternalEntityShouldNotBeParsed_XPathDocument(NetFrameworkVersion version, string testFilePath) =>
            WithAnalyzer(version).AddPaths(testFilePath).Verify();

        [TestMethod]
        public void XmlExternalEntityShouldNotBeParsed_NoCrashOnExternalParameterUse() =>
            WithAnalyzer(NetFrameworkVersion.After452)
                .AddPaths("XmlExternalEntityShouldNotBeParsed_XmlReader_ExternalParameter.cs", "XmlExternalEntityShouldNotBeParsed_XmlReader_ParameterProvider.cs")
                .Verify();

        private VerifierBuilder WithAnalyzer(NetFrameworkVersion version)
        {
            var fxVersion = Substitute.For<INetFrameworkVersionProvider>();
            fxVersion.GetDotNetFrameworkVersion(Arg.Any<Compilation>()).Returns(version);
            return builder.AddAnalyzer(() => new XmlExternalEntityShouldNotBeParsed(fxVersion));
        }
    }
}
