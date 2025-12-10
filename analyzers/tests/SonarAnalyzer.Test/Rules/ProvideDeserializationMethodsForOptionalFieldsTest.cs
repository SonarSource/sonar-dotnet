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
public class ProvideDeserializationMethodsForOptionalFieldsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ProvideDeserializationMethodsForOptionalFields>();

    [TestMethod]
    public void ProvideDeserializationMethodsForOptionalFields_CS() =>
        builderCS.AddPaths("ProvideDeserializationMethodsForOptionalFields.cs").Verify();

    [TestMethod]
    public void ProvideDeserializationMethodsForOptionalFields_CS_Latest() =>
        builderCS.AddPaths("ProvideDeserializationMethodsForOptionalFields.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void ProvideDeserializationMethodsForOptionalFields_VB() =>
        new VerifierBuilder<VB.ProvideDeserializationMethodsForOptionalFields>().AddPaths("ProvideDeserializationMethodsForOptionalFields.vb").Verify();
}
