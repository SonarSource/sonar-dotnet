/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

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
public static class LanguageOptions
{
    // Intermediate values used by multiple properties below. Declared first to ensure correct initialization order.
    private static readonly IEnumerable<ParseOptions> Cs13 = CreateOptions(CSharp13);
    private static readonly IEnumerable<ParseOptions> Cs12 = CreateOptions(CSharp12);
    private static readonly IEnumerable<ParseOptions> Cs11 = CreateOptions(CSharp11);
    private static readonly IEnumerable<ParseOptions> Cs10 = CreateOptions(CSharp10);
    private static readonly IEnumerable<ParseOptions> Cs9 = CreateOptions(CSharp9);
    private static readonly IEnumerable<ParseOptions> Cs8 = CreateOptions(CSharp8);
    private static readonly IEnumerable<ParseOptions> Cs7 = CreateOptions(CSharp7, CSharp7_1, CSharp7_2, CSharp7_3);
    private static readonly IEnumerable<ParseOptions> Vb15 = CreateOptions(VisualBasic15, VisualBasic15_3, VisualBasic15_5);

    public static ImmutableArray<ParseOptions> BeforeCSharp7 { get; } = CreateOptions(CSharp5).Concat(CreateOptions(CSharp6)).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> BeforeCSharp8 { get; } = BeforeCSharp7.Concat(Cs7).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> BeforeCSharp9 { get; } = BeforeCSharp8.Concat(Cs8).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> BeforeCSharp10 { get; } = BeforeCSharp9.Concat(Cs9).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> BeforeCSharp11 { get; } = BeforeCSharp10.Concat(Cs10).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> BeforeCSharp12 { get; } = BeforeCSharp11.Concat(Cs11).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> BeforeCSharp13 { get; } = BeforeCSharp12.Concat(Cs12).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> BeforeCSharp14 { get; } = BeforeCSharp13.Concat(Cs13).FilterByEnvironment();

    public static ImmutableArray<ParseOptions> FromCSharp14 { get; } = CreateOptions(CSharp14).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp13 { get; } = Cs13.Concat(FromCSharp14).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp12 { get; } = Cs12.Concat(FromCSharp13).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp11 { get; } = Cs11.Concat(FromCSharp12).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp10 { get; } = Cs10.Concat(FromCSharp11).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp9 { get; } = Cs9.Concat(FromCSharp10).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp8 { get; } = Cs8.Concat(FromCSharp9).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp7 { get; } = Cs7.Concat(FromCSharp8).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromCSharp6 { get; } = CreateOptions(CSharp6).Concat(FromCSharp7).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> CSharpPreview { get; } = CreateOptions(Preview).ToImmutableArray();

    public static ImmutableArray<ParseOptions> CSharpLatest { get; } = CreateOptions(CS.LanguageVersion.Latest).ToImmutableArray();
    public static ImmutableArray<ParseOptions> VisualBasicLatest { get; } = CreateOptions(VB.LanguageVersion.Latest).ToImmutableArray();

    public static ImmutableArray<ParseOptions> OnlyCSharp7 { get; } = Cs7.FilterByEnvironment();

    public static ImmutableArray<ParseOptions> FromVisualBasic15 { get; } = Vb15.Concat(CreateOptions(VisualBasic16)).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromVisualBasic14 { get; } = CreateOptions(VisualBasic14).Concat(FromVisualBasic15).FilterByEnvironment();
    public static ImmutableArray<ParseOptions> FromVisualBasic12 { get; } = CreateOptions(VisualBasic12).Concat(FromVisualBasic14).FilterByEnvironment();

    private static readonly ImmutableArray<ParseOptions> DefaultParseOptions = FromCSharp7.Concat(FromVisualBasic12).ToImmutableArray(); // Values depends on the build environment

    public static IEnumerable<ParseOptions> OrDefault(this IEnumerable<ParseOptions> parseOptions, string language) =>
        parseOptions is not null && parseOptions.Any() ? parseOptions : Default(language);

    public static IEnumerable<ParseOptions> Default(string language) =>
        DefaultParseOptions.Where(x => x.Language == language);

    public static ImmutableArray<ParseOptions> Between(CS.LanguageVersion from, CS.LanguageVersion to)
    {
        var versions = Between<CS.LanguageVersion>(from, to);
        return CreateOptions(versions).FilterByEnvironment();
    }

    public static ImmutableArray<ParseOptions> Between(VB.LanguageVersion from, VB.LanguageVersion to)
    {
        var versions = Between<VB.LanguageVersion>(from, to);
        return CreateOptions(versions).FilterByEnvironment();
    }

    public static ImmutableArray<ParseOptions> Latest(AnalyzerLanguage language) =>
        language.LanguageName switch
        {
            LanguageNames.CSharp => CSharpLatest,
            LanguageNames.VisualBasic => VisualBasicLatest,
            _ => throw new UnexpectedLanguageException(language)
        };

    private static ImmutableArray<ParseOptions> FilterByEnvironment(this IEnumerable<ParseOptions> options) =>
        TestEnvironment.IsAzureDevOpsContext && !TestEnvironment.IsPullRequestBuild
            ? options.ToImmutableArray()
            : [options.First()]; // Use only the oldest version for local test run and debug

    private static IEnumerable<ParseOptions> CreateOptions(params CS.LanguageVersion[] options) =>
        options.Select(x => new CS.CSharpParseOptions(x));

    private static IEnumerable<ParseOptions> CreateOptions(params VB.LanguageVersion[] options) =>
        options.Select(x => new VB.VisualBasicParseOptions(x));

    private static T[] Between<T>(T from, T to) where T : struct, Enum
    {
        var comparer = Comparer<T>.Default;

        // Enum comparison is based on the documented numeric ordering. See:
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.languageversion
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.visualbasic.languageversion
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Where(x => comparer.Compare(x, from) >= 0 && comparer.Compare(x, to) <= 0)
            .ToArray();
    }
}
