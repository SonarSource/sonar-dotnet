/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class AssignmentExpressionSyntaxExtensionsTest
{
    [TestMethod]
    public void MapAssignmentArguments_TupleElementsAreExtracted() =>
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

    [TestMethod]
    public void MapAssignmentArguments_SimpleAssignmentReturnsSingleElementArray() =>
        AssertMapAssignmentArguments("int x; x = 1;", new[]
            {
                new
                {
                    Left = WithIdentifier("x"),
                    Right = WithToken("1"),
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_NestedDeconstruction() =>
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

    [TestMethod]
    public void MapAssignmentArguments_RightSideNotATupleExpression() =>
        AssertMapAssignmentArguments("(var x, var y) = M(); static (int, int) M() => (1, 2);", new[]
            {
                new
                {
                    Left = WithDesignationArguments("x", "y"),
                    Right = new { Expression = WithIdentifier("M") },
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_LeftSideNotATupleExpression() =>
        AssertMapAssignmentArguments("(int, int) tuple; tuple = (1, 2);", new[]
            {
                new
                {
                    Left = WithIdentifier("tuple"),
                    Right = WithTokenArguments("1", "2"),
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_SimpleDeconstructionAssignment() =>
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

    [TestMethod]
    public void MapAssignmentArguments_NestedDeconstructionAssignment() =>
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

    [TestMethod]
    public void MapAssignmentArguments_MisalignedDeconstructionAssignment() =>
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
                                WithIdentifierVariables("b", "c", "d"),
                            },
                        },
                    },
                    Right = new
                    {
                        Arguments = new object[]
                        {
                            new { Expression = WithToken("1") },
                            new { Expression = WithTokenArguments("2", "3") },
                            new { Expression = WithToken("4") },
                        }
                    },
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_MisalignedDeconstructionAssignmentInNestedTuple() =>
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
                                WithIdentifierVariables("b", "c", "d"),
                            },
                        },
                    },
                    Right = new
                    {
                        Arguments = new object[]
                        {
                            new { Expression = WithToken("1") },
                            new { Expression = WithTokenArguments("2", "3") },
                        }
                    },
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_MixedAssignment() =>
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

    [TestMethod]
    public void MapAssignmentArguments_MisalignedLeft() =>
        AssertMapAssignmentArguments("(var x, var y) = (1, 2, 3);", new[]
            {
                new
                {
                    Left = WithDesignationArguments("x", "y"),
                    Right = WithTokenArguments("1", "2", "3"),
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_MisalignedRight() =>
        AssertMapAssignmentArguments("(var x, var y, var z) = (1, 2);", new[]
            {
                new
                {
                    Left = WithDesignationArguments("x", "y", "z"),
                    Right = WithTokenArguments("1", "2"),
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_MisalignedNested1() =>
        AssertMapAssignmentArguments("(var a, (var b, var c)) = (1, (2, 3, 4));", new[]
            {
                new
                {
                    Left = new
                    {
                        Arguments = new object[]
                        {
                            new { Expression = WithDesignation("a") },
                            new { Expression = WithDesignationArguments("b", "c") },
                        }
                    },
                    Right = new
                    {
                        Arguments = new object[]
                        {
                            new { Expression = WithToken("1") },
                            new { Expression = WithTokenArguments("2", "3", "4") },
                        }
                    },
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_MisalignedNested2() =>
        AssertMapAssignmentArguments("(var a, (var b, var c), var d) = (1, (2, 3, 4));", new[]
            {
                new
                {
                    Left = new
                    {
                        Arguments = new object[]
                        {
                            new { Expression = WithDesignation("a") },
                            new { Expression = WithDesignationArguments("b", "c") },
                            new { Expression = WithDesignation("d") },
                        }
                    },
                    Right = new
                    {
                        Arguments = new object[]
                        {
                            new { Expression = WithToken("1") },
                            new { Expression = WithTokenArguments("2", "3", "4") },
                        }
                    },
                },
            });

    [TestMethod]
    public void MapAssignmentArguments_DifferentConventions() =>
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

    [DataTestMethod]
    // Tuples.
    [DataRow("(var a, (x, var b)) = (0, (x++, 1));", "var a | 0", "x | x++", "var b | 1")]
    [DataRow("(var a, (var b, var c, var d), var e) = (0, (1, 2, 3), 4);", "var a | 0", "var b | 1", "var c | 2", "var d | 3", "var e | 4")]
    // Designation.
    [DataRow("var (a, (b, c)) = (0, (1, 2));", "a | 0", "b | 1", "c | 2")]
    [DataRow("var (a, (b, _)) = (0, (1, 2));", "a | 0", "b | 1", "_ | 2")]
    [DataRow("var (a, _) = (0, (1, 2));", "a | 0", "_ | (1, 2)")]
    // Unaligned tuples.
    [DataRow("(var a, var b) = (0, 1, 2);", "(var a, var b) | (0, 1, 2)")]
    [DataRow("(var a, var b) = (0, 1, 2);", "(var a, var b) | (0, 1, 2)")]
    [DataRow("(var a, var b) = (0, (1, 2));", "var a | 0", "var b | (1, 2)")]
    [DataRow("(var a, (var b, var c)) = (0, 1);", "var a | 0", "(var b, var c) | 1")] // Syntacticly correct
    [DataRow("(var a, var b, var c) = (0, (1, 2));", "(var a, var b, var c) | (0, (1, 2))")]
    [DataRow("(var a, (var b, var c)) = (0, 1, 2);", "(var a, (var b, var c)) | (0, 1, 2)")]
    // Unaligned designation.
    [DataRow("var (a, (b, c)) = (0, (1, 2, 3));", "var (a, (b, c)) | (0, (1, 2, 3))")]
    [DataRow("var (a, (b, c)) = (0, (1, (2, 3)));", "a | 0", "b | 1", "c | (2, 3)")]
    [DataRow("var (a, (b, (c, d))) = (0, (1, 2));", "a | 0", "b | 1", "(c, d) | 2")]
    [DataRow("var (a, (b, c, d)) = (0, (1, 2));", "var (a, (b, c, d)) | (0, (1, 2))")]
    // Mixed.
    [DataRow("(var a, var (b, c)) = (0, (1, 2));", "var a | 0", "b | 1", "c | 2")]
    [DataRow("(var a, var (b, (c, (d, e)))) = (0, (1, (2, (3, 4))));", "var a | 0", "b | 1", "c | 2", "d | 3", "e | 4")]
    [DataRow("(var a, (var b, var (c, d))) = (0, (1, (2, 3)));", "var a | 0", "var b | 1", "c | 2", "d | 3")]
    public void MapAssignmentArguments_DataTest(string code, params string[] pairs)
    {
        var actualMapping = ParseAssignmentExpression(code).MapAssignmentArguments();
        var actualMappingPairs = actualMapping.Select(x => $"{x.Left} | {x.Right}");
        actualMappingPairs.Should().BeEquivalentTo(pairs);
    }

    [DataTestMethod]
    // Normal assignment
    [DataRow("int a; a = 1;", "a")]
    // Deconstruction into tuple
    [DataRow("(var a, var b) = (1, 2);", "a", "b")]
    [DataRow("(var a, var b) = (1, (2, 3));", "a", "b")]
    [DataRow("(var a, _) = (1, 2);", "a", "_")]  // "_" can refer to a local variable or be a discard.
    [DataRow("(var _, var _) = (1, 2);")]        // "var _" is always a discard.
    [DataRow("(var _, _) = (1, 2);", "_")]
    [DataRow("(_, _) = (1, 2);", "_", "_")]
    [DataRow("_ = (1, 2);", "_")]
    [DataRow("(var a, (var b, var c), var d) = (1, (2, 3), 4);", "a", "b", "c", "d")]
    [DataRow("int b; (var a, (b, var c), _) = (1, (2, 3), 4);", "a", "b", "c", "_")]
    [DataRow("(var a, (int, int) b) = (1, (2, 3));", "a", "b")]
    // Deconstruction into declaration expression designation
    [DataRow("var (a, b) = (1, 2);", "a", "b")]
    [DataRow("var (a, _) = (1, 2);", "a")]
    [DataRow("var (_, _) = (1, 2);")]
    // Mixed
    [DataRow("(var a, var (b, c), var d, (int, int) e) = (1, (2, 3), (4, 5), (6, 7));", "a", "b", "c", "d", "e")]
    public void AssignmentTargets_DeconstructTargets(string assignment, params string[] expectedTargets)
    {
        var allTargets = ParseAssignmentExpression(assignment).AssignmentTargets();
        var allTargetsAsString = allTargets.Select(x => x.ToString());
        allTargetsAsString.Should().BeEquivalentTo(expectedTargets);
    }

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

    private static object WithTokenArguments(params string[] tokens) =>
        new { Arguments = tokens.Select(x => new { Expression = WithToken(x) }) };

    private static object WithDesignationArguments(params string[] designations) =>
        new { Arguments = designations.Select(x => new { Expression = WithDesignation(x) }) };

    private static object WithIdentifierVariables(params string[] identifier) =>
        new { Variables = identifier.Select(x => WithIdentifier(x)) };

    private static AssignmentExpressionSyntax ParseAssignmentExpression(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText($@"
public class C
{{
    public void M()
    {{
        {code}
    }}
}}");
        syntaxTree.GetDiagnostics().Should().BeEmpty();
        return syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().Single();
    }
}
