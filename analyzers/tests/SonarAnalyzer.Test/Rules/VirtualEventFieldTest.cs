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
    public class VirtualEventFieldTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<VirtualEventField>();

        [TestMethod]
        public void VirtualEventField() =>
            builder.AddPaths("VirtualEventField.cs").Verify();

#if NET

        [TestMethod]
        public void VirtualEventField_CSharp9() =>
            builder.AddPaths("VirtualEventField.CSharp9.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void VirtualEventField_CSharp9_CodeFix() =>
            builder.WithCodeFix<VirtualEventFieldCodeFix>()
                .AddPaths("VirtualEventField.CSharp9.cs")
                .WithCodeFixedPaths("VirtualEventField.CSharp9.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .VerifyCodeFix();

        [TestMethod]
        public void VirtualEventField_CSharp11() =>
            builder.AddPaths("VirtualEventField.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

        [TestMethod]
        public void VirtualEventField_CodeFix() =>
            builder.WithCodeFix<VirtualEventFieldCodeFix>()
                .AddPaths("VirtualEventField.cs")
                .WithCodeFixedPaths("VirtualEventField.Fixed.cs")
                .VerifyCodeFix();
    }
}
