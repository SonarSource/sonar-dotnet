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
public class PureAttributeOnVoidMethodTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.PureAttributeOnVoidMethod>();

    [TestMethod]
    public void PureAttributeOnVoidMethod_CS() =>
        builderCS.AddPaths("PureAttributeOnVoidMethod.cs").Verify();

    [TestMethod]
    public void PureAttributeOnVoidMethod_CSharpLatest() =>
        builderCS.AddPaths("PureAttributeOnVoidMethod.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void PureAttributeOnVoidMethod_TopLevelStatements() =>
        builderCS.AddPaths("PureAttributeOnVoidMethod.TopLevelStatements.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void PureAttributeOnVoidMethod_VB() =>
        new VerifierBuilder<VB.PureAttributeOnVoidMethod>().AddPaths("PureAttributeOnVoidMethod.vb").Verify();

    [TestMethod]
    public void PureAttributeOnVoidMethod_CSharp7() =>
        builderCS.AddPaths("PureAttributeOnVoidMethod.CSharp7.cs").WithOptions(LanguageOptions.OnlyCSharp7).Verify();
}
