﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MemberInitializerRedundantTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MemberInitializerRedundant>();
        private readonly VerifierBuilder builderSonarCfg = new VerifierBuilder().AddAnalyzer(() => new MemberInitializerRedundant(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg));

        [TestMethod]
        public void MemberInitializerRedundant_RoslynCfg() =>
            builder.AddPaths(@"MemberInitializerRedundant.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        public void MemberInitializerRedundant_RoslynCfg_FlowCaptureOperationNotSupported() =>
            builder.AddPaths(@"MemberInitializerRedundant.RoslynCfg.FlowCaptureBug.cs").WithOptions(ParseOptionsHelper.FromCSharp8).VerifyNoIssues();

        [TestMethod]
        public void MemberInitializerRedundant_SonarCfg() =>
            builderSonarCfg.AddPaths(@"MemberInitializerRedundant.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        public void MemberInitializerRedundant_CodeFix() =>
            builder
                .WithCodeFix<MemberInitializedToDefaultCodeFix>()
                .AddPaths("MemberInitializerRedundant.cs")
                .WithCodeFixedPaths("MemberInitializerRedundant.Fixed.cs")
                .VerifyCodeFix();

#if NET

        [TestMethod]
        public void MemberInitializerRedundant_CSharp9() =>
            builder.AddPaths("MemberInitializerRedundant.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void MemberInitializerRedundant_CSharp9_CodeFix() =>
            builder
                .WithCodeFix<MemberInitializedToDefaultCodeFix>()
                .AddPaths("MemberInitializerRedundant.CSharp9.cs")
                .WithCodeFixedPaths("MemberInitializerRedundant.CSharp9.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .VerifyCodeFix();

        [TestMethod]
        public void MemberInitializerRedundant_CSharp10() =>
            builder.AddPaths("MemberInitializerRedundant.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void MemberInitializerRedundant_CSharp10_CodeFix() =>
            builder
                .WithCodeFix<MemberInitializedToDefaultCodeFix>()
                .AddPaths("MemberInitializerRedundant.CSharp10.cs")
                .WithCodeFixedPaths("MemberInitializerRedundant.CSharp10.Fixed.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .VerifyCodeFix();

        [TestMethod]
        public void MemberInitializerRedundant_CSharp12() =>
            builder.AddPaths("MemberInitializerRedundant.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

#endif

    }
}
