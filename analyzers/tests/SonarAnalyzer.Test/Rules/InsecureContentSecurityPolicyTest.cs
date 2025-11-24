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

#if NET

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class InsecureContentSecurityPolicyTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<InsecureContentSecurityPolicy>().AddReferences([
        AspNetCoreMetadataReference.MicrosoftAspNetCore,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpFeatures,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
        AspNetCoreMetadataReference.MicrosoftExtensionsPrimitives]);

    [TestMethod]
    public void InsecureContentSecurityPolicy_CS() =>
        builder.AddPaths("InsecureContentSecurityPolicy.cs").Verify();

    [TestMethod]
    public void InsecureContentSecurityPolicy_CS_Latest() =>
        builder.AddPaths("InsecureContentSecurityPolicy.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();
}

#endif
