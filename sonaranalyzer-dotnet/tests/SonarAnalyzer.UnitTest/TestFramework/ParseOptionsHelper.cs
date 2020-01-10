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
                ? parseOptions
                : defaultParseOptions;

        public static IEnumerable<ParseOptions> GetParseOptionsByFileExtension(string extension) =>
            defaultParseOptions.Where(GetFilterByFileExtension(extension));

        public static Func<ParseOptions, bool> GetFilterByLanguage(string language)
        {
            if (language == LanguageNames.CSharp)
            {
                return CSharpFilter;
            }

            if (language == LanguageNames.VisualBasic)
            {
                return VisualBasicFilter;
            }

            throw new NotSupportedException($"Not supported language '{language}'");
        }

        public static Func<ParseOptions, bool> GetFilterByFileExtension(string extension)
        {
            if (extension == ".cs")
            {
                return CSharpFilter;
            }

            if (extension == ".vb")
            {
                return VisualBasicFilter;
            }

            throw new NotSupportedException($"Not supported language extension '{extension}'");
        }

        private static readonly Func<ParseOptions, bool> VisualBasicFilter = (options) => options is VB.VisualBasicParseOptions;

        private static readonly Func<ParseOptions, bool> CSharpFilter = (options) => options is CS.CSharpParseOptions;

        public static IEnumerable<ParseOptions> FromCSharp6 { get; }

        public static IEnumerable<ParseOptions> FromCSharp7 { get; }

        public static IEnumerable<ParseOptions> FromCSharp8 { get; }

        public static IEnumerable<ParseOptions> BeforeCSharp7 { get; }

        public static IEnumerable<ParseOptions> BeforeCSharp8 { get; }

        public static IEnumerable<ParseOptions> CSharp5 { get; } =
            ImmutableArray.Create(new CS.CSharpParseOptions(CS.LanguageVersion.CSharp5));

        public static IEnumerable<ParseOptions> CSharp6 { get; } =
            ImmutableArray.Create(new CS.CSharpParseOptions(CS.LanguageVersion.CSharp6));

        public static IEnumerable<ParseOptions> CSharp7 { get; } =
            ImmutableArray.Create(
                new CS.CSharpParseOptions(CS.LanguageVersion.CSharp7),
                new CS.CSharpParseOptions(CS.LanguageVersion.CSharp7_1),
                new CS.CSharpParseOptions(CS.LanguageVersion.CSharp7_2),
                new CS.CSharpParseOptions(CS.LanguageVersion.CSharp7_3));

        public static IEnumerable<ParseOptions> CSharp8 { get; } =
            ImmutableArray.Create(new CS.CSharpParseOptions(CS.LanguageVersion.CSharp8));

        public static IEnumerable<ParseOptions> FromVisualBasic12 { get; }

        public static IEnumerable<ParseOptions> FromVisualBasic14 { get; }

        public static IEnumerable<ParseOptions> FromVisualBasic15 { get; }

        public static IEnumerable<ParseOptions> VisualBasic12 { get; } =
            ImmutableArray.Create(new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic12));

        public static IEnumerable<ParseOptions> VisualBasic14 { get; } =
            ImmutableArray.Create(new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic14));

        public static IEnumerable<ParseOptions> VisualBasic15 { get; } =
            ImmutableArray.Create(
                new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic15),
                new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic15_3),
                new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic15_5));

        public static IEnumerable<ParseOptions> VisualBasic16 { get; } =
            ImmutableArray.Create(new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic16));
    }
}
