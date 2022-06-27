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
        public void MapAssignmentArguments_TupleElementsAreExtracted()
        {
            var code = "(var x, var y) = (1, 2);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
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
        public void MapAssignmentArguments_SimpleAssignmentReturnsSingleElementArray()
        {
            var code = "int x; x = 1;";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
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
        public void MapAssignmentArguments_NestedDeconstruction()
        {
            var code = "(var a, (var b, (var c, var d)), var e) = (1, (2, (3, 4)), 5);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
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
        public void MapAssignmentArguments_RightSideNotATupleExpression()
        {
            var code = "(var x, var y) = M(); static (int, int) M() => (1, 2);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
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
        public void MapAssignmentArguments_LeftSideNotATupleExpression()
        {
            var code = "(int, int) tuple; tuple = (1, 2);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Identifier = new { Text = "tuple" } },
                        Right = new
                        {
                            Arguments = new[]
                            {
                                new { Expression = new { Token = new { Text = "1" } } },
                                new { Expression = new { Token = new { Text = "2" } } },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_SimpleDeconstructionAssignment()
        {
            var code = "var (x, y) =  (1, 2);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Identifier = new { Text = "x" } },
                        Right = new { Token = new { Text = "1" } },
                    },
                    new
                    {
                        Left = new { Identifier = new { Text = "y" } },
                        Right = new { Token = new { Text = "2" } },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_NestedDeconstructionAssignment()
        {
            var code = "var (a, (b, c), d) =  (1, (2, 3), 4);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Identifier = new { Text = "a" } },
                        Right = new { Token = new { Text = "1" } },
                    },
                    new
                    {
                        Left = new { Identifier = new { Text = "b" } },
                        Right = new { Token = new { Text = "2" } },
                    },
                    new
                    {
                        Left = new { Identifier = new { Text = "c" } },
                        Right = new { Token = new { Text = "3" } },
                    },
                    new
                    {
                        Left = new { Identifier = new { Text = "d" } },
                        Right = new { Token = new { Text = "4" } },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedDeconstructionAssignment()
        {
            var code = "var (a, (b, c, d)) =  (1, (2, 3), 4);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Designation = new { Variables = new { Count = 2 } } },
                        Right = new { Arguments = new { Count = 3 } },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedDeconstructionAssignmentInNestedTuple()
        {
            var code = "var (a, (b, c, d)) =  (1, (2, 3));";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Designation = new { Variables = new { Count = 2 } } },
                        Right = new { Arguments = new { Count = 2 } },
                    },
                });
            mapping[0].Left.ToString().Should().Be("var (a, (b, c, d))");
            mapping[0].Right.ToString().Should().Be("(1, (2, 3))");
        }

        [TestMethod]
        public void MapAssignmentArguments_MixedAssignment()
        {
            var code = "int a; (a, var b) =  (1, 2);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().HaveCount(2);
            mapping[0].Should().BeEquivalentTo(new
            {
                Left = new { Identifier = new { Text = "a" } },
                Right = new { Token = new { Text = "1" } },
            });
            mapping[1].Should().BeEquivalentTo(new
            {
                Left = new { Designation = new { Identifier = new { Text = "b" } } },
                Right = new { Token = new { Text = "2" } },
            });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedLeft()
        {
            var code = "(var x, var y) = (1, 2, 3);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
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
                        Right =new
                        {
                            Arguments = new[]
                            {
                                new { Expression = new { Token = new { Text = "1" } } },
                                new { Expression = new { Token = new { Text = "2" } } },
                                new { Expression = new { Token = new { Text = "3" } } },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedRight()
        {
            var code = "(var x, var y, var z) = (1, 2);";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
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
                                new { Expression = new { Designation = new { Identifier = new { Text = "z" } } } },
                            }
                        },
                        Right =new
                        {
                            Arguments = new[]
                            {
                                new { Expression = new { Token = new { Text = "1" } } },
                                new { Expression = new { Token = new { Text = "2" } } },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedNested1()
        {
            var code = "(var a, (var b, var c)) = (1, (2, 3, 4));";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Arguments = new { Count = 2 } },
                        Right = new { Arguments = new { Count = 2 } },
                    },
                });
            mapping[0].Left.ToString().Should().Be("(var a, (var b, var c))");
            mapping[0].Right.ToString().Should().Be("(1, (2, 3, 4))");
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedNested2()
        {
            var code = "(var a, (var b, var c), var d) = (1, (2, 3, 4));";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        Left = new { Arguments = new { Count = 3 } },
                        Right = new { Arguments = new { Count = 2 } },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_DifferentConventions()
        {
            var code = @"int a;
                         M() => 1;
                         ((a), (var b, _), object c, double d, _) = (1, (two: 2, (3)), string.Empty, M(), new object());";
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().HaveCount(6);
            mapping[0].Should().BeEquivalentTo(new
            {
                Left = new { Expression = new { Identifier = new { Text = "a" } } },
                Right = new { Token = new { Text = "1" } },
            }, "first element is an assignment");
            mapping[1].Should().BeEquivalentTo(new
            {
                Left = new { Designation = new { Identifier = new { Text = "b" } } },
                Right = new { Token = new { Text = "2" } },
            }, "second element is a declaration");
            mapping[2].Should().BeEquivalentTo(new
            {
                Left = new { Identifier = new { Text = "_" } },
                Right = new { Expression = new { Token = new { Text = "3" } } },
            }, "third element is a discard");
            mapping[3].Should().BeEquivalentTo(new
            {
                Left = new { Designation = new { Identifier = new { Text = "c" } } },
                Right = new
                {
                    Expression = new { Keyword = new { Text = "string" } },
                    Name = new { Identifier = new { Text = "Empty" } },
                },
            }, "fourth element is a declaration with an assignment of a static property");
            mapping[4].Should().BeEquivalentTo(new
            {
                Left = new { Designation = new { Identifier = new { Text = "d" } } },
                Right = new { Expression = new { Identifier = new { Text = "M" } } },
            }, "fifth element is declaration with conversion of a method result");
            mapping[5].Should().BeEquivalentTo(new
            {
                Left = new { Identifier = new { Text = "_" } },
                Right = new { Type = new { Keyword = new { Text = "object" } } },
            }, "sixth element is a discard of an object creation");
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
        public void AssignmentTargets_DeconstructTargets(string assignment, string expectedTargets)
        {
            var allTargets = ParseAssignmentExpression(assignment).AssignmentTargets();
            var allTargetsAsString = string.Join(",", allTargets.Select(x => x.ToString()));
            allTargetsAsString.Should().Be(expectedTargets);
        }

        private static AssignmentExpressionSyntax ParseAssignmentExpression(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(code));
            var assigment = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
            return assigment;
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
