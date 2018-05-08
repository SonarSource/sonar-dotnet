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

extern alias csharp;
using csharp::SonarAnalyzer.Security.Ucfg;
using csharp::SonarAnalyzer.SymbolicExecution.ControlFlowGraph;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Protobuf.Ucfg;
using ULocation = SonarAnalyzer.Protobuf.Ucfg.Location;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class UniversalControlFlowGraphBuilder_GetLocation
    {
        [TestMethod]
        public void GetLocation_Returns_1Based_Line_0Based_LineOffset()
        {
            const string code = @"  // 1 - SQ line numbers
public class Class1                 // 2
{                                   // 3
    public string Foo(string s)     // 4
    {                               // 5
        var x = s.Trim();           // 6 %0 = string.Trim(s)    --> SL = 6, SLO = 16, EL = 6, ELO = 23
                                    // 7 x = __id(%0)           --> SL = 6, SLO = 12, EL = 6, ELO = 23
        return x;                   // 8
//23456789012345678901234567890     // SQ column offsets
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            var block = ucfg.BasicBlocks[0];

            // Block locations are not used by the Security engine
            ucfg.BasicBlocks[0].Location.Should().BeNull();

            AssertLocation(ucfg.BasicBlocks[0].Instructions[0].Location,
                startLine: 6, startLineOffset: 16, endLine: 6, endLineOffset: 23);
            AssertLocation(ucfg.BasicBlocks[0].Instructions[1].Location,
                startLine: 6, startLineOffset: 12, endLine: 6, endLineOffset: 23);
        }

        [TestMethod]
        public void GetLocation_Multiline_Invocation_StartAtZero_EndAtZero()
        {
            const string code = @"  // 1 - SQ line numbers
public class Class1                 // 2
{                                   // 3
    public string Foo(string s)     // 4
    {                               // 5
        var x =                     // 6 %0 = string.Trim(s)    --> SL = 7, SLO = 0, EL = 8, ELO = 0
s.Trim(                             // 7 x = __id(%0)           --> SL = 6, SLO = 12, EL = 8, ELO = 0
);                                  // 8
                                    // 9
                                    // 10
        return x;                   // 11
//23456789012345678901234567890     // SQ column offsets
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            var block = ucfg.BasicBlocks[0];

            // Block locations are not used by the Security engine
            ucfg.BasicBlocks[0].Location.Should().BeNull();

            AssertLocation(ucfg.BasicBlocks[0].Instructions[0].Location,
                startLine: 7, startLineOffset: 0, endLine: 8, endLineOffset: 0);
            AssertLocation(ucfg.BasicBlocks[0].Instructions[1].Location,
                startLine: 6, startLineOffset: 12, endLine: 8, endLineOffset: 0);
        }

        private static void AssertLocation(ULocation location, int startLine, int startLineOffset, int endLine, int endLineOffset)
        {
            location.StartLine.Should().Be(startLine);
            location.StartLineOffset.Should().Be(startLineOffset);
            location.EndLine.Should().Be(endLine);
            location.EndLineOffset.Should().Be(endLineOffset);
        }

        private UCFG GetUcfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(methodName);

            var builder = new UniversalControlFlowGraphBuilder(semanticModel,
                CSharpControlFlowGraph.Create(method.Body, semanticModel));

            return builder.Build(method, semanticModel.GetDeclaredSymbol(method));
        }
    }
}
