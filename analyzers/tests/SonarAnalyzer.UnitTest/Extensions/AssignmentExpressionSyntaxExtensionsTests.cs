/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class AssignmentExpressionSyntaxExtensionsTests
    {
        [TestMethod]
        public void AssignmentExpressionSyntaxExtensions_TupleElementsAreExtracted()
        {
            var code = "(var x, var y) = (1, 2);";
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            var mapping = assigment.MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "x" } } },
                        Right = new { Token = new { Text = "1" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "y" } } },
                        Right = new { Token = new { Text = "2" } },
                    },
                });
        }

        [TestMethod]
        public void AssignmentExpressionSyntaxExtensions_SimpleAssignmentReturnsSingleElementArray()
        {
            var code = "int x; x = 1;";
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            var mapping = assigment.MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Identifier = new { Text = "x" } },
                        Right = new { Token = new { Text = "1" } },
                    },
                });
        }

        [TestMethod]
        public void AssignmentExpressionSyntaxExtensions_NestedDeconstruction()
        {
            var code = "(var a, (var b, (var c, var d)), var e) = (1, (2, (3, 4)), 5);";
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            var mapping = assigment.MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "a" } } },
                        Right = new { Token = new { Text = "1" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "b" } } },
                        Right = new { Token = new { Text = "2" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "c" } } },
                        Right = new { Token = new { Text = "3" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "d" } } },
                        Right = new { Token = new { Text = "4" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "e" } } },
                        Right = new { Token = new { Text = "5" } },
                    },
                });
        }

        [TestMethod]
        public void AssignmentExpressionSyntaxExtensions_RightSideNotATupleExpression()
        {
            var code = "(var x, var y) = M(); static (int, int) M() => (1, 2);";
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            var mapping = assigment.MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new
                        {
                            Arguments = new[]
                            {
                                new { Expression = new { Designation = new { Identifier = new { Text = "x" } } } },
                                new { Expression = new { Designation = new { Identifier = new { Text = "y" } } } },
                            }
                        },
                        Right = new { Expression = new { Identifier = new { Text = "M" } } },
                    },
                });
        }

        [TestMethod]
        public void AssignmentExpressionSyntaxExtensions_MisalignedLeft()
        {
            var code = "(var x, var y) = (1, 2, 3);";
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            var mapping = assigment.MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "x" } } },
                        Right = new { Token = new { Text = "1" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "y" } } },
                        Right = new { Token = new { Text = "2" } },
                    },
                });
        }

        [TestMethod]
        public void AssignmentExpressionSyntaxExtensions_MisalignedRight()
        {
            var code = "(var x, var y, var z) = (1, 2);";
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            var mapping = assigment.MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "x" } } },
                        Right = new { Token = new { Text = "1" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "y" } } },
                        Right = new { Token = new { Text = "2" } },
                    },
                });
        }

        [TestMethod]
        public void AssignmentExpressionSyntaxExtensions_MisalignedNested()
        {
            var code = "(var a, (var b, var c, var d), var e) = (1, (2, 3));";
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            var mapping = assigment.MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "a" } } },
                        Right = new { Token = new { Text = "1" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "b" } } },
                        Right = new { Token = new { Text = "2" } },
                    },
                    new
                    {
                        Left = new { Designation = new { Identifier = new { Text = "c" } } },
                        Right = new { Token = new { Text = "3" } },
                    },
                });
        }

        [DataTestMethod]
        // Normal assignment
        [DataRow("int a; a = 1;", "a")]
        // Deconstruction into tuple
        [DataRow("(var a, var b) = (1, 2);", "var a,var b")]
        [DataRow("(var a, _) = (1, 2);", "var a,_")]
        [DataRow("(var _, _) = (1, 2);", "var _,_")]
        [DataRow("(_, _) = (1, 2);", "_,_")]
        [DataRow("(var a, (var b, var c), var d) = (1, (2, 3), 4);", "var a,var b,var c,var d")]
        [DataRow("int b; (var a, (b, var c), _) = (1, (2, 3), 4);", "var a,b,var c,_")]
        // Deconstruction into declaration expression designation
        [DataRow("var (a, b) = (1, 2);", "a,b")]
        [DataRow("var (a, _) = (1, 2);", "a")]
        [DataRow("var (_, _) = (1, 2);", "")]
        public void AssignmentExpressionSyntaxExtensions_AssignmentTargets_DeconstructTargets(string assignment, string expectedTargets)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(assignment));
            var assignmentExpression = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().First();
            var allTargets = assignmentExpression.AssignmentTargets();
            var allTargetsAsString = string.Join(",", allTargets.Select(x => x.ToString()));
            allTargetsAsString.Should().Be(expectedTargets);
        }

        private static string WrapInMethod(string code) =>
$@"
public class C
{{
    public void M()
    {{
        {code}
    }}
}}
";
    }
}
