/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class MutableFieldsShouldNotBePublicReadonlyTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<MutableFieldsShouldNotBePublicReadonly>();

    [TestMethod]
    public void PublicMutableFieldsShouldNotBeReadonly() =>
        builder.AddPaths("MutableFieldsShouldNotBePublicReadonly.cs").AddReferences(NuGetMetadataReference.SystemCollectionsImmutable("1.3.0")).Verify();

#if NET

    [TestMethod]
    public void PublicMutableFieldsShouldNotBeReadonly_CS_Latest() =>
        builder.AddPaths("MutableFieldsShouldNotBePublicReadonly.Latest.cs")
            .AddReferences(MetadataReferenceFacade.SystemCollections)
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

}
