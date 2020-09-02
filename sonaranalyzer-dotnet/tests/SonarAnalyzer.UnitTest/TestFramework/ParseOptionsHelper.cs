/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

using static Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using static Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{

    /// <summary>
    /// Returned values depend on the build environment.
    /// Local run: Only the latest language version is used to simplify debugging.
    /// CI run: All configured versions are returned.
    /// </summary>
    internal static class ParseOptionsHelper
    {
        public static ImmutableArray<ParseOptions> BeforeCSharp7 { get; }
        public static ImmutableArray<ParseOptions> BeforeCSharp8 { get; }

        public static ImmutableArray<ParseOptions> FromCSharp6 { get; }
        public static ImmutableArray<ParseOptions> FromCSharp7 { get; }
        public static ImmutableArray<ParseOptions> FromCSharp8 { get; }

        private static ImmutableArray<ParseOptions> FromVisualBasic12 { get; }
        public static ImmutableArray<ParseOptions> FromVisualBasic14 { get; }
        public static ImmutableArray<ParseOptions> FromVisualBasic15 { get; }

        private static readonly ImmutableArray<ParseOptions> defaultParseOptions;

#pragma warning disable S3963 // The static fields are dependent between them so the values cannot be set inline

        static ParseOptionsHelper()
        {
            var cs7 = CreateOptions(CSharp7, CSharp7_1, CSharp7_2, CSharp7_3);
            var vb15 = CreateOptions(VisualBasic15, VisualBasic15_3, VisualBasic15_5);

            BeforeCSharp7 = CreateOptions(CSharp5).Concat(CreateOptions(CSharp6)).FilterByEnvironment();
            BeforeCSharp8 = BeforeCSharp7.Concat(cs7).FilterByEnvironment();

            FromCSharp8 = CreateOptions(CSharp8).FilterByEnvironment();
            FromCSharp7 = cs7.Concat(FromCSharp8).FilterByEnvironment();
            FromCSharp6 = CreateOptions(CSharp6).Concat(FromCSharp7).FilterByEnvironment();

            FromVisualBasic15 = vb15.Concat(CreateOptions(VisualBasic16)).FilterByEnvironment();
            FromVisualBasic14 = CreateOptions(VisualBasic14).Concat(FromVisualBasic15).FilterByEnvironment();
            FromVisualBasic12 = CreateOptions(VisualBasic12).Concat(FromVisualBasic14).FilterByEnvironment();

            defaultParseOptions = FromCSharp7.Concat(FromVisualBasic12).ToImmutableArray(); // Values depends on the build environment
        }
#pragma warning restore S3963

        public static IEnumerable<ParseOptions> GetParseOptionsOrDefault(IEnumerable<ParseOptions> parseOptions) =>
            parseOptions != null && parseOptions.WhereNotNull().Any()
                ? parseOptions.WhereNotNull()
                : defaultParseOptions;

        public static Func<ParseOptions, bool> GetFilterByLanguage(string language) =>
            language switch
            {
                LanguageNames.CSharp => x => x is CS.CSharpParseOptions,
                LanguageNames.VisualBasic => x => x is VB.VisualBasicParseOptions,
                _ => throw new NotSupportedException($"Not supported language '{language}'")
            };

        private static ImmutableArray<ParseOptions> FilterByEnvironment(this IEnumerable<ParseOptions> options) =>
            TestContextHelper.IsAzureDevOpsContext
                ? options.ToImmutableArray()
                : ImmutableArray.Create(options.Last());    // Use only the latest version for local test run and debug

        private static IEnumerable<ParseOptions> CreateOptions(params CS.LanguageVersion[] options) =>
            options.Select(x => new CS.CSharpParseOptions(x));

        private static IEnumerable<ParseOptions> CreateOptions(params VB.LanguageVersion[] options) =>
            options.Select(x => new VB.VisualBasicParseOptions(x));
    }
}
