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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CommentedOutCodeTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<CommentedOutCode>();

    [TestMethod]
    public void CommentedOutCode_Nonconcurrent() =>
        builder.AddPaths("CommentedOutCode_Nonconcurrent.cs").WithConcurrentAnalysis(false).Verify();

    [TestMethod]
    public void CommentedOutCode() =>
        builder.AddPaths("CommentedOutCode.cs").Verify();

    [TestMethod]
    public void CommentedOutCode_NoDocumentation() =>
        builder.AddPaths("CommentedOutCode.cs")
            .WithConcurrentAnalysis(false)
            .WithOptions(ImmutableArray.Create<ParseOptions>(new CSharpParseOptions(documentationMode: DocumentationMode.None)))
            .Verify();

    [TestMethod]
    public void CommentedOutCode_CodeFix_SingleLine() =>
        builder.AddPaths("CommentedOutCode.SingleLine.ToFix.cs")
            .WithCodeFix<CommentedOutCodeCodeFix>()
            .WithCodeFixedPaths("CommentedOutCode.SingleLine.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void CommentedOutCode_CodeFix_MultiLine() =>
       builder.AddPaths("CommentedOutCode.MultiLine.ToFix.cs")
           .WithCodeFix<CommentedOutCodeCodeFix>()
           .WithCodeFixedPaths("CommentedOutCode.MultiLine.Fixed.cs")
           .VerifyCodeFix();
}
