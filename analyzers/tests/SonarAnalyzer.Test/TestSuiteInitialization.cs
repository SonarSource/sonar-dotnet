﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.Test.Helpers;

namespace SonarAnalyzer.Test
{
    [TestClass]
    public class TestSuiteInitialization
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ConfigureFluentValidation();

            Console.WriteLine(@"Running tests initialization...");
            Console.WriteLine(@$"Build reason: {TestContextHelper.BuildReason() ?? "Not set / Local build"}");

            var csVersions = ParseOptionsHelper.Default(LanguageNames.CSharp).Cast<CSharpParseOptions>().Select(x => x.LanguageVersion.ToDisplayString());
            Console.WriteLine(@"C# versions used for analysis: " + string.Join(", ", csVersions));

            var vbVersions = ParseOptionsHelper.Default(LanguageNames.VisualBasic).Cast<VisualBasicParseOptions>().Select(x => x.LanguageVersion.ToDisplayString());
            Console.WriteLine(@"VB.Net versions used for analysis: " + string.Join(", ", vbVersions));
        }

        private static void ConfigureFluentValidation()
        {
            AssertionOptions.FormattingOptions.MaxLines = -1;
            AssertionOptions.FormattingOptions.MaxDepth = 5; // Keeping the default for MaxDepth of 5 as a good compromise. Change it here if needed.
        }
    }
}
