/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class UseValueParameterTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<UseValueParameter>();

        [TestMethod]
        public void UseValueParameter() =>
            builder.AddPaths("UseValueParameter.cs").Verify();

#if NET

        [TestMethod]
        public void UseValueParameter_CS_Latest() =>
            builder.AddPaths("UseValueParameter.Latest.cs", "UseValueParameter.Latest.Partial.cs")
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

#endif

        [TestMethod]
        public void UseValueParameter_InvalidCode() =>
            builder.AddSnippet(@"
public int Foo
{
    get => field;
    set => // Noncompliant
}")
                .WithErrorBehavior(CompilationErrorBehavior.Ignore)
                .Verify();
    }
}
