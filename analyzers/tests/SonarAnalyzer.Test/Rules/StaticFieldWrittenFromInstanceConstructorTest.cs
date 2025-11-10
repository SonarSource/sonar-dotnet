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
public class StaticFieldWrittenFromInstanceConstructorTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<StaticFieldWrittenFromInstanceConstructor>();

    [TestMethod]
    public void StaticFieldWrittenFromInstanceConstructor() =>
        builder.AddPaths("StaticFieldWrittenFromInstanceConstructor.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

    [TestMethod]
    public void StaticFieldWrittenFromInstanceConstructor_Latest() =>
        builder.AddPaths("StaticFieldWrittenFromInstanceConstructor.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

}
