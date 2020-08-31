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

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal static class ParseOptionsHelper
    {
        public static IEnumerable<ParseOptions> BeforeCSharp7 { get; }
        public static IEnumerable<ParseOptions> BeforeCSharp8 { get; }
        public static IEnumerable<ParseOptions> FromCSharp6 { get; }
        public static IEnumerable<ParseOptions> FromCSharp7 { get; }
        public static IEnumerable<ParseOptions> FromCSharp8 { get; }
        public static IEnumerable<ParseOptions> CSharp5 { get; } = CreateOptions(CS.LanguageVersion.CSharp5);
        public static IEnumerable<ParseOptions> CSharp6 { get; } = CreateOptions(CS.LanguageVersion.CSharp6);
        public static IEnumerable<ParseOptions> CSharp7 { get; } = CreateOptions(
                CS.LanguageVersion.CSharp7,
                CS.LanguageVersion.CSharp7_1,
                CS.LanguageVersion.CSharp7_2,
                CS.LanguageVersion.CSharp7_3);
        public static IEnumerable<ParseOptions> CSharp8 { get; } = CreateOptions(CS.LanguageVersion.CSharp8);

        public static IEnumerable<ParseOptions> FromVisualBasic12 { get; }
        public static IEnumerable<ParseOptions> FromVisualBasic14 { get; }
        public static IEnumerable<ParseOptions> FromVisualBasic15 { get; }
        public static IEnumerable<ParseOptions> VisualBasic12 { get; } = CreateOptions(VB.LanguageVersion.VisualBasic12);
        public static IEnumerable<ParseOptions> VisualBasic14 { get; } = CreateOptions(VB.LanguageVersion.VisualBasic14);
        public static IEnumerable<ParseOptions> VisualBasic15 { get; } = CreateOptions(
            VB.LanguageVersion.VisualBasic15,
            VB.LanguageVersion.VisualBasic15_3,
            VB.LanguageVersion.VisualBasic15_5);
        public static IEnumerable<ParseOptions> VisualBasic16 { get; } = CreateOptions(VB.LanguageVersion.VisualBasic16);

        private static readonly IEnumerable<ParseOptions> defaultParseOptions;

#pragma warning disable S3963 // The static fields are dependent between them so the values cannot be set inline
        static ParseOptionsHelper()
        {
            BeforeCSharp7 = CSharp5.Concat(CSharp6);
            BeforeCSharp8 = BeforeCSharp7.Concat(CSharp7);

            FromCSharp8 = CSharp8;
            FromCSharp7 = CSharp7.Concat(FromCSharp8);
            FromCSharp6 = CSharp6.Concat(FromCSharp7);

            FromVisualBasic15 = VisualBasic15.Concat(VisualBasic16);
            FromVisualBasic14 = VisualBasic14.Concat(FromVisualBasic15);
            FromVisualBasic12 = VisualBasic12.Concat(FromVisualBasic14);

            defaultParseOptions = FromCSharp7.Concat(FromVisualBasic12);
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
