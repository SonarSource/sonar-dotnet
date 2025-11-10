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
public class PrivateFieldUsedAsLocalVariableTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<PrivateFieldUsedAsLocalVariable>();

    [TestMethod]
    public void PrivateFieldUsedAsLocalVariable() =>
        builder.AddPaths("PrivateFieldUsedAsLocalVariable.cs")
            .Verify();

#if NET

    [TestMethod]
    public void PrivateFieldUsedAsLocalVariable_CSharp9() =>
        builder.AddPaths("PrivateFieldUsedAsLocalVariable.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void PrivateFieldUsedAsLocalVariable_CSharp10() =>
        builder.AddPaths("PrivateFieldUsedAsLocalVariable.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

#endif

}
