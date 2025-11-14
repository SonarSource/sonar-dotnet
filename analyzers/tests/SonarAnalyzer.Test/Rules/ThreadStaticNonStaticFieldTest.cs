/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ThreadStaticNonStaticFieldTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<ThreadStaticNonStaticField>();

        [TestMethod]
        public void ThreadStaticNonStaticField() =>
            builder.AddPaths("ThreadStaticNonStaticField.cs").Verify();

#if NET

        [TestMethod]
        public void ThreadStaticNonStaticField_CSharp9() =>
            builder.AddPaths("ThreadStaticNonStaticField.CSharp9.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .Verify();

        [TestMethod]
        public void ThreadStaticNonStaticField_CSharp10() =>
            builder.AddPaths("ThreadStaticNonStaticField.CSharp10.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

        [TestMethod]
        public void ThreadStaticNonStaticField_CodeFix_CSharp10() =>
            builder.WithCodeFix<ThreadStaticNonStaticFieldCodeFix>()
                .AddPaths("ThreadStaticNonStaticField.CSharp10.cs")
                .WithCodeFixedPaths("ThreadStaticNonStaticField.CSharp10.Fixed.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .VerifyCodeFix();

        [TestMethod]
        public void ThreadStaticNonStaticField_CSharp11() =>
            builder.AddPaths("ThreadStaticNonStaticField.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

#endif

        [TestMethod]
        public void ThreadStaticNonStaticField_CodeFix() =>
            builder.WithCodeFix<ThreadStaticNonStaticFieldCodeFix>()
                .AddPaths("ThreadStaticNonStaticField.cs")
                .WithCodeFixedPaths("ThreadStaticNonStaticField.Fixed.cs")
                .VerifyCodeFix();
    }
}
