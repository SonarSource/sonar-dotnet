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
            AssertMapAssignmentArguments("(var x, var y) = (1, 2);", new[]
                {
                    new
                    {
                        Left = WithDesignation("x"),
                        Right = WithToken("1"),
                    },
                    new
                    {
                        Left = WithDesignation("y"),
                        Right = WithToken("2"),
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_SimpleAssignmentReturnsSingleElementArray()
        {
            AssertMapAssignmentArguments("int x; x = 1;", new[]
                {
                    new
                    {
                        Left = WithIdentifier("x"),
                        Right = WithToken("1"),
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_NestedDeconstruction()
        {
            AssertMapAssignmentArguments("(var a, (var b, (var c, var d)), var e) = (1, (2, (3, 4)), 5);", new[]
                {
                    new
                    {
                        Left = WithDesignation("a"),
                        Right = WithToken("1"),
                    },
                    new
                    {
                        Left = WithDesignation("b"),
                        Right = WithToken("2"),
                    },
                    new
                    {
                        Left = WithDesignation("c"),
                        Right = WithToken("3"),
                    },
                    new
                    {
                        Left = WithDesignation("d"),
                        Right = WithToken("4"),
                    },
                    new
                    {
                        Left = WithDesignation("e"),
                        Right = WithToken("5"),
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_RightSideNotATupleExpression()
        {
            AssertMapAssignmentArguments("(var x, var y) = M(); static (int, int) M() => (1, 2);", new[]
                {
                    new
                    {
                        Left = new
                        {
                            Arguments = new[]
                            {
                                new { Expression = WithDesignation("x") },
                                new { Expression = WithDesignation("y") },
                            }
                        },
                        Right = new { Expression = WithIdentifier("M") },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_LeftSideNotATupleExpression()
        {
            AssertMapAssignmentArguments("(int, int) tuple; tuple = (1, 2);", new[]
                {
                    new
                    {
                        Left = WithIdentifier("tuple"),
                        Right = new
                        {
                            Arguments = new[]
                            {
                                new { Expression = WithToken("1") },
                                new { Expression = WithToken("2") },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_SimpleDeconstructionAssignment()
        {
            AssertMapAssignmentArguments("var (x, y) =  (1, 2);", new[]
                {
                    new
                    {
                        Left = WithIdentifier("x"),
                        Right = WithToken("1"),
                    },
                    new
                    {
                        Left = WithIdentifier("y"),
                        Right = WithToken("2"),
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_NestedDeconstructionAssignment()
        {
            AssertMapAssignmentArguments("var (a, (b, c), d) =  (1, (2, 3), 4);", new[]
                {
                    new
                    {
                        Left = WithIdentifier("a"),
                        Right = WithToken("1"),
                    },
                    new
                    {
                        Left = WithIdentifier("b"),
                        Right = WithToken("2"),
                    },
                    new
                    {
                        Left = WithIdentifier("c"),
                        Right = WithToken("3"),
                    },
                    new
                    {
                        Left = WithIdentifier("d"),
                        Right = WithToken("4"),
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedDeconstructionAssignment()
        {
            AssertMapAssignmentArguments("var (a, (b, c, d)) =  (1, (2, 3), 4);", new[]
                {
                    new
                    {
                        Left = new
                        {
                            Designation = new
                            {
                                Variables = new object[]
                                {
                                    WithIdentifier("a"),
                                    new
                                    {
                                        Variables = new[]
                                        {
                                            WithIdentifier("b"),
                                            WithIdentifier("c"),
                                            WithIdentifier("d"),
                                        }
                                    },
                                },
                            },
                        },
                        Right = new
                        {
                            Arguments = new object[]
                            {
                                new { Expression = WithToken("1") },
                                new
                                {
                                    Expression = new
                                    {
                                        Arguments = new object[]
                                        {
                                            new { Expression = WithToken("2") },
                                            new { Expression = WithToken("3") },
                                        },
                                    },
                                },
                                new { Expression = WithToken("4") },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedDeconstructionAssignmentInNestedTuple()
        {
            AssertMapAssignmentArguments("var (a, (b, c, d)) =  (1, (2, 3));", new object[]
                {
                    new
                    {
                        Left = new
                        {
                            Designation = new
                            {
                                Variables = new object[]
                                {
                                    WithIdentifier("a"),
                                    new
                                    {
                                        Variables = new[]
                                        {
                                            WithIdentifier("b"),
                                            WithIdentifier("c"),
                                            WithIdentifier("d"),
                                        }
                                    },
                                },
                            },
                        },
                        Right = new
                        {
                            Arguments = new object[]
                            {
                                new { Expression = WithToken("1") },
                                new
                                {
                                    Expression = new
                                    {
                                        Arguments = new object[]
                                        {
                                            new { Expression = WithToken("2") },
                                            new { Expression = WithToken("3") },
                                        },
                                    },
                                },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MixedAssignment()
        {
            AssertMapAssignmentArguments("int a; (a, var b) =  (1, 2);", new[]
            {
                new
                {
                    Left = WithIdentifier("a"),
                    Right = WithToken("1"),
                },
                new
                {
                    Left = WithDesignation("b"),
                    Right = WithToken("2"),
                },
            });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedLeft()
        {
            AssertMapAssignmentArguments("(var x, var y) = (1, 2, 3);", new[]
                {
                    new
                    {
                        Left = new
                        {
                            Arguments = new[]
                            {
                                new { Expression = WithDesignation("x") },
                                new { Expression = WithDesignation("y") },
                            }
                        },
                        Right =new
                        {
                            Arguments = new[]
                            {
                                new { Expression = WithToken("1") },
                                new { Expression = WithToken("2") },
                                new { Expression = WithToken("3") },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedRight()
        {
            AssertMapAssignmentArguments("(var x, var y, var z) = (1, 2);", new[]
                {
                    new
                    {
                        Left = new
                        {
                            Arguments = new[]
                            {
                                new { Expression = WithDesignation("x") },
                                new { Expression = WithDesignation("y") },
                                new { Expression = WithDesignation("z") },
                            }
                        },
                        Right =new
                        {
                            Arguments = new[]
                            {
                                new { Expression = WithToken("1") },
                                new { Expression = WithToken("2") },
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedNested1()
        {
            AssertMapAssignmentArguments("(var a, (var b, var c)) = (1, (2, 3, 4));", new[]
                {
                    new
                    {
                        Left = new
                        {
                            Arguments = new object[]
                            {
                                new { Expression = WithDesignation("a") },
                                new
                                {
                                    Expression = new
                                    {
                                        Arguments = new[]
                                        {
                                            new { Expression = WithDesignation("b") },
                                            new { Expression = WithDesignation("c") },
                                        }
                                    }
                                }
                            }
                        },
                        Right = new
                        {
                            Arguments = new object[]
                            {
                                new { Expression = WithToken("1") },
                                new
                                {
                                    Expression = new
                                    {
                                        Arguments = new[]
                                        {
                                            new { Expression = WithToken("2") },
                                            new { Expression = WithToken("3") },
                                            new { Expression = WithToken("4") },
                                        }
                                    }
                                }
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_MisalignedNested2()
        {
            AssertMapAssignmentArguments("(var a, (var b, var c), var d) = (1, (2, 3, 4));", new[]
                {
                    new
                    {
                        Left = new
                        {
                            Arguments = new object[]
                            {
                                new { Expression = WithDesignation("a") },
                                new
                                {
                                    Expression = new
                                    {
                                        Arguments = new[]
                                        {
                                            new { Expression = WithDesignation("b") },
                                            new { Expression = WithDesignation("c") },
                                        }
                                    }
                                },
                                new { Expression = WithDesignation("d") },
                            }
                        },
                        Right = new
                        {
                            Arguments = new object[]
                            {
                                new { Expression = WithToken("1") },
                                new
                                {
                                    Expression = new
                                    {
                                        Arguments = new[]
                                        {
                                            new { Expression = WithToken("2") },
                                            new { Expression = WithToken("3") },
                                            new { Expression = WithToken("4") },
                                        }
                                    }
                                }
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void MapAssignmentArguments_DifferentConventions()
        {
            AssertMapAssignmentArguments(
                @"int a;
                int M() => 1;
                ((a), (var b, _), object c, double d, _) = (1, (two: 2, (3)), string.Empty, M(), new object());", new object[]
                {
                    new
                    {
                        Left = new { Expression = WithIdentifier("a") },
                        Right = WithToken("1"),
                    },
                    new
                    {
                        Left = WithDesignation("b"),
                        Right = WithToken("2"),
                    },
                    new
                    {
                        Left = WithIdentifier("_"),
                        Right = new { Expression = WithToken("3") },
                    },
                    new
                    {
                        Left = WithDesignation("c"),
                        Right = new
                        {
                            Expression = new { Keyword = new { Text = "string" } },
                            Name = WithIdentifier("Empty"),
                        },
                    },
                    new
                    {
                        Left = WithDesignation("d"),
                        Right = new { Expression = WithIdentifier("M") },
                    },
                    new
                    {
                        Left = WithIdentifier("_"),
                        Right = new { Type = new { Keyword = new { Text = "object" } } },
                    },
                });
        }

        [DataTestMethod]
        // Normal assignment
        [DataRow("int a; a = 1;", "a")]
        // Deconstruction into tuple
        [DataRow("(var a, var b) = (1, 2);", "var a", "var b")]
        [DataRow("(var a, _) = (1, 2);", "var a", "_")]
        [DataRow("(var _, _) = (1, 2);", "var _", "_")]
        [DataRow("(_, _) = (1, 2);", "_", "_")]
        [DataRow("(var a, (var b, var c), var d) = (1, (2, 3), 4);", "var a", "var b", "var c", "var d")]
        [DataRow("int b; (var a, (b, var c), _) = (1, (2, 3), 4);", "var a", "b", "var c", "_")]
        // Deconstruction into declaration expression designation
        [DataRow("var (a, b) = (1, 2);", "a", "b")]
        [DataRow("var (a, _) = (1, 2);", "a")]
        [DataRow("var (_, _) = (1, 2);")]
        public void AssignmentTargets_DeconstructTargets(string assignment, params string[] expectedTargets)
        {
            var allTargets = ParseAssignmentExpression(assignment).AssignmentTargets();
            var allTargetsAsString = allTargets.Select(x => x.ToString());
            allTargetsAsString.Should().BeEquivalentTo(expectedTargets);
        }

        #region Helpers

        private static void AssertMapAssignmentArguments<T>(string code, T[] expectation)
        {
            var mapping = ParseAssignmentExpression(code).MapAssignmentArguments();
            mapping.Should().BeEquivalentTo(expectation);
        }

        private static object WithDesignation(string identifier) =>
            new { Designation = WithIdentifier(identifier) };

        private static object WithIdentifier(string identifier) =>
            new { Identifier = new { Text = identifier } };

        private static object WithToken(string identifier) =>
            new { Token = new { Text = identifier } };

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

        #endregion

    }
}
