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

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal static class ParseOptionsHelper
    {
        public static IEnumerable<ParseOptions> BeforeCSharp7 { get; }
        public static IEnumerable<ParseOptions> BeforeCSharp8 { get; }

        public static IEnumerable<ParseOptions> FromCSharp6 { get; }
        public static IEnumerable<ParseOptions> FromCSharp7 { get; }
        public static IEnumerable<ParseOptions> FromCSharp8 { get; }

        public static IEnumerable<ParseOptions> FromVisualBasic12 { get; }
        public static IEnumerable<ParseOptions> FromVisualBasic14 { get; }
        public static IEnumerable<ParseOptions> FromVisualBasic15 { get; }

        private static readonly IEnumerable<ParseOptions> defaultParseOptions;

#pragma warning disable S3963 // The static fields are dependent between them so the values cannot be set inline
        static ParseOptionsHelper()
        {
            var cs7 = CreateOptions(CSharp7, CSharp7_1, CSharp7_2, CSharp7_3);
            var vb15 = CreateOptions(VisualBasic15, VisualBasic15_3, VisualBasic15_5);

            BeforeCSharp7 = CreateOptions(CSharp5).Concat(CreateOptions(CSharp6));
            BeforeCSharp8 = BeforeCSharp7.Concat(cs7);

            FromCSharp8 = CreateOptions(CSharp8);
            FromCSharp7 = cs7.Concat(FromCSharp8);
            FromCSharp6 = CreateOptions(CSharp6).Concat(FromCSharp7);

            FromVisualBasic15 = vb15.Concat(CreateOptions(VisualBasic16));
            FromVisualBasic14 = CreateOptions(VisualBasic14).Concat(FromVisualBasic15);
            FromVisualBasic12 = CreateOptions(VisualBasic12).Concat(FromVisualBasic14);

            defaultParseOptions = FromCSharp7.Concat(FromVisualBasic12); // List of values depends on the build environment
        }
#pragma warning restore S3963

        public static IEnumerable<ParseOptions> GetParseOptionsOrDefault(IEnumerable<ParseOptions> parseOptions) =>
            parseOptions != null && parseOptions.WhereNotNull().Any()
                ? parseOptions.WhereNotNull()
                : defaultParseOptions;

        public static Func<ParseOptions, bool> GetFilterByLanguage(string language) =>
            language switch
            {
                LanguageNames.CSharp => x => x is VB.VisualBasicParseOptions,
                LanguageNames.VisualBasic => x => x is CS.CSharpParseOptions,
                _ => throw new NotSupportedException($"Not supported language '{language}'")
            };

        private static IEnumerable<ParseOptions> CreateOptions(params CS.LanguageVersion[] options) =>
            options.Select(x => new CS.CSharpParseOptions(x)).ToImmutableArray();

        private static IEnumerable<ParseOptions> CreateOptions(params VB.LanguageVersion[] options) =>
            options.Select(x => new VB.VisualBasicParseOptions(x)).ToImmutableArray();
    }
}
