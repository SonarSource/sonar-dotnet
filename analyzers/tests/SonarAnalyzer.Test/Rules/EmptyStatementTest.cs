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
    public class EmptyStatementTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<EmptyStatement>();
        private readonly VerifierBuilder codeFix = new VerifierBuilder<EmptyStatement>().WithCodeFix<EmptyStatementCodeFix>();

        [TestMethod]
        public void EmptyStatement() =>
            builder.AddPaths("EmptyStatement.cs").Verify();

#if NET

        [TestMethod]
        public void EmptyStatement_CS_TopLevelStatements() =>
            builder.AddPaths("EmptyStatement.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void EmptyStatement_CS_Latest() =>
            builder.AddPaths("EmptyStatement.Latest.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .Verify();

#endif

        [TestMethod]
        public void EmptyStatement_CodeFix() =>
            codeFix.AddPaths("EmptyStatement.cs")
                .WithCodeFixedPaths("EmptyStatement.Fixed.cs")
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void EmptyStatement_CodeFix_CS_TopLevelStatements() =>
            codeFix.AddPaths("EmptyStatement.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .WithCodeFixedPaths("EmptyStatement.TopLevelStatements.Fixed.cs")
                .VerifyCodeFix();

#endif

    }
}
