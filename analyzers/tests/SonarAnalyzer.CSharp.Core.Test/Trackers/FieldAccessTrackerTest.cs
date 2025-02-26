﻿/*
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Trackers.Test;

[TestClass]
public class FieldAccessTrackerTest
{
    [TestMethod]
    public void MatchSet_CS()
    {
        var tracker = new CSharpFieldAccessTracker();
        var context = CreateContext("assignConst");
        tracker.MatchSet()(context).Should().BeTrue();

        context = CreateContext("read");
        tracker.MatchSet()(context).Should().BeFalse();
    }

    [TestMethod]
    public void AssignedValueIsConstant_CS()
    {
        var tracker = new CSharpFieldAccessTracker();
        var context = CreateContext("assignConst");
        tracker.AssignedValueIsConstant()(context).Should().BeTrue();

        context = CreateContext("assignVariable");
        tracker.AssignedValueIsConstant()(context).Should().BeFalse();

        context = CreateContext("invocationArg");
        tracker.AssignedValueIsConstant()(context).Should().BeFalse();
    }

    private static FieldAccessContext CreateContext(string fieldName)
    {
        const string code = """
            public class Sample
            {
                private int assignConst;
                private int assignVariable;
                private int read;
                private int invocationArg;

                private void Usage()
                {
                    var x = read;
                    assignConst = 42;
                    assignVariable = x;
                    Method(invocationArg);
                }

                private void Method(int arg) { }
            }
            """;
        var testCode = new SnippetCompiler(code, false, AnalyzerLanguage.CSharp);
        var node = testCode.GetNodes<IdentifierNameSyntax>().First(x => x.ToString() == fieldName);
        return new FieldAccessContext(testCode.CreateAnalysisContext(node), fieldName);
    }
}
