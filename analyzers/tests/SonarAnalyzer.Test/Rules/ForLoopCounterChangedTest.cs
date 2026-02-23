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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ForLoopCounterChangedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ForLoopCounterChanged>();

    [TestMethod]
    public void ForLoopCounterChanged() =>
        builder.AddPaths("ForLoopCounterChanged.cs").Verify();

    [TestMethod]
    public void ForLoopCounterChanged_CS_Latest() =>
        builder.AddPaths("ForLoopCounterChanged.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [CombinatorialDataTestMethod]
    public void ForLoopCounterChanged_VariableUsage(
        [DataValues(true, false)] bool inInitializer,
        [DataValues(true, false)] bool inCondition,
        [DataValues(true, false)] bool inIncrementor)
    {
        var initializer = inInitializer ? "i = 0" : string.Empty;
        var condition = inCondition ? "i < 10" : string.Empty;
        var incrementor = inIncrementor ? "i++" : string.Empty;
        var expectedRaise = inCondition && inIncrementor;
        var noncompliant = expectedRaise ? " // Noncompliant" : string.Empty;

        var declaration = inInitializer ? string.Empty : "int i = 0;";
        var verifier = builder.AddSnippet($$"""
            class C
            {
                void M()
                {
                    {{declaration}}
                    for ({{(inInitializer ? "int i = 0" : initializer)}}; {{condition}}; {{incrementor}})
                    {
                        i = 5;{{noncompliant}}
                    }
                }
            }
            """);
        if (expectedRaise)
        {
            verifier.Verify();
        }
        else
        {
            verifier.VerifyNoIssues();
        }
    }
}
