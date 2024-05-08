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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class GetTypeWithIsAssignableFromTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<GetTypeWithIsAssignableFrom>();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.cs").Verify();

#if NET

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp9() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp9_CodeFix() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp9.cs")
            .WithCodeFix<GetTypeWithIsAssignableFromCodeFix>()
            .WithCodeFixedPaths("GetTypeWithIsAssignableFrom.CSharp9.Fixed.cs")
            .WithTopLevelStatements()
            .VerifyCodeFix();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp10() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp10.cs")
            .WithTopLevelStatements()
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .Verify();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp10_CodeFix() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp10.cs")
            .WithCodeFix<GetTypeWithIsAssignableFromCodeFix>()
            .WithCodeFixedPaths("GetTypeWithIsAssignableFrom.CSharp10.Fixed.cs")
            .WithTopLevelStatements()
            .WithOptions(ParseOptionsHelper.FromCSharp10)
            .VerifyCodeFix();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp11() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp11.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .VerifyNoIssues();  // list pattern is compliant

#endif

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CodeFix() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.cs")
            .WithCodeFix<GetTypeWithIsAssignableFromCodeFix>()
            .WithCodeFixedPaths("GetTypeWithIsAssignableFrom.Fixed.cs", "GetTypeWithIsAssignableFrom.Fixed.Batch.cs")
            .VerifyCodeFix();
}
