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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.TestFramework.MetadataReferences;

namespace SonarAnalyzer.CSharp.Styling.Test.Rules;

[TestClass]
public class UseInnermostRegistrationContextTest
{
    private readonly VerifierBuilder builder = StylingVerifierBuilder.Create<UseInnermostRegistrationContext>()
        .AddReferences([MetadataReference.CreateFromFile(typeof(SonarAnalysisContext).Assembly.Location)])
        .AddReferences(NuGetMetadataReference.MicrosoftCodeAnalysisCSharp());

    [TestMethod]
    public void UseInnermostRegistrationContext() =>
        builder.AddPaths("UseInnermostRegistrationContext.cs").Verify();
}
