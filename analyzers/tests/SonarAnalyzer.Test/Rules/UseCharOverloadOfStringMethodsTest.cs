/*
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseCharOverloadOfStringMethodsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseCharOverloadOfStringMethods>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseCharOverloadOfStringMethods>();

#if NETFRAMEWORK

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS() =>
        builderCS.AddPaths("UseCharOverloadOfStringMethods.Framework.cs").Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_VB() =>
        builderVB.AddPaths("UseCharOverloadOfStringMethods.Framework.vb").VerifyNoIssues();

#else

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS() =>
        builderCS.AddPaths("UseCharOverloadOfStringMethods.cs").Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS_Latest() =>
        builderCS.AddPaths("UseCharOverloadOfStringMethods.Latest.cs")
        .WithOptions(ParseOptionsHelper.CSharpLatest)
        .Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_VB() =>
        builderVB.AddPaths("UseCharOverloadOfStringMethods.vb").Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS_Fix() =>
        builderCS
        .WithCodeFix<CS.UseCharOverloadOfStringMethodsCodeFix>()
        .AddPaths("UseCharOverloadOfStringMethods.ToFix.cs")
        .WithCodeFixedPaths("UseCharOverloadOfStringMethods.Fixed.cs")
        .VerifyCodeFix();

#endif

}
