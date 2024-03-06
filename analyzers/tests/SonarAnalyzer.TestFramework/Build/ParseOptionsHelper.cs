/*
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

using System.Linq;
using System.Numerics;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using static Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Build;

/// <summary>
/// Returned values depend on the build environment.
/// Local run: Only the latest language version is used to simplify debugging.
/// CI run: All configured versions are returned.
/// </summary>
public static class ParseOptionsHelper
{
    public static ImmutableArray<ParseOptions> BeforeCSharp7 { get; }
    public static ImmutableArray<ParseOptions> BeforeCSharp8 { get; }
    public static ImmutableArray<ParseOptions> BeforeCSharp9 { get; }
    public static ImmutableArray<ParseOptions> BeforeCSharp10 { get; }
    public static ImmutableArray<ParseOptions> BeforeCSharp11 { get; }
    public static ImmutableArray<ParseOptions> BeforeCSharp12 { get; }

    public static ImmutableArray<ParseOptions> FromCSharp6 { get; }
    public static ImmutableArray<ParseOptions> FromCSharp7 { get; }
    public static ImmutableArray<ParseOptions> FromCSharp8 { get; }
    public static ImmutableArray<ParseOptions> FromCSharp9 { get; }
    public static ImmutableArray<ParseOptions> FromCSharp10 { get; }
    public static ImmutableArray<ParseOptions> FromCSharp11 { get; }
    public static ImmutableArray<ParseOptions> FromCSharp12 { get; }
    public static ImmutableArray<ParseOptions> CSharpPreview { get; }

    public static ImmutableArray<ParseOptions> CSharpLatest { get; }
    public static ImmutableArray<ParseOptions> VisualBasicLatest { get; }

    public static ImmutableArray<ParseOptions> OnlyCSharp7 { get; }

    public static ImmutableArray<ParseOptions> FromVisualBasic12 { get; }
    public static ImmutableArray<ParseOptions> FromVisualBasic14 { get; }
    public static ImmutableArray<ParseOptions> FromVisualBasic15 { get; }

    private static readonly ImmutableArray<ParseOptions> DefaultParseOptions;

#pragma warning disable S3963 // The static fields are dependent between them so the values cannot be set inline

    static ParseOptionsHelper()
    {
        var cs11 = CreateOptions(CSharp11);
        var cs10 = CreateOptions(CSharp10);
        var cs9 = CreateOptions(CSharp9);
        var cs8 = CreateOptions(CSharp8);
        var cs7 = CreateOptions(CSharp7, CSharp7_1, CSharp7_2, CSharp7_3);
        var vb15 = CreateOptions(VisualBasic15, VisualBasic15_3, VisualBasic15_5);

        BeforeCSharp7 = CreateOptions(CSharp5).Concat(CreateOptions(CSharp6)).FilterByEnvironment();
        BeforeCSharp8 = BeforeCSharp7.Concat(cs7).FilterByEnvironment();
        BeforeCSharp9 = BeforeCSharp8.Concat(cs8).FilterByEnvironment();
        BeforeCSharp10 = BeforeCSharp9.Concat(cs9).FilterByEnvironment();
        BeforeCSharp11 = BeforeCSharp10.Concat(cs10).FilterByEnvironment();
        BeforeCSharp12 = BeforeCSharp11.Concat(cs11).FilterByEnvironment();

        FromCSharp12 = CreateOptions(CSharp12).FilterByEnvironment();
        FromCSharp11 = cs11.Concat(FromCSharp12).FilterByEnvironment();
        FromCSharp10 = cs10.Concat(FromCSharp11).FilterByEnvironment();
        FromCSharp9 = cs9.Concat(FromCSharp10).FilterByEnvironment();
        FromCSharp8 = cs8.Concat(FromCSharp9).FilterByEnvironment();
        FromCSharp7 = cs7.Concat(FromCSharp8).FilterByEnvironment();
        FromCSharp6 = CreateOptions(CSharp6).Concat(FromCSharp7).FilterByEnvironment();

        OnlyCSharp7 = cs7.FilterByEnvironment();

        FromVisualBasic15 = vb15.Concat(CreateOptions(VisualBasic16)).FilterByEnvironment();
        FromVisualBasic14 = CreateOptions(VisualBasic14).Concat(FromVisualBasic15).FilterByEnvironment();
        FromVisualBasic12 = CreateOptions(VisualBasic12).Concat(FromVisualBasic14).FilterByEnvironment();

        DefaultParseOptions = FromCSharp7.Concat(FromVisualBasic12).ToImmutableArray(); // Values depends on the build environment
        CSharpPreview = CreateOptions(Preview).ToImmutableArray();
        CSharpLatest = CreateOptions(CS.LanguageVersion.Latest).ToImmutableArray();
        VisualBasicLatest = CreateOptions(VB.LanguageVersion.Latest).ToImmutableArray();
    }
#pragma warning restore S3963

    public static IEnumerable<ParseOptions> OrDefault(this IEnumerable<ParseOptions> parseOptions, string language) =>
        parseOptions != null && parseOptions.Any() ? parseOptions : Default(language);

    public static IEnumerable<ParseOptions> Default(string language) =>
        DefaultParseOptions.Where(x => x.Language == language);

    public static ImmutableArray<ParseOptions> Latest(AnalyzerLanguage language) =>
        language.LanguageName switch
        {
            LanguageNames.CSharp => CSharpLatest,
            LanguageNames.VisualBasic => VisualBasicLatest,
            _ => throw new UnexpectedLanguageException(language)
        };

    private static ImmutableArray<ParseOptions> FilterByEnvironment(this IEnumerable<ParseOptions> options) =>
        true
            ? options.ToImmutableArray()
            : ImmutableArray.Create(options.First()); // Use only the oldest version for local test run and debug

    private static IEnumerable<ParseOptions> CreateOptions(params CS.LanguageVersion[] options) =>
        options.Select(x => new CS.CSharpParseOptions(x, preprocessorSymbols: BuildCSharpPreprocessorSymbols(x)));

    private static IEnumerable<ParseOptions> CreateOptions(params VB.LanguageVersion[] options) =>
        options.Select(x => new VB.VisualBasicParseOptions(x, preprocessorSymbols: BuildVisualBasicPreprocessorSymbols(x)));

    private static string[] BuildCSharpPreprocessorSymbols(CS.LanguageVersion version) =>
        [
            .. BuildNetRuntimePreprocessorSymbols(),
            .. BuildLangVersionOrLaterPreprocessorSymbols(version),
            version.ToString().ToUpperInvariant(),
        ];

    private static IEnumerable<KeyValuePair<string, object>> BuildVisualBasicPreprocessorSymbols(VB.LanguageVersion version)
    {
        return [
                .. BuildNetRuntimePreprocessorSymbols().Select(ToValuedSymbol),
                .. BuildLangVersionOrLaterPreprocessorSymbols(version).Select(ToValuedSymbol),
                ToValuedSymbol(version.ToString().ToUpperInvariant()),
            ];

        KeyValuePair<string, object> ToValuedSymbol(string name) => new KeyValuePair<string, object>(name, 1);
    }

    private static IEnumerable<string> BuildLangVersionOrLaterPreprocessorSymbols<T>(T version) where T : struct, Enum =>
        Enum.GetValues(typeof(T))
            .Cast<T>()
            .Where(x => ((IComparable)x).CompareTo(version) <= 0) // Includes Default
            .Select(x => $"{x.ToString().ToUpperInvariant()}_OR_GREATER");

    private static List<string> BuildNetRuntimePreprocessorSymbols()
    {
        var result = new List<string>();
#if NET
        result.Add("NET");
#endif
#if NETFRAMEWORK
        result.Add("NETFRAMEWORK");
#endif
        return result;
    }
}
