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

namespace SonarAnalyzer.CSharp.Core.Test.Extensions;

[TestClass]
public class TypeSyntaxExtensionsTest
{
    [TestMethod]
    [DataRow("""$$int$$ field;""", "int")]                                      // identity — no wrapper
    [DataRow("""$$int[]$$ field;""", "int")]                                    // array
    [DataRow("""$$int[][]$$ field;""", "int")]                                  // nested array
    [DataRow("""$$int?$$ field;""", "int")]                                     // nullable
    [DataRow("""$$int*$$ field;""", "int")]                                     // pointer
    [DataRow("""$$int?[]$$ field;""", "int")]                                   // nullable inside array
    [DataRow("""void M(ref int p) { $$ref int$$ x = ref p; }""", "int")]        // ref
    [DataRow("""void M(ref int p) { $$scoped ref int$$ x = ref p; }""", "int")] // scoped ref
    [DataRow("""$$List<int>$$ field;""", "List<int>")]                          // generic — not unwrapped
    public void Unwrap(string snippet, string expected)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            using System;
            using System.Collections.Generic;
            unsafe class Test
            {
                public Test(int i) { }
                {{snippet}}
            }
            """).Node;
        ((TypeSyntax)node).Unwrap().ToString().Should().Be(expected);
    }
}
