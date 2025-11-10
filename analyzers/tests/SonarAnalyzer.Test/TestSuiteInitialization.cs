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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test;

[TestClass]
public class TestSuiteInitialization
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        ConfigureFluentValidation();

        Console.WriteLine(@"Running tests initialization...");
        Console.WriteLine(@$"Build reason: {TestEnvironment.BuildReason() ?? "Not set / Local build"}");

        var csVersions = LanguageOptions.Default(LanguageNames.CSharp).Cast<CSharpParseOptions>().Select(x => x.LanguageVersion.ToDisplayString());
        Console.WriteLine(@"C# versions used for analysis: " + string.Join(", ", csVersions));

        var vbVersions = LanguageOptions.Default(LanguageNames.VisualBasic).Cast<VisualBasicParseOptions>().Select(x => x.LanguageVersion.ToDisplayString());
        Console.WriteLine(@"VB.Net versions used for analysis: " + string.Join(", ", vbVersions));
    }

    private static void ConfigureFluentValidation()
    {
        AssertionOptions.FormattingOptions.MaxLines = -1;
        AssertionOptions.FormattingOptions.MaxDepth = 5; // Keeping the default for MaxDepth of 5 as a good compromise. Change it here if needed.
    }
}
