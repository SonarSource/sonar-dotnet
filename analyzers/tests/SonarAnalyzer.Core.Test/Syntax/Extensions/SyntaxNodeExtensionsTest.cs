﻿/*
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Core.Syntax.Extensions.Test;

[TestClass]
public class SyntaxNodeExtensionsTest
{
    [TestMethod]
    public void LineNumberToReport()
    {
        const string code = """
         namespace Test
         {
             class Sample { }
         }
         """;
        var method = CSharpSyntaxTree.ParseText(code).Single<ClassDeclarationSyntax>();
        method.LineNumberToReport().Should().Be(3);
        method.GetLocation().GetLineSpan().StartLinePosition.LineNumberToReport().Should().Be(3);
    }
}
