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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class OptionalParameterTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.OptionalParameter>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.OptionalParameter>();

    [TestMethod]
    public void OptionalParameter_CS() =>
        builderCS.AddPaths("OptionalParameter.cs").Verify();

    [TestMethod]
    public void OptionalParameter_VB() =>
        builderVB.AddPaths("OptionalParameter.vb").Verify();

#if NET

    [TestMethod]
    public void OptionalParameter_CS_Web() =>
        builderCS.AddPaths("OptionalParameter.Web.cs")
            .AddReferences(MetadataReferenceFacade.AspNetCoreReferences).Verify();

    [TestMethod]
    public void OptionalParameter_CSharp10() =>
        builderCS.AddPaths("OptionalParameter.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).VerifyNoIssues();

    [TestMethod]
    public void OptionalParameter_CSharp11() =>
        builderCS.AddPaths("OptionalParameter.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

#endif

}
