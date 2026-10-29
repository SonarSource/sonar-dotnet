/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Trackers.Test;

[TestClass]
public class CSharpAssignmentFinderTest
{
    private static readonly CSharpAssignmentFinder Finder = new();

    [TestMethod]
    public void FindLinearPrecedingAssignment_ResolvesDeclarationInitializer() =>
        FindResult("""
            var items = GetItems();
            Use(items);
            """).Should().BeOfType<PrecedingAssignment.Found>().Which.Assignment.ToString().Should().Be("GetItems()");

    [TestMethod]
    public void FindLinearPrecedingAssignment_ResolvesMostRecentReassignment() =>
        FindResult("""
            var items = GetItems();
            items = GetOther();
            Use(items);
            """).Should().BeOfType<PrecedingAssignment.Found>().Which.Assignment.ToString().Should().Be("GetOther()");

    [TestMethod]
    public void FindLinearPrecedingAssignment_ConditionalReassignment_IsUncertain() =>
        FindResult("""
            var items = GetItems();
            if (flag)
            {
                items = GetOther();
            }
            Use(items);
            """).Should().Be(PrecedingAssignment.Uncertain.Instance);

    [TestMethod]
    public void FindLinearPrecedingAssignment_LoopReassignment_IsUncertain() =>
        FindResult("""
            var items = GetItems();
            for (var i = 0; i < 10; i++)
            {
                items = GetOther();
            }
            Use(items);
            """).Should().Be(PrecedingAssignment.Uncertain.Instance);

    [TestMethod]
    public void FindLinearPrecedingAssignment_NoDeclarationOrAssignment_IsNone() =>
        FindResult(
            """
            Use(items);
            """,
            "object items").Should().Be(PrecedingAssignment.None.Instance);

    [TestMethod]
    public void FindLinearPrecedingAssignment_ShadowedByLocalFunctionParameter_StopsAtBoundary() =>
        FindResult("""
            var items = GetItems();

            void Local(object items)
            {
                Use(items);
            }
            """).Should().Be(PrecedingAssignment.None.Instance);

    [TestMethod]
    public void FindLinearPrecedingAssignment_ShadowedByLambdaParameter_StopsAtBoundary() =>
        FindResult("""
            var items = GetItems();
            Action<object> act = items => Use(items);
            """).Should().Be(PrecedingAssignment.None.Instance);

    private static PrecedingAssignment FindResult(string methodBody, string extraParameters = "") =>
        FindResultCore($$"""
            using System;

            public class Sample
            {
                void M(bool flag{{(extraParameters.Length == 0 ? string.Empty : ", " + extraParameters)}})
                {
                    {{methodBody}}
                }

                object GetItems() => null;
                object GetOther() => null;
                void Use(object x) { }
            }
            """);

    private static PrecedingAssignment FindResultCore(string code)
    {
        var testCode = new SnippetCompiler(code, false, AnalyzerLanguage.CSharp);
        var node = testCode.Nodes<IdentifierNameSyntax>().Last(x => x.ToString() == "items");
        return Finder.FindLinearPrecedingAssignment("items", node);
    }
}
