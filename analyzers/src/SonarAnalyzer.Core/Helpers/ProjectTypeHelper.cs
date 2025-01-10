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

using System.Runtime.CompilerServices;

namespace SonarAnalyzer.Helpers;

internal static class ProjectTypeHelper
{
    // This list is duplicated in sonar-scanner-msbuild and sonar-security and should be manually synchronized after each change.
    public /* for testing */ static readonly ISet<string> TestAssemblyNames = new HashSet<string>
    {
        "dotMemory.Unit",
        "Microsoft.VisualStudio.TestPlatform.TestFramework",
        "Microsoft.VisualStudio.QualityTools.UnitTestFramework",
        "Machine.Specifications",
        "nunit.framework",
        "nunitlite",
        "TechTalk.SpecFlow",
        "xunit", // Legacy Xunit (v1.x)
        "xunit.core",
        // Assertion
        "FluentAssertions",
        "Shouldly",
        // Mock
        "FakeItEasy",
        "Moq",
        "NSubstitute",
        "Rhino.Mocks",
        "Telerik.JustMock"
    };

    private static readonly ConditionalWeakTable<Compilation, IsTestWrapper> Cache = new ConditionalWeakTable<Compilation, IsTestWrapper>();

    // Should only be used by SonarAnalysisContext
    public static bool IsTest(this Compilation compilation) =>
        // We can't detect references => it's MAIN
        compilation != null && Cache.GetValue(compilation, x => new IsTestWrapper(x)).Value;

    private class IsTestWrapper
    {
        public readonly bool Value;

        public IsTestWrapper(Compilation compilation) =>
            Value = compilation.ReferencedAssemblyNames.Any(x => TestAssemblyNames.Contains(x.Name));
    }
}
