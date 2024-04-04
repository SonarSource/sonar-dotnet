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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ClearTextProtocolsAreSensitiveTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new ClearTextProtocolsAreSensitive(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void ClearTextProtocolsAreSensitive() =>
            builder.AddPaths("ClearTextProtocolsAreSensitive.cs")
                .AddReferences(AdditionalReferences)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .WithConcurrentAnalysis(false)
                .Verify();

#if NETFRAMEWORK

        [TestMethod]
        public void ClearTextProtocolsAreSensitive_NetFx() =>
            builder.AddPaths("ClearTextProtocolsAreSensitive.NetFramework.cs")
                .AddReferences(FrameworkMetadataReference.SystemWebServices)
                .Verify();

#endif

#if NET

        [TestMethod]
        public void ClearTextProtocolsAreSensitive_CSharp9() =>
            builder.AddPaths("ClearTextProtocolsAreSensitive.CSharp9.cs")
                .WithTopLevelStatements()
                .AddReferences(AdditionalReferences)
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void ClearTextProtocolsAreSensitive_CSharp10() =>
            builder.AddPaths("ClearTextProtocolsAreSensitive.CSharp10.cs")
                .WithTopLevelStatements()
                .AddReferences(AdditionalReferences)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void ClearTextProtocolsAreSensitive_CSharp11() =>
            builder.AddPaths("ClearTextProtocolsAreSensitive.CSharp11.cs")
                .WithTopLevelStatements()
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void ClearTextProtocolsAreSensitive_CSharp12() =>
            builder.AddPaths("ClearTextProtocolsAreSensitive.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

#endif

        internal static IEnumerable<MetadataReference> AdditionalReferences =>
            MetadataReferenceFacade.SystemNetHttp
                .Concat(MetadataReferenceFacade.SystemComponentModelPrimitives)
                .Concat(MetadataReferenceFacade.SystemXml)
                .Concat(MetadataReferenceFacade.SystemXmlLinq)
                .Concat(MetadataReferenceFacade.SystemWeb);
    }
}
