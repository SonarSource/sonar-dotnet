extern alias csharp;
/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2018 SonarSource SA
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

using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    [TestClass]
    public class UcfgSerializerTest
    {
        [TestMethod]
        public void Serialize_Some_Method()
        {
            var code = @"
class C
{
    void Foo(string a, string b)
    {
        var x = a + b;
        if (true)
        {
            Bar(a, 1);
        }
        else
        {
            Bar(b, 2);
        }
    }
    void Bar(string a, int x) { }
}
";
            var dot = UcfgSerializer.Serialize(UcfgVerifier.GetUcfgForMethod(code, "Foo"));

            dot.Should().BeIgnoringLineEndings(@"digraph ""C.Foo(string, string)"" {
ENTRY [shape=record label=""{ENTRY|a|b}""]
ENTRY -> 0
0 [shape=record label=""{BLOCK:#0|%0 := __concat [ a, b ]|x := __id [ %0 ]|TERMINATOR: JUMP: #1, #2}""]
0 -> 1
0 -> 2
1 [shape=record label=""{BLOCK:#1|%1 := C.Bar(string, int) [ this, a, CONST ]|TERMINATOR: JUMP: #3}""]
1 -> 3
2 [shape=record label=""{BLOCK:#2|%2 := C.Bar(string, int) [ this, b, CONST ]|TERMINATOR: JUMP: #3}""]
2 -> 3
3 [shape=record label=""{BLOCK:#3|TERMINATOR: RET: CONST}""]
3 -> END
END [shape=record label=""{END}""]
}
");
        }
    }
}
