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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class ArgumentSyntaxExtensionsTest
{
    [TestMethod]
    [DataRow("($$a, b) = (42, 42);")]
    [DataRow("(a, $$b) = (42, 42);")]
    [DataRow("(a, (b, $$c)) = (42, (42, 42));")]
    [DataRow("(a, (b, ($$c, d))) = (42, (42, (42, 42)));")]
    public void IsInTupleAssignmentTarget_IsTrue(string assignment)
    {
        var code = $$"""
            public class Sample
            {
                public void Method()
                {
                    int a, b, c, d;
                    {{assignment}}
                }
            }
            """;
        var argument = GetTupleArgumentAtMarker(ref code);
        argument.IsInTupleAssignmentTarget().Should().BeTrue();
    }

    [TestMethod]
    public void IsInTupleAssignmentTarget_IsFalse()
    {
        const string code = """
            public class Sample
            {
                public void TupleArg((int, int) tuple) { }

                public void Method(int methodArgument)
                {
                    int a = 42;
                    var t = (a, methodArgument);
                    var x = (42, 42);
                    var nested = (42, (42, 42));
                    TupleArg((42, 42));
                    Method(0);
                }
            }
            """;
        var arguments = CSharpSyntaxTree.ParseText(code).GetRoot().DescendantNodes().OfType<ArgumentSyntax>().ToArray();

        arguments.Should().HaveCount(12);
        foreach (var argument in arguments)
        {
            argument.IsInTupleAssignmentTarget().Should().BeFalse();
        }
    }

    [TestMethod]
    // Simple tuple
    [DataRow("($$1, (2, 3))", "(1, (2, 3))")]
    [DataRow("(1, ($$2, 3))", "(1, (2, 3))")]
    [DataRow("(1, (2, $$3))", "(1, (2, 3))")]
    // With method call with single argument
    [DataRow("($$1, (M(2), 3))", "(1, (M(2), 3))")]
    [DataRow("(1, ($$M(2), 3))", "(1, (M(2), 3))")]
    [DataRow("(1, (M($$2), 3))", null)]
    // With method call with two arguments
    [DataRow("(1, $$M(2, 3))", "(1, M(2, 3))")]
    [DataRow("(1, M($$2, 3))", null)]
    [DataRow("(1, M(2, $$3))", null)]
    // With method call with tuple argument
    [DataRow("($$M((1, 2)), 3)", "(M((1, 2)), 3)")]
    [DataRow("(M($$(1, 2)), 3)", null)]
    [DataRow("(M(($$1, 2)), 3)", "(1, 2)")]
    public void OutermostTuple_DifferentPositions(string tuple, string expectedOuterTuple)
    {
        var code = $$"""
            public class C
            {
                public void Test()
                {
                    _ = {{tuple}};
                }

                static int M(int a) => 0;
                static int M(int a, int b) => 0;
                static int M((int a, int b) t) => 0;
            }
            """;
        var argument = GetTupleArgumentAtMarker(ref code);
        var outerMostTuple = argument.OutermostTuple();
        if (expectedOuterTuple is null)
        {
            outerMostTuple.Should().BeNull();
        }
        else
        {
            outerMostTuple.Should().NotBeNull();
            outerMostTuple.Value.SyntaxNode.ToString().Should().Be(expectedOuterTuple);
        }
    }

    private static ArgumentSyntax GetTupleArgumentAtMarker(ref string code)
    {
        var nodePosition = code.IndexOf("$$");
        code = code.Replace("$$", string.Empty);
        var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
        tree.GetDiagnostics().Should().BeEmpty();
        var nodeAtPosition = tree.GetRoot().FindNode(new TextSpan(nodePosition, 0), getInnermostNodeForTie: true);
        var argument = nodeAtPosition?.AncestorsAndSelf().OfType<ArgumentSyntax>().First();
        return argument;
    }
}
