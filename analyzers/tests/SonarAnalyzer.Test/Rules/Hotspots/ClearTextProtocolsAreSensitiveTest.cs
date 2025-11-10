/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ClearTextProtocolsAreSensitiveTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new ClearTextProtocolsAreSensitive(AnalyzerConfiguration.AlwaysEnabled));

    internal static IEnumerable<MetadataReference> AdditionalReferences =>
        MetadataReferenceFacade.SystemNetHttp
            .Concat(MetadataReferenceFacade.SystemComponentModelPrimitives)
            .Concat(MetadataReferenceFacade.SystemXml)
            .Concat(MetadataReferenceFacade.SystemXmlLinq)
            .Concat(MetadataReferenceFacade.SystemWeb);

    [TestMethod]
    public void ClearTextProtocolsAreSensitive() =>
        builder.AddPaths("ClearTextProtocolsAreSensitive.cs")
            .AddReferences(AdditionalReferences)
            .WithOptions(LanguageOptions.FromCSharp8)
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
    public void ClearTextProtocolsAreSensitive_Latest() =>
        builder.AddPaths("ClearTextProtocolsAreSensitive.Latest.cs")
            .WithTopLevelStatements()
            .AddReferences(AdditionalReferences)
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

}
